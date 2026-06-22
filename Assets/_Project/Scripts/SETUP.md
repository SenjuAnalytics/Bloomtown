# Bloomtown Unity Client — Setup Guide

## 1. Install LiteNetLib

Script `NetworkClient.cs` membutuhkan LiteNetLib. Ada dua cara:

### Opsi A: Git URL (termudah)
1. `Window > Package Manager`
2. Klik `+` (pojok kiri atas) → **Add package from git URL**
3. Masukkan:
   ```
   https://github.com/RevenantX/LiteNetLib.git?path=LiteNetLib#master
   ```
4. Klik **Add** → tunggu download selesai

### Opsi B: Copy DLL dari server build
1. Build server dulu:
   ```
   dotnet build src/Bloomtown.Server/Bloomtown.Server.csproj -c Release
   ```
2. Cari `LiteNetLib.dll` di:
   ```
   src/Bloomtown.Server/bin/Release/net8.0/LiteNetLib.dll
   ```
3. Copy ke `Assets/Plugins/LiteNetLib/LiteNetLib.dll`

---

## 2. Copy Bloomtown.Shared.dll ke Unity

```
dotnet build src/Bloomtown.Shared/Bloomtown.Shared.csproj -f netstandard2.1 -c Release
```
Lalu copy:
```
src/Bloomtown.Shared/bin/Release/netstandard2.1/Bloomtown.Shared.dll
  →  Assets/Plugins/Bloomtown.Shared/Bloomtown.Shared.dll
```

---

## 3. Copy Script Files

Copy semua folder dari sini ke `Assets/_Project/Scripts/`:
```
Network/
  NetworkEvents.cs
  NetworkClient.cs
Player/
  LocalPlayer.cs
Entity/
  EntityInterpolator.cs
  EntityRegistry.cs
Bootstrap/
  GameBootstrap.cs
```

---

## 4. Buat Prefabs

### LocalPlayer Prefab
1. Buat empty GameObject → rename "LocalPlayer"
2. Add Component: `CharacterController`
3. Add Component: `LocalPlayer`
4. Buat child GameObject "CameraRig"
   - Tambahkan `Camera` component ke CameraRig atau child-nya
   - Tag: MainCamera
5. Di LocalPlayer inspector → assign _cameraRig = CameraRig transform
6. Save as prefab ke `Assets/_Project/Prefabs/LocalPlayer.prefab`

### NPC Prefab
1. Buat Capsule GameObject → rename "NPC"
2. Add Component: `EntityInterpolator`
3. Save as prefab ke `Assets/_Project/Prefabs/NPC.prefab`

### RemotePlayer Prefab
1. Buat Capsule GameObject → rename "RemotePlayer"
2. Add Component: `EntityInterpolator`
3. Save as prefab ke `Assets/_Project/Prefabs/RemotePlayer.prefab`

---

## 5. Setup Scene

1. Buat scene "Bootstrap" (index 0 di Build Settings)
2. Buat 3 empty GameObjects:

   **"Network"**
   - Add Component: `NetworkClient`
   - Inspector: Server Host = `127.0.0.1`, Port = `7777`

   **"EntityRegistry"**
   - Add Component: `EntityRegistry`
   - Assign: NPC Prefab, Remote Player Prefab

   **"GameBootstrap"**
   - Add Component: `GameBootstrap`
   - Assign: Network Client, Entity Registry, Local Player Prefab

---

## 6. Test

1. Jalankan server terlebih dahulu:
   ```
   dotnet run --project src/Bloomtown.Server
   ```
2. Play di Unity Editor
3. Lihat Console:
   - `[NetworkClient] Connecting → 127.0.0.1:7777`
   - `[NetworkClient] ✓ Connected`
   - `[GameBootstrap] State → InGame. LocalPlayer entity id = X`
   - `[LocalPlayer] Spawned entity X at (...)` 
4. Status overlay muncul di pojok kiri atas (hanya saat Editor/Development Build)

---

## Troubleshooting

| Error | Penyebab | Fix |
|-------|----------|-----|
| `The type or namespace 'LiteNetLib' could not be found` | LiteNetLib belum install | Ikuti langkah 1 |
| `The type or namespace 'Bloomtown' could not be found` | DLL Shared belum di-copy | Ikuti langkah 2 |
| `_localPlayerPrefab belum di-assign` | Lupa assign di Inspector | Ikuti langkah 5 |
| Server tidak direspons / disconnect loop | Port 7777 diblokir firewall | Cek Windows Firewall, tambah exception untuk port 7777 UDP |
