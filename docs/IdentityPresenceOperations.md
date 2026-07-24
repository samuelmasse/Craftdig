# Identity and Presence Operations

This release is one breaking protocol cutover. It has no legacy ticket endpoint, compatibility
mode, save migrator, or old player-ID rewrite path.

## Before rollout

1. Back up anything an operator wants to retain, then create a new multiplayer world. Worlds made
   before this feature are unsupported and may be deleted.
2. Configure every public TLS host and port for which the server accepts Identity tickets. The
   value must match exactly what a player enters.
3. Keep raw TCP disabled in production. Raw TCP is development-only and can use only no-auth,
   visibly unverified identities.
4. Deploy `craftdig.io`, the game server, and the client as one coordinated protocol cutover.

Example server configuration:

```ini
PublicServer = true
DisableTls = false
EnableRawTcp = false
MaxPlayers = 100
PublicServerContexts:0 = play.example.com:36676
IdentityJwksUrl = https://craftdig.io/.well-known/jwks.json
PresenceEgressBytesPerSecond = 20000000
```

`PublicServerContexts` is an allowlist, not a redirect. For example, a ticket for
`play.example.com:36676` is rejected by a server configured only for `127.0.0.1:36676`.
DNS connections require a publicly trusted certificate matching the selected name. Literal IP
connections accept certificate name and chain errors so the generated server certificate works with
simple port forwarding; their traffic is encrypted, but the server's identity is not authenticated.
Use a DNS name and publicly trusted certificate when that authentication is required.
Private-server player allowlists must contain canonical `playerId` UUIDs. Usernames are nonunique
display labels and never grant access.

## Deployment order

1. Build and test both repositories.
2. Deploy the sole strict `POST /api/GetToken` contract and one-key JWKS publisher from
   `craftdig.io`.
3. Confirm `https://craftdig.io/.well-known/jwks.json` contains the existing `craftdig-key-1` public key.
4. Start the updated game server on a clean world with its exact public context configured.
5. Release or start updated clients. Old clients and servers are intentionally incompatible.

Before step 2, use `cdk list --long` and `cdk diff --no-change-set` to confirm the exact AWS
account, stack region, subdomain, and certificate. The CDK certificate must cover the resulting
domain, and a CloudFront viewer certificate must be issued in `us-east-1`. Do not deploy if the
diff includes an unintended certificate, domain, region, or frontend change.

No deployment is performed by the implementation work itself. Deploying `craftdig.io` changes an
external production service and remains an explicit operator action.

## Signing-key publication

Identity keeps the existing `JwtKeyPairGenerator` resource and the permanent `craftdig-key-1`
signing key. There is no environment-variable trigger, update-triggered rotation, retirement
metadata, or overlap mechanism. Ordinary deployments do not regenerate or replace the key.

When creating a completely fresh stack, the publication Lambda:

1. generates the permanent RS256 key;
2. uploads a standard JWKS containing exactly that one public key;
3. invalidates the exact CloudFront JWKS path and waits for completion;
4. activates the matching private key only after the public key is available.

Do not manually replace the secret or JWKS: this release has no old/new-key overlap, so doing so
would invalidate in-flight tickets. A future key replacement requires a separately reviewed
publication and overlap design.

After the cutover, confirm `/.well-known/jwks.json` contains exactly one public RS256 key and that a
new ticket uses its `kid`. Never log or copy the private-key secret.

## Identity outage behavior

- Already authenticated clients continue until their installed ticket expires.
- Ticket refresh and new authenticated joins fail closed while Identity or JWKS is unavailable.
- A client is disconnected when its installed ticket expires; it does not remain admitted forever.
- Cached public keys may be used only for the bounded stale-key window during a transient JWKS
  outage. An unknown key is never trusted from stale data.
- Do not switch production to raw/no-auth as an outage workaround. If no-auth is intentionally used
  for local development, its roster and nameplates remain explicitly unverified.

## Presence capacity and monitoring

