# Epic: Verifiable Multiplayer Identity and Presence

## Status

- Status: Implementation complete; Identity deployed; local multiplayer smoke test reported passing; coordinated game rollout and live capacity validation pending
- Primary repository: `Craftdig`
- Identity repository: `craftdig.io`
- Protocol version: 1
- Breaking multiplayer protocol change: Yes
- Target implementation order: Identity replacement, protocol primitives, admission, clean-world player-ID model, presence service, client verification, UI, rollout
- Automated verification: 89 security tests pass (26 protocol, 19 server, 15 client, 19 shared Craftdig.Identity, 10 Identity service); deterministic N=20/100/1,000 load and real-socket backpressure tests pass
- Remaining verification: run the signed-in two-client TLS acceptance check, measure live game-tick/presence-thread/TLS behavior on the deployment target, set the production capacity from those measurements, and roll out the game server and clients together

## Summary

Implement end-to-end, viewer-verifiable multiplayer identity while treating the game server as potentially malicious.

Every authenticated account has one public `playerId`. That same UUID is:

- the Craftdig account ID in the Identity ticket;
- the persistent world-profile ID;
- the network-visible dimension player Ent ID; and
- the client cache key for tickets, verified presence, nameplates, and the player list.

There is no separate public avatar ID and no client-visible world-player-to-avatar mapping. A per-connection `sessionId` and temporary P-256 key prevent ticket theft and old-session replay. Every viewer supplies fresh random challenges, players sign one common flat challenge round, and each viewer verifies the resulting evidence locally.

The Identity service only issues short-lived tickets and publishes signing keys. It does not receive presence challenges, contact game servers, inspect sessions, or learn whether Bob verified Alice.

All ongoing server presence work runs outside the game tick in a dedicated, single-owner presence service with bounded queues and paced fanout.

## Motivation

The current flow authenticates a client only to the game server. Remote clients receive a server-created player Ent but no signed account identity, ticket, proof of possession, or account-to-Ent binding. A malicious server can therefore invent names, impersonate accounts to other players, replay old authentication material, or attach an authenticated name to an arbitrary Ent.

The current Identity ticket is also a bearer token bound only to an arbitrary host string and server nonce. It has no port, client session, or proof-of-possession key. Craftdig servers validate its signature and lifetime but do not enforce issuer, audience, type, algorithm, host, or maximum lifetime.

This epic replaces that model with a short-lived Identity ticket bound to an ephemeral client key and exact server context, followed by fresh viewer-verifiable presence signatures.

## Goals

- Make `playerId` the only authenticated player/avatar identity.
- Let Bob independently verify that the holder of Alice's ticket-bound session key recently participated in Bob's server context.
- Prevent a stolen Identity ticket from being useful without the corresponding temporary private key.
- Prevent cross-host and cross-port ticket replay.
- Bound replay after disconnect or session loss using Bob's local monotonic time.
- Keep all cryptography, HTTP, ticket parsing, and batch assembly off render and game-tick threads.
- Support 20, 100, and 1,000 connected players with bounded work and paced output.
- Keep the Identity service stateless after ticket issuance.
- Preserve an explicit development/no-auth mode without presenting it as verified identity.
- Make the new identity protocol a clean-world cutoff; pre-feature worlds are not supported and may be deleted.

## Non-goals

- Proving that the human Alice personally authored an action.
- Proving that Alice remains connected at the exact instant Bob renders a frame.
- Proving movement, combat, inventory, chat, ping, or world state against a malicious server.
- Preventing the server from omitting players, partitioning viewers, delaying traffic, or denying service.
- Proving that two viewers received the same global roster.
- Signing every gameplay input or movement packet.
- Supporting multiple simultaneous bodies, corpses, possessed entities, or overlapping dimension incarnations for one `playerId` in protocol v1.
- Adding verified skin assets in v1. A later ticket version may add an asset ID and content hash.
- Making usernames the security identifier. `playerId` is authoritative; usernames are display labels.

## Threat model and guarantee

### Trusted

- Craftdig Identity's signing key and Google-to-Craftdig account mapping.
- Correct WebPKI TLS validation by clients when connecting to a DNS host. A direct-IP connection
  uses encrypted but server-unverified TLS so a private server does not require a public certificate.
- SHA-256 and P-256 ECDSA.
- A client's temporary private key while that client process remains uncompromised.
- Bob's local cryptographic implementation and monotonic clock.

### Untrusted

- The game server and server operator.
- Other clients.
- Server timestamps, roster claims, ping values, Ent movement, and world state.
- Tickets, proofs, and keys delivered through the game server until locally verified.

### Precise v1 guarantee

At Bob's local time `T`, if Alice is `Verified`, Bob has:

1. a valid Craftdig Identity ticket binding Alice's `playerId`, display name, server context, `sessionId`, and temporary public key; and
2. a valid signature from that temporary key over the ticket and a challenge round containing a 256-bit nonce Bob generated at local time `T0`, where `T <= T0 + 15 seconds`.

Bob may conclude:

> The holder of the temporary private key certified for this Craftdig player ID signed a round containing Bob's fresh challenge for this server context within Bob's 15-second freshness window.

Bob may not conclude that Alice authored the avatar's movement or that the malicious server displayed or applied gameplay honestly.

## Accepted design decisions

