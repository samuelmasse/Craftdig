namespace Craftdig.World;

[World]
public record class WorldMeta(string Name, int Seed, Ent GameMode, Ent Difficulty);