Presence owns a separate server thread; the game tick does not parse tickets, hash rounds, verify
signatures, or build proof batches.

The fixed v1 schedule is:

- one round every 10 seconds;
- client challenges every 5 seconds with stable jitter;
- round chunks paced during the first 4 seconds;
- proof output begins after 1 second and is batched throughout the round;
- a 20 MB/s default presence token budget;
- disconnect after three output-backpressure strikes.

At 1,000 players, deterministic protocol modeling gives about 154.5 MB per round, or 15.45 MB/s
before TLS and socket overhead. Identity ticket refresh fanout adds load and must remain jittered.
Treat 1,000 players as a modeled ceiling until the live TLS/CPU/allocation benchmark is completed.
Lower `MaxPlayers` if measured traffic or CPU cannot stay inside the configured budget.

The checked-in deterministic performance test currently reports, on the development machine, a
1,000-player round encoded in about 0.33 ms with about 159 KB allocated, 1,000 P-256 verifications
at about 10,300/s, and a 1,000-challenge/two-round-proof inbox burst in about 3.7 ms with about
1.82 MB allocated. Its combined framed-payload and modeled ticket-refresh estimate is 17.17 MB/s.
These are regression guards, not substitutes for the live TLS, socket, UI, and game-tick check.

Monitor these non-sensitive counters: active sessions, lifecycle/challenge/proof queue depth, round
age and challenge count, accepted/rejected challenges and proofs, bytes queued, backpressure events,
and slow-socket disconnects. Investigate sustained queue growth, round age over 10 seconds, round
chunks not delivered by 4 seconds, or recurring slow-socket disconnects. Logs and metrics must not
contain Google tokens, compact Identity tickets, private keys, nonces, or complete signatures.

## Security diagnostics

Normal client and server logs intentionally cover only actionable boundaries: connection and
authentication outcomes, development transport warnings, malformed security frames and disconnect
reasons, Identity ticket/JWKS/refresh failures, worker failures, and the server's 60-second aggregate
presence metrics. Server ticket rejection includes a safe stage such as
`ServerContextNotAllowed`, `SigningKeyUnavailable`, or `TicketSignature`.

For local transport diagnosis, `LogLevel = Trace` additionally reports command type, command ID,
and byte count. It does not log command payloads. Per-challenge, per-proof, per-player verification,
ticket-cache, and routine scheduling events are deliberately omitted.

The Identity Lambdas log the API action name, sanitized uncaught-error type, cold-start readiness,
and signing-key publication lifecycle. They do not log request bodies, Authorization headers,
Google tokens, compact JWTs, nonce values, public/private key coordinates, or signature values.

## Manual two-client acceptance check

Use two signed-in clients, Alice and Bob, against one TLS server:

1. Alice and Bob join and each appears once. Their network player Ent IDs must equal their ticket
   `sub` UUIDs.
2. Hold Tab on Bob. Alice transitions from `Pending` to `Verified` after a complete round and proof.
3. Alice's nameplate uses the ticket username, not a server-provided name.
4. Keep Bob outside Alice's streamed chunks. Alice remains in Bob's global roster.
5. Disconnect Alice. Bob's prior verification becomes stale or disappears without any server claim
   extending it.
6. Leave both clients connected through a ticket refresh. Neither player respawns or gains a second
   roster entry.
7. Try a local raw/no-auth client. It must be labeled development/unverified and must never become
   `Verified`.

Also test a wrong configured host or port: authentication must fail rather than silently accepting a
different context.

## Rollback

Before player rollout, rollback is the normal deployment rollback plus restoring the prior world
backup if desired. The Identity deployment retains the existing raw-PEM signing secret and JWKS,
so it does not require a signing-key rollback. The implementation intentionally contains no ticket
or protocol compatibility branch. After the breaking cutover creates or changes a new world, do
not mix old and new clients/servers or try to migrate player Ent IDs backward. Stop the server,
preserve the new world for investigation, and roll the full client/server/Identity set together.