1. `playerId` is the Craftdig UUID from ticket `sub`.
2. A network-visible player Ent must have `Ent.Id == playerId`.
3. There is exactly one active player Ent and one active session per `playerId` on a server.
4. The persistent world profile remains server-side and uses the same `playerId` in a separate arena.
5. New multiplayer dimension player Ents do not store a `WorldPlayer` reference; server systems resolve the profile by `player.Id`. Pre-feature multiplayer worlds are intentionally unsupported.
6. Identity tickets and verified profiles use direct protocol messages and client caches, not synchronized world-profile Ents.
7. A client generates a new `sessionId` and temporary P-256 key for every connection or reconnect.
8. Identity tickets last at most 15 minutes and are refreshed around 10 minutes with jitter.
9. Presence rounds occur every 10 seconds; clients submit challenges every 5 seconds with stable jitter.
10. Bob's maximum presence-proof age is 15 seconds, measured from Bob's local challenge creation time.
11. Presence uses a flat sorted challenge list and ordinary message chunks. There are no Merkle trees, roster roots, or inclusion paths in v1.
12. Every client signs once per round and verifies at most one proof per advertised ticket.
13. Bob's verification result is private local state. There is no verification-result command or acknowledgement.
14. Usernames may be nonunique. UI disambiguation uses `playerId`; server duplicate-session logic uses only `playerId`.
15. Exposing the stable Craftdig `playerId` to other players is accepted for v1. Per-server pseudonymous IDs require a later privacy design.

## Cryptographic suite and canonical encoding

- Identity issuer signature: RS256, using the existing Identity key infrastructure.
- Client proof-of-possession key: P-256.
- Client signature hash: SHA-256.
- Client signature encoding: fixed 64-byte IEEE P1363 `R || S`, each integer 32-byte big-endian.
- General hash: SHA-256.
- Server nonce: 32 random bytes.
- Viewer nonce: 32 random bytes.
- `sessionId`: UUID v4, encoded as canonical RFC 9562 network bytes in signed data.
- Integers in signed data: unsigned big-endian.
- Text in signed data: UTF-8 after the canonicalization rules below.

Never hash or sign raw `MemoryMarshal` output. .NET `Guid` memory layout and native integer endianness are not the canonical cryptographic encoding. Add explicit canonical writers and cross-language golden vectors.

The five new identity/presence commands must also use dedicated exact-length binary codecs rather than the current generic native-struct marshalling path. Register their IDs through raw handlers, parse unsigned big-endian integers and RFC 9562 UUID bytes explicitly, reject trailing bytes, and cap counts before allocation. Existing non-security commands may retain their current transport encoding during this epic.

When an input below is already defined as a 32-byte digest, use `SignHash`/`VerifyHash` with that digest. Do not accidentally hash the digest a second time by passing it to a `SignData` overload.

### Server context

The default server context is the host and explicit port selected by the client. DNS hosts are
TLS-authenticated; literal IP hosts identify an encrypted but server-unverified connection:

```text
serverContextBytes =
    "Craftdig Server Context v1\0" ||
    hostKind:u8 ||
    hostLength:u16be ||
    canonicalHostBytes ||
    port:u16be

serverContextHash = SHA256(serverContextBytes)
```

Canonical host rules:

- DNS: lowercase IDNA A-label form, no trailing dot, UTF-8 bytes.
- IPv4: four raw network-order bytes.
- IPv6: sixteen raw network-order bytes.
- URI schemes, paths, query strings, user info, and implicit/default ports are forbidden.
- The game server has an explicit configured allowlist of public host-and-port contexts it accepts.
- Identity validates and signs the canonical structured host and port but does not perform DNS lookups or assert server ownership.

Certificate and SPKI pinning are not part of v1. DNS connections use WebPKI host validation, which
permits ordinary certificate renewal and key rotation. Literal IP connections accept name and chain
errors so friends can use the server's generated certificate with simple port forwarding; use a DNS
name and publicly trusted certificate when server identity authentication is required.

## Identity ticket protocol

### Client to Identity

The authenticated client calls `POST /api/GetToken` using its Google ID token as the Bearer credential.

Target request:

```json
{
  "version": 1,
  "server": {
    "host": "play.example.com",
    "port": 36676
  },
  "sessionId": "6e355d14-5d89-47dd-9b12-962e6679e750",
  "publicKey": {
    "kty": "EC",
    "crv": "P-256",
    "x": "base64url-32-byte-x",
    "y": "base64url-32-byte-y"
  }
}
```

The request does not contain a game-server nonce. The later client signature over `ReadyAuthCommand`'s nonce provides connection freshness.

Identity must reject unknown fields, private JWK field `d`, non-P-256 keys, invalid curve points, padded or incorrectly sized coordinates, invalid UUIDs, noncanonical hosts, and ports outside `1..65535`.

### Identity to client

The response remains:

```json
{
  "jwt": "eyJ..."
}
```

Protected header:

```json
{
  "alg": "RS256",
  "kid": "active-versioned-key-id",
  "typ": "craftdig-multiplayer-ticket+jwt"
}
```

Claims:

```json
{
  "iss": "craftdig-auth",
  "aud": "craftdig:multiplayer:v1",
  "sub": "11111111-1111-1111-1111-111111111111",
  "username": "Alice",
  "ver": 1,
  "server": {
    "host": "play.example.com",
    "port": 36676
  },
  "sid": "6e355d14-5d89-47dd-9b12-962e6679e750",
  "cnf": {
    "jwk": {
      "kty": "EC",
      "crv": "P-256",
      "x": "base64url-32-byte-x",
      "y": "base64url-32-byte-y"
    }
  },
  "jti": "random-ticket-uuid",
  "iat": 1784232000,
  "nbf": 1784232000,
  "exp": 1784232900
}
```

The client and server enforce:

- exact `alg`, `typ`, `iss`, `aud`, and `ver`;
- known `kid` from Craftdig JWKS;
- valid signature;
- required claims and canonical formats;
- `exp - iat <= 900 seconds`;
- at most 30 seconds of clock skew;
- exact requested server context, `sessionId`, and public key;
- valid UUID `sub`, `sid`, and `jti`;
- no duplicate or ambiguous required claims.

