using Craftdig.Dev;

RootLoop.RunGlfw<RootLoadNativeState>(injector => injector.Add(new FailSafeConfig { Enabled = false }));
