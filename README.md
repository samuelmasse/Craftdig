# Craftdig

Craftdig is a C# Minecraft clone

## Repository solution

From this checkout, generate the gitignored solution from projects and evaluated
dependencies, then open `Craftdig.slnx`:

```powershell
dotnet run --project ../AlvorKit/scripts/AlvorKit.Script.Solution -- --repo-root .
```

Add `--watch` for continuous updates. See
[the solution workflow](../AlvorKit/docs/Solutions.md) for discovery and CI.