The raw compact JWT bytes are retained without reserialization:

```text
ticketHash = SHA256(exact compact JWT ASCII bytes)
```

## Game protocol command inventory

Existing command IDs must be frozen. New enum entries are appended with explicit numeric assignments; no existing implicit value may shift.

### Modified existing commands

| ID | Command | Direction | Change |
|---:|---|---|---|
| 20001 | `BeginAuthCommand` | Client to server | Empty wire format remains. Permit both initial authentication and authenticated ticket refresh when no nonce is already pending. |
| 30001 | `ReadyAuthCommand` | Server to client | Payload changes from a UTF-8 hexadecimal nonce to exactly 32 raw random bytes. |
| 20002 | `CompleteAuthCommand` | Client to server | Payload changes from raw JWT to `ticketBytes[N] || signature[64]`; ticket length is payload length minus 64. |
| 20003 | `ResultAuthCommand` | Server to client | Remains an empty success response for initial auth or refresh. |

`CompleteAuthCommand`'s client signature is:

```text
authDigest = SHA256(
    "Craftdig authentication v1\0" ||
    serverContextHash ||
    serverNonce ||
    ticketHash
)
```

On initial auth, the server installs the ticket/session after strict validation. On refresh, it additionally requires unchanged `sub`, `sid`, `cnf.jwk`, and server context, a newer ticket issuance time, and no expired current connection.

### New commands

| ID | Command | Direction | Use |
|---:|---|---|---|
| 20011 | `PresenceChallengeCommand` | Client to server | Submit one viewer freshness challenge. |
| 20012 | `PresenceProofCommand` | Client to server | Submit one ticket-key signature for a completed round. |
| 30017 | `PlayerIdentityCommand` | Server to client | Deliver or update one raw Identity ticket. |
| 30018 | `PresenceRoundChunkCommand` | Server to client | Deliver the complete flat challenge round in bounded chunks. |
| 30019 | `PresenceProofBatchCommand` | Server to client | Stream zero or more proof records for a round as they become available. |

There is deliberately no leave command, roster-complete command, verification-result command, or server-signed roster command.

### `PlayerIdentityCommand`

- Header: empty.
- Payload: exact compact JWT bytes.
- Maximum ticket payload: 3,072 bytes.
- Sent to all authenticated clients when a session joins or refreshes.
- Sent once per existing active ticket when a new viewer completes authentication.
- Treated as an upsert keyed by `ticketHash` and `playerId` after local verification.
- Emitted through the presence service's single output scheduler so ticket and proof ordering is deterministic.
- A new viewer receives the paced ticket snapshot before the presence service begins proof fanout to that viewer.

The server may omit or corrupt this message; clients then display no verified identity.

### `PresenceChallengeCommand`

```text
header:
    sequence:u64

payload:
    nonce[32]
```

The server derives the viewer `sessionId` from the socket's installed Identity session. For no-auth viewers, the server assigns an untrusted per-connection session ID so they may verify authenticated peers, but no-auth clients never produce a verified proof of their own.

The server retains only the newest unexpired challenge per connection.

`sequence` must strictly increase within a session. Duplicate or decreasing sequences are ignored or rejected without replacing the newest accepted challenge.

### Presence challenge record

```text
sessionId[16] || sequence:u64be || nonce[32]
```

Each canonical record is 56 bytes. Records are sorted lexicographically by their complete canonical bytes. Duplicate session IDs, duplicate complete records, invalid lengths, and counts above the configured player maximum invalidate the round.

### Round hash

```text
roundHash = SHA256(
    "Craftdig presence round v1\0" ||
    challengeCount:u32be ||
    concatenatedSortedChallengeRecords
)
```

### `PresenceRoundChunkCommand`

```text
header:
    roundHash[32]
    chunkIndex:u16
    chunkCount:u16
    totalChallengeCount:u32

payload:
    whole PresenceChallengeRecord values
```

- Chunks contain only complete 56-byte records.
- All chunks for a round must agree on hash, count, and chunk total.
- Clients accept each chunk index once and cap the total assembled bytes.
- Records must already be in canonical order across chunk boundaries; clients reject out-of-order or duplicate session records rather than silently normalizing malformed input.
- The client signs only after assembling and hashing the full list and finding one of its own still-pending challenges exactly once.
- At most one current round and one draining previous round are retained.

### Presence proof digest

```text
proofDigest = SHA256(
    "Craftdig presence proof v1\0" ||
    serverContextHash ||
    roundHash ||
    ticketHash
)
```

The fixed 64-byte P-256 signature is produced over this digest. `playerId` is transitively bound through the Identity-signed ticket selected by `ticketHash`; no avatar field is needed because the player Ent ID must equal ticket `sub`.

### `PresenceProofCommand`

```text
roundHash[32] || ticketHash[32] || signature[64]
```

Total command body: 128 bytes. The honest server accepts at most one proof per socket per round and verifies it before fanout to prevent malicious-client amplification.

### Presence proof record

```text
ticketHash[32] || signature[64]
```

Each proof record is 96 bytes.

### `PresenceProofBatchCommand`

```text
header:
    roundHash[32]

payload:
    zero or more complete PresenceProofRecord values
```

There is no chunk index, total proof count, or final commit. Proof completeness is not trustworthy when the server may be malicious. The server streams small batches as verified proofs arrive; missing proofs expire locally.

## Admission and ticket refresh flow

### Initial connection

```text
Client -> Identity: GetToken(version, host, port, sessionId, public key)
Identity -> Client: signed ticket

Client -> Server: BeginAuthCommand
Server -> Client: ReadyAuthCommand(32-byte nonce)
Client -> Server: CompleteAuthCommand(ticket + signature)
Server -> Client: ResultAuthCommand

Server -> Clients: PlayerIdentityCommand(ticket)
Client -> Server: SpawnPlayerCommand
Server -> Clients: EntUpdateCommand(player Ent ID = ticket.sub)
```

