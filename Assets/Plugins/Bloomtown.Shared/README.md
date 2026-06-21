# Bloomtown.Shared in Unity

This folder is where the compiled `Bloomtown.Shared.dll` (netstandard2.1 build) goes so the
Unity client can use the same protocol types, math, and constants as the server — no copy-paste
drift between client and server code.

## How to update the DLL

1. From the repo root, build only the netstandard2.1 target:

   ```
   dotnet build src/Bloomtown.Shared/Bloomtown.Shared.csproj -f netstandard2.1 -c Release
   ```

2. Copy the output into this folder:

   ```
   src/Bloomtown.Shared/bin/Release/netstandard2.1/Bloomtown.Shared.dll
   -> Assets/Plugins/Bloomtown.Shared/Bloomtown.Shared.dll
   ```

3. Back in Unity, the Editor will auto-import the new DLL (you'll see it recompile). No manual
   "Reimport" needed unless Unity is already open and doesn't notice the file change.

## Known risk

`Bloomtown.Shared.csproj` is multi-targeted (`net8.0;netstandard2.1`). If anything in
`Bloomtown.Shared` uses a BCL API that doesn't exist in netstandard2.1 (e.g. `DateOnly`,
`TimeOnly`, `Random.Shared`, some newer `System.Text.Json` features), the netstandard2.1 build
will fail to compile even though the net8.0 build (used by the server) succeeds. If that
happens, swap the offending API for a netstandard2.1-safe equivalent, or wrap it in
`#if NET8_0 ... #endif`.

## TODO (not automated yet)

Consider a small build script / MSBuild post-build step that does steps 1-2 automatically on
every Shared rebuild, so this never goes stale silently.