### Refresh

```text
Client -> Identity: GetToken(same host, port, sessionId, public key)
Identity -> Client: refreshed ticket

Client -> Server: BeginAuthCommand
Server -> Client: ReadyAuthCommand(new nonce)
Client -> Server: CompleteAuthCommand(refreshed ticket + signature)
Server -> Client: ResultAuthCommand
Server -> Clients: PlayerIdentityCommand(refreshed ticket)
```

Refresh does not spawn a second player. Presence proofs switch to the new `ticketHash` only after the refreshed ticket is installed. Peers may retain the previous verified cache entry only until its existing local freshness and ticket-expiry bounds.

## Presence cadence and load smoothing

The 10-second interval is a work window, not a single timer tick.

### Continuous challenge submission

- Each client sends a challenge every 5 seconds.
- Initial offset is stable jitter derived from the connection session, for example `SHA256(sessionId || "challenge-offset") mod 5000ms`.
- Clients retain their two newest pending challenges and local creation times.
- The server freezes the newest challenge per active connection at each 10-second round boundary.

### Rolling round schedule

```text
Second 0:    Freeze newest challenge per connection and encode round chunks once.
Seconds 0-3: Pace PresenceRoundChunk fanout using fair round-robin byte budgets.
Seconds 1-9: Clients complete at staggered times and return one proof.
Seconds 1-9: Verify proofs and continuously broadcast small proof batches.
Seconds 9-10: Drain queues, expire work, and leave scheduling margin.
Second 10:   Freeze the next round; challenge collection has continued throughout.
```

Client proof timing may add deterministic sub-second jitter derived from `sessionId` and `roundHash` if server-side staggering is insufficient.

### Freshness

Bob records each challenge using a local monotonic clock:

```text
validUntil = localChallengeCreationTime + 15 seconds
```

Receiving a replay never moves this deadline. Proofs that complete after it are stale even if ticket time claims remain valid.

## Dedicated server presence service

Add a server-owned `ServerPresenceLoop` that starts before listeners and stops cleanly during shutdown. The game tick performs no ongoing presence work.

### Thread ownership

Only the presence loop mutates:

- active presence sessions and connection generations;
- raw tickets, ticket hashes, and temporary public keys;
- latest challenges;
- current and previous rounds;
- accepted proofs;
- broadcast queues and token-bucket state.

It does not access ECS Ents, world/dimension collections, Google credentials, HTTP, or mutable JWKS state.

### Input events

Socket receiver and lifecycle threads copy bounded data into a bounded inbox:

- `SessionAdded`
- `SessionTicketUpdated`
- `SessionRemoved`
- `ChallengeReceived`
- `ProofReceived`
- `StopRequested`

Every event includes `sessionId` and `connectionGeneration`. A delayed disconnect from generation 7 cannot remove generation 8.

Use separate bounded lanes or equivalent priority handling:

- lifecycle events are lossless and highest priority;
- challenges are latest-wins per session;
- proofs have at most one pending slot per session and current round.

If lifecycle registration cannot be recorded, authentication fails rather than creating an authenticated session unknown to the presence service.

Never enqueue a span into `NetSocket`'s reusable receive buffer; copy required fixed bytes first.

### Output and backpressure

- `NetSocket.Send` may be called from the presence loop; it already serializes output-buffer access and existing push threads perform TCP/TLS writes.
- Do not hold `ServerSockets`' global list lock while broadcasting. Maintain the presence loop's own session snapshot.
- Encode immutable round chunks and proof batches once, then reuse their bytes for fanout where the socket API permits.
- Route `PlayerIdentityCommand`, round chunks, and proof batches through this one output scheduler; do not concurrently broadcast Identity messages from authentication threads.
- Pace output using fair round-robin scheduling and a token bucket.
- Current sockets have approximately 128 KiB of output segments, less than a full 1,000-player round; never enqueue an entire round to one socket at once.
- Add a `TrySend`-style backpressure result or equivalent observability so the presence service can distinguish a successful enqueue from a slow-socket disconnect.
- Bound inbox, round, proof, and per-socket queues.
- Keep only the newest challenge and at most one proof per socket per round.
- Disconnect consistently slow or abusive sockets rather than allowing unbounded memory growth.
- Never block the presence loop on JWKS HTTP. Admission passes an immutable validated ticket/public-key snapshot into it.

The server-wide `PresenceEgressBytesPerSecond` setting defaults to `20000000`. The scheduler gives the current round's challenge chunks priority during its first four seconds, then alternates established-session Identity updates with proof batches. A new viewer's initial Identity snapshot must still finish before proof batches are sent to that viewer.

Every 60 seconds the server logs one non-sensitive cumulative presence summary with active-session count; lifecycle, challenge, and proof inbox depths; round count and age; last-round challenge count; accepted/rejected challenge and proof counts; bytes queued; backpressure events; and slow-socket disconnects. Tickets, nonces, and signatures are never included.

One dedicated thread is the initial target. If benchmarks show P-256 verification is the bottleneck, a bounded crypto worker pool may return immutable results to the single owning presence loop without involving the game tick.

## Client verification and local cache

Add client services for:

- temporary P-256 key lifecycle;
- direct JWKS retrieval and bounded caching;
- strict Identity-ticket validation;
- pending challenge tracking using a monotonic clock;
- round chunk assembly and canonical hashing;
- proof verification on a bounded background worker;
- immutable verified-profile snapshots.

Suggested indexes:

```text
ticketsByHash: ticketHash -> ValidatedIdentityTicket
currentTicketByPlayerId: playerId -> ticketHash
verifiedPlayersById: playerId -> VerifiedPlayerSnapshot
pendingChallengesBySequence: sequence -> local creation/deadline/nonce
```

If a malicious server sends a proof before its referenced ticket, the client may retain it only in the bounded current-round queue until that ticket arrives. The proof never extends its challenge deadline and is discarded when the round or local challenge expires.

For each proof, Bob verifies:

1. the complete round hashes to `roundHash`;
2. one of Bob's exact pending challenges occurs exactly once;
3. Bob's local challenge deadline has not passed;
4. the referenced ticket exists and passes strict Identity validation;
5. the ticket server context matches Bob's TLS connection;
6. the P-256 proof signature validates using ticket `cnf.jwk`;
7. the proof signer's ticket `sid` appears exactly once in the same challenge round;
8. ticket `sub`, `sid`, and key are unique within the accepted session set;
9. any displayed remote player Ent has `Ent.Id == UUID(ticket.sub)`;
10. duplicate active tickets or player Ents for one `playerId` produce an invalid/ambiguous state.

Proof verification runs off the render thread. At 1,000 players, target a sustained 100-150 verifications per second and prioritize visible players, then an open player list, then other entries. Priority affects latency only, never validation rules.

### Local status model

- `Pending`: a ticket or Ent is known but no current proof has been verified.
- `Verified`: ticket, current proof, context, and Ent-ID equality are valid.
- `Stale`: previous proof exceeded Bob's local deadline without replacement.
- `Invalid`: malformed ticket/proof, bad signature, context mismatch, duplicate identity, or Ent-ID mismatch.
- `Unverified`: development/no-auth identity or identity evidence unavailable by design.

Bob updates only his private client cache. No status is sent to the server, Identity service, Alice, or other viewers.

## Player Ent model and clean-world cutoff

### Target data model

- Persistent world profile Ent ID: `playerId`.
- Dimension player Ent ID: the same `playerId`, in its separate dimension arena.
- Client receives no synchronized world-profile Ent, avoiding duplicate GUIDs in its global replica dictionary.
- Dimension player lookup and profile synchronization use `player.Id`.
- New multiplayer player Ents do not set the saved `WorldPlayer` reference.
- Non-player Ents retain independently allocated IDs.
- Reject a player spawn if `playerId` collides with a non-player Ent or an active duplicate.

### New saves

Allocate a new player with `DimensionEntArena.Alloc(playerId)`, not `Alloc()`.

### Pre-feature saves

No save migrator is part of this epic. Multiplayer worlds created before this protocol cutoff are unsupported and may be deleted before starting the updated server. The server does not guess at, rewrite, or preserve legacy random player Ent IDs.

### Explicit v1 constraint

The same `playerId` cannot identify two simultaneously visible player incarnations. Multiple bodies, corpses, possession, or overlapping dimension transitions require a future scoped or incarnation ID and a client replica key such as `(scopeId, playerId, incarnationId)`.

## UI behavior

### Nameplates

- Look up the immutable local cache by `remotePlayerEnt.Id`.
- Show only the Identity-ticket username, never a server-provided string.
- Visually distinguish `Verified`, `Pending`, `Stale`, `Invalid`, and `Unverified`.
- Perform no ticket parsing, HTTP, hashing, or signature verification during rendering.

### Player list

- Add a multiplayer player list backed by `verifiedPlayersById`, independent of streamed terrain chunks.
- Reconcile the existing Tab binding that currently opens creative inventory. Multiplayer composition should override/remap it without breaking singleplayer/creative behavior.
- Use `playerId` to disambiguate duplicate display names, for example a short suffix.
- If ping is shown, label it `server-reported`; it is not Identity-verified and remains outside the presence signature.

### No-auth development mode

- Retain the current visually distinct development name convention where useful.
- Never issue or synthesize an Identity ticket for no-auth users.
- Never render no-auth users as `Verified`.
- Raw TCP and no-auth deployments must show an explicit development/unverified indicator.

## `craftdig.io` workstream

### Required ticket changes

- Replace the `GetToken` `{ host, nonce }` schema with the strict v1 request above.
- Preserve Google Bearer verification and `{ jwt }` response shape.
- Rename the signing helper conceptually from generic JWT creation to multiplayer-ticket creation.
- Add server port, session ID, and validated P-256 `cnf.jwk` claims.
- Remove server nonce from request and ticket.
- Add exact `typ`, multiplayer `aud`, `ver`, `nbf`, and active `kid`.
- Retain RS256 issuer signing.
- Set and test a maximum 15-minute ticket lifetime.
- Never log raw tickets, Google credentials, private keys, or proof signatures.

### Stable account mapping

The Google `sub` to Craftdig UUID mapping must be atomic. Replace get-then-unconditional-put with a conditional DynamoDB write using `attribute_not_exists(sub)`. On conditional failure, consistently read and use the winning mapping. Validate stored `userId` values as UUIDs before signing.

### Remove unsafe warm-up signing

Delete the Lambda cold-start code that creates and logs a real signed fake JWT. Test signing through unit tests, not production initialization.

### Signing-key operation

- Add the permanent `craftdig-key-1` `kid` to every ticket header.
- Retain the existing production signing key, raw PKCS8 secret, standard public JWK, and custom-resource logical ID.
- On a fresh stack only, generate the key once, publish its public JWK, invalidate CloudFront, wait for that invalidation, and only then activate the matching private key.
- Make custom-resource Update and Delete no-ops so ordinary deployments never regenerate or replace the signing key. There is no environment-variable trigger, update-triggered rotation, custom retirement metadata, or key-overlap mechanism in this release.
- Treat future signing-key replacement as a separate protocol/operations change that must design an overlap window before it is enabled.

### Identity behavior that remains unchanged

- Google remains the upstream login provider.
- Craftdig's random UUID remains the persistent `playerId`.
- RS256 remains the Identity-ticket signing algorithm.
- `/.well-known/jwks.json` remains the public-key endpoint.
- The service does not register or contact game servers.
- The service does not receive presence challenges, proofs, or Bob's local result.

### Username policy

For this epic, usernames are nonunique display labels. Remove game-server duplicate-name enforcement and use only `playerId` for session uniqueness. A future globally unique name feature would require transactional normalized-name reservation in `craftdig.io`.

### Clean API replacement

`POST /api/GetToken` has exactly one request contract: the strict v1 PoP request in this epic. Delete the old `{ host, nonce }` bearer-ticket contract and its signing/audience helpers. No `GetTokenV2`, discriminated compatibility branch, fallback, or legacy validation path is shipped.

## Craftdig server admission and JWKS hardening

- Validate exact issuer, audience, type, algorithm, `kid`, ticket version, lifetime, required claims, server context, session, and P-256 JWK.
- Configure at most 30 seconds of JWT clock skew and enforce `exp - iat <= 900 seconds`.
- Preserve exact ticket bytes and hash after authentication.
- Refresh JWKS only for an unknown `kid` or cache expiry, not after every validation failure.
- Rate-limit unknown-key refresh and use bounded HTTP timeout/cancellation.
- Require exactly one usable public RS256 key in the standard JWKS and reject private, empty, or multi-key sets.
- Respect cache headers and permit the known cached key only for the bounded stale window during a transient Identity outage. Unknown keys still fail closed.
- Do not perform synchronous JWKS HTTP while holding the global authentication lock.
- Bound ticket length before parsing.
- Reject duplicate active `playerId`; do not reject duplicate username.
- On disconnect or refresh, update the presence service using session and connection generation.

## Performance budgets

Ignoring framing/TLS and tickets, each round distributes approximately:

```text
56 bytes per viewer challenge
96 bytes per player proof
152 bytes per player per receiving client
```

| Players | Per-client bytes/round | Server egress/round | Server egress at 10s |
|---:|---:|---:|---:|
| 20 | 3,040 B | 60,800 B | 0.006 MB/s |
| 100 | 15,200 B | 1.52 MB | 0.152 MB/s |
| 1,000 | 152,000 B | 152 MB | 15.2 MB/s |

At 1,000 players:

- each client signs once per 10 seconds;
- each client verifies about 100 proofs per second;
- the honest server verifies about 100 proofs per second before fanout;
- challenge fanout averages about 5.6 MB/s;
- proof fanout averages about 9.6 MB/s;
- target presence egress budget is 18-20 MB/s including scheduling headroom, before measuring TLS overhead.

If tickets average 1 KiB and refresh around every 10 minutes with independent jitter, ticket updates add about 1.7 KiB/s to each viewer and about 1.7 MB/s of server fanout at 1,000 players. A new viewer also requires a paced initial ticket snapshot of roughly 1 MiB. These costs are separate from the 15.2 MB/s presence-round estimate.

Scheduling can remove spikes but cannot reduce average O(N²) server fanout. If measured capacity is below the average, reduce the player cap or design a later compressed/delta protocol rather than weakening verification.

## Security acceptance matrix

| Scenario | Required result |
|---|---|
| Forged username | UI ignores server string and uses only a valid Identity ticket. |
| Stolen ticket | Join and presence fail without the ticket-bound P-256 private key. |
| Wrong host or port | Ticket is rejected by client and server. |
| Ticket replay on a new connection | Fresh server nonce and join signature are required. |
| Old presence proof | Bob's unchanged local challenge deadline makes it stale. |
| Replay after disconnect | At most the remaining 15-second local freshness window. |
| Wrong player Ent | `Ent.Id != ticket.sub` is invalid. |
| Duplicate player Ent or session | The identity is invalid/ambiguous; honest server rejects the duplicate. |
| Forked challenge rounds | Bob accepts only proofs over a complete round containing Bob's current challenge. Global fork detection remains out of scope. |
| Delayed or incomplete round | No signature or local verification; previous state expires normally. |
| Malformed/oversized input | Rejected before allocation or cryptographic work beyond configured bounds. |
| Malicious-client proof spam | One proof per socket/round; bounded queues and server verification prevent fanout. |
| Identity outage | Existing tickets work until expiry; new joins/refreshes fail closed to unverified/disconnected behavior. |
| Unknown JWKS `kid` | A bounded early refresh is attempted; the ticket fails closed if the one published key still does not match. |
| No-auth/raw connection | Never shown as verified. |
| Malicious server fakes movement | Identity may remain verified, but UI does not claim movement authorship. |
| Server lies about ping/world | Values remain explicitly server-reported/unverified. |

## Implementation work packages

### WP0: Freeze protocol and test vectors

- [x] Make existing command IDs explicit.
- [x] Add new command IDs 20011, 20012, 30017, 30018, and 30019.
- [x] Add fixed hash, nonce, session-ID, and P1363-signature value types.
- [x] Add dedicated exact-length raw codecs for all five new security-sensitive commands.
- [x] Add canonical server-context, UUID, integer, round, authentication, and proof encoders.
- [x] Publish shared golden vectors consumable by C# and TypeScript tests.
- [x] Document hard limits for tickets, players, chunks, records, queues, and active rounds.

Acceptance:

- Existing IDs do not change.
- C# and TypeScript produce identical server-context and ticket-request values.
- Golden auth, round, and proof digests match byte-for-byte on every supported platform.

### WP1: Identity-service v1 ticket

- [x] Add versioned strict request schema.
- [x] Validate/canonicalize server host and port.
- [x] Validate session UUID and P-256 public JWK.
- [x] Issue the exact header and claims specified above.
- [x] Add active `kid` to ticket header.
- [x] Change ticket lifetime to 15 minutes.
- [x] Remove fake token creation/logging.
- [x] Fix atomic Google-account mapping.
- [x] Add schema, ticket, race, and one-key publication tests.
- [x] Replace the old `GetToken` contract outright and remove its dead backend code.

Acceptance:

- Invalid keys, contexts, and extra fields are rejected.
- Parallel first login yields exactly one permanent Craftdig UUID.
- No server nonce, private key, or Google claim appears in the ticket.
- Ticket verifies using published JWKS and exact expected metadata.

### WP2: Client key and ticket acquisition

- [x] Generate new P-256 key and `sessionId` per connection.
- [x] Compute server context only after successful TLS negotiation and the applicable DNS or direct-IP certificate policy.
- [x] Call the versioned Identity endpoint.
- [x] Strictly validate the returned ticket against the request.
- [x] Retain exact bytes/hash and protect the private key in process memory.
- [x] Refresh around 10 minutes with jitter using the same session/key.
- [x] Clear ticket state and dispose key material on disconnect.

Acceptance:

- A returned ticket with any mismatched requested field aborts connection.
- Reconnect creates a new session/key.
- Ticket refresh does not create a second avatar.

### WP3: Strict game-server admission

- [x] Change `ReadyAuthCommand` nonce encoding.
- [x] Change `CompleteAuthCommand` payload and verify P-256 signature.
- [x] Enforce all ticket header/claim/context/lifetime rules.
- [x] Preserve raw ticket, hash, session, and public key.
- [x] Reuse auth commands for safe ticket refresh.
- [x] Replace duplicate-name logic with duplicate-`playerId` logic.
- [x] Harden JWKS caching, refresh, timeout, and lock behavior.
- [x] Notify the presence service using immutable session events.

Acceptance:

- A ticket without its private key cannot authenticate.
- Tickets for another host/port, issuer, audience, type, algorithm, or version fail.
- Unknown-key refresh cannot be amplified by arbitrary invalid JWTs.

### WP4: Player-ID Ent model

- [x] Allocate new player Ents with `playerId`.
- [x] Resolve world profile and update position by player Ent ID.
- [x] Reject duplicate or non-player ID collisions.
- [x] Document the clean-world cutoff and fail safely on ID collisions.

Acceptance:

- A new player, reconnect, and restart use the same `playerId` Ent and preserve state in a world created by the new implementation.
- No migration, backup, or legacy-ID rewrite code is shipped.

### WP5: Dedicated server presence service

- [x] Add lifecycle-managed presence thread and bounded inbox.
- [x] Add lossless lifecycle, latest-wins challenge, and one-proof-per-round input lanes.
- [x] Add session generation-safe join/update/remove handling.
- [x] Register challenge and proof receivers that only validate/copy/enqueue.
- [x] Implement latest-challenge storage and round freezing.
- [x] Encode/chunk each round once.
- [x] Verify one proof per socket/round.
- [x] Stream proof batches as proofs arrive.
- [x] Implement token-bucket output and fair round-robin fanout.
- [x] Add observable send/backpressure handling for slow sockets.
- [x] Add queue, round-age, bytes, proof, rejection, and slow-socket metrics without sensitive payload logs.

Acceptance:

- Presence adds no cryptographic or batch work to the game tick.
- Old disconnect events cannot remove a replacement session.
- Slow clients cannot create unbounded memory or block other clients.
- 1,000-player test stays within configured queue and output budgets.

### WP6: Client presence verification

- [x] Register identity, round-chunk, and proof-batch receivers before spawn/streaming.
- [x] Add direct JWKS cache independent of the game server.
- [x] Add bounded ticket and round caches.
- [x] Generate challenges with monotonic timestamps and stable jitter.
- [x] Assemble and hash one round off the render thread.
- [x] Sign at most once per valid round.
- [x] Verify proofs on bounded background workers.
- [x] Maintain immutable per-player status snapshots.
- [x] Enforce duplicate, context, expiry, and Ent-ID rules.

Acceptance:

- Bob independently verifies Alice without trusting a server decision.
- Replays never extend Bob's local deadline.
- Missing chunks/proofs expire cleanly without partial trust.
- No HTTP, JWT parsing, hashing, or signatures occur in rendering.

### WP7: Nameplates and multiplayer player list

- [x] Render authenticated nameplates from the local cache keyed by Ent ID.
- [x] Add clear status treatment for all five local states.
- [x] Add a global multiplayer player list.
- [x] Resolve Tab/creative-inventory input composition.
- [x] Disambiguate duplicate display names with player ID.
- [x] Label any displayed ping as server-reported.
- [x] Add explicit no-auth/raw development indicator.

Acceptance:

- A server-provided unauthenticated name is never displayed as verified.
- Nameplate identity disappears or becomes stale at the local deadline.
- Off-chunk roster entries do not require replicated world-profile Ents.

### WP8: Verification, load, and adversarial testing

- [x] Add protocol framing, length, order, duplicate, and chunk tests.
- [x] Add positive end-to-end authentication/presence test.
- [x] Test that every accepted proof signer's ticket session appears exactly once in the signed round.
- [x] Add wrong-key, wrong-context, wrong-Ent, replay, delayed, fork, omission, duplicate-session, refresh, reconnect, and outage tests.
- [x] Add no-auth/raw negative verification tests.
- [x] Add N=20, N=100, and N=1,000 load simulations.
- [x] Measure deterministic round CPU/allocation, P-256 throughput, inbox depth, and encoded outbound bandwidth.
- [ ] Measure live game-tick latency, presence-thread CPU, and TLS/socket overhead on the deployment target.
- [x] Test slow-reader backpressure and output-ring exhaustion with a real loopback socket.
- [x] Audit logs to confirm they contain no raw Google tokens, Identity tickets, private keys, nonces, or complete signatures.

Acceptance:

- All security matrix cases have automated coverage where practical.
- Sustained 1,000-player presence processing meets the documented average budget or the configured player cap is reduced.
- The renderer and game tick show no presence-related work spike.

### WP9: Rollout and cleanup

- [x] Deploy the sole strict Identity `GetToken` schema and JWKS `kid` support.
- [ ] Deploy game server/client protocol together or enforce a clear minimum version.
- [ ] Start the updated server with a clean world; pre-feature multiplayer saves are outside the rollout contract.
- [ ] Stage live load with `MaxPlayers` and the presence egress budget before raising the production cap.
- [ ] Monitor issuance, authentication, proof rejection, queue, and bandwidth metrics.
- [x] Confirm no legacy Identity issuance or game-server validation code remains.
- [x] Confirm Identity still publishes the single expected `craftdig-key-1` public key before releasing clients.
- [x] Update operator and player-facing documentation.

Acceptance:

- No legacy bearer ticket can produce a `Verified` state after cutoff.
- Rollback remains possible until the protocol cutover is confirmed; old and new clients/servers are never mixed.
- Ordinary deployments do not replace the signing key. Any future replacement requires a separately reviewed overlap design.

## Implemented source map

### Craftdig

- `src/Craftdig.Protocol/Commands.cs`, `Commands/`, and `Codecs/`: frozen command IDs and exact security-command wire formats.
- `src/Craftdig.Protocol/Security/` and `Presence/`: canonical UUID, server-context, digest, nonce, key, signature, challenge, round, and proof types.
- `src/Craftdig.Protocol/Net/` and `ProtocolLimits.cs`: raw command handlers, bounded framing, and observable send backpressure.
- `src/Craftdig.Menus.Multiplayer/`: TLS context, Identity HTTP request, connection/session key lifecycle, admission, ticket refresh, disconnect cleanup, and presence UI.
- `src/Craftdig.Menus/Menus/ModuleMainMenu.cs`: development-only raw TCP and no-auth controls.
- `src/Craftdig.Identity/`: the shared strict ticket validator, JWKS cache, and canonical JSON reading used by both client and server.
- `src/Craftdig.Client/Identity/` and `src/Craftdig.Client/Presence/`: ticket ledger, roster and status policy, challenge/signing workers, and proof verification over the shared validator.
- `src/Craftdig.Client/Sync/`: player Ent observation keyed by authenticated `playerId`.
- `src/Craftdig.Server/`: configuration, strict ticket/JWKS admission, listener integration, session lifecycle, dedicated presence thread, fair output scheduling, backpressure, and aggregate metrics.
- `src/Craftdig.World.Backend/`, `src/Craftdig.World.Server/`, `src/Craftdig.Dimension/`, `src/Craftdig.Dimension.Backend/`, and `src/Craftdig.Dimension.Server/`: persistent profile and dimension player indexing by `playerId`, including the clean-world cutoff.
- `res/Craftdig.Server.Cli/Server.ini` and `scripts/Craftdig.Dev.Server/Program.cs`: production-safe defaults and explicit local development transport options.
- `tests/Craftdig.Identity.Test/`, `tests/Craftdig.Protocol.Security.Test/`, `tests/Craftdig.Server.Security.Test/`, and `tests/Craftdig.Client.Security.Test/`: validator, JWKS, protocol, adversarial, transport, lifecycle, cache, performance, and real-socket backpressure coverage.

### craftdig.io

- `common/src/index.ts`
- `lambdas/src/multiplayer-ticket-request.ts`
- `lambdas/src/get-token.ts`
- `lambdas/src/jwt.ts`
- `lambdas/src/google-auth.ts`
- `lambdas/src/api.ts`
- `lambdas/src/generate-jwt-keypair.ts`
- `lambdas/src/get-username.ts`
- `lambdas/test/multiplayer-ticket.test.ts`
- `src/api.ts`
- `src/frontend.ts`

## Definition of done

- [x] The two repositories implement the same canonical ticket/context/session rules.
- [x] Identity tickets are PoP-bound, server-context-bound, typed, audience-restricted, short-lived, and keyed by versioned `kid`.
- [x] One Google account cannot race into multiple permanent Craftdig IDs.
- [x] Game-server admission strictly validates the ticket and temporary-key signature.
- [x] Player Ent network IDs equal authenticated `playerId` in new multiplayer worlds.
- [x] Bob independently verifies presence using Bob's own fresh challenge and local clock.
- [x] No server or Bob verification-result assertion is trusted or transmitted.
- [x] All ongoing presence work is outside the game tick and render thread.
- [x] No-auth/raw identities are visibly unverified.
- [x] N=20, N=100, and N=1,000 deterministic performance and adversarial tests pass within configured budgets.
- [x] Legacy ticket issuance/validation is removed from the implementation.
- [x] Operational documentation covers signing-key publication, Identity outage, the clean-world cutoff, metrics, and rollback.

## Resolved implementation choices and remaining rollout checks

- Protocol v1 prohibits multiple simultaneously visible incarnations for one `playerId`; client replicas remain keyed by `playerId`. Supporting overlapping dimension incarnations requires a later protocol and replica-key design.
- Deterministic N=1,000 tests support the implemented single-owner presence thread, so no crypto worker pool was added. This is a regression result, not a deployment-target capacity measurement.
- The production `MaxPlayers` and `PresenceEgressBytesPerSecond` values remain rollout decisions. Set them only after the signed-in two-client check and live game-tick, presence-thread, TLS, socket, CPU, allocation, and bandwidth measurements on the deployment target.
