# ================================================
# Bloomtown Project Setup Script (P0 Preparation)
# ================================================

Write-Host "=== Bloomtown Setup Script ===" -ForegroundColor Cyan
Write-Host "This script will prepare the project structure for P0." -ForegroundColor Yellow

$root = Get-Location

# 1. Create recommended folder structure
Write-Host "`n[1/5] Creating folder structure..." -ForegroundColor Green

$folders = @(
    "src/Bloomtown.Server/Simulation/Systems",
    "src/Bloomtown.Server/Simulation/World",
    "src/Bloomtown.Server/Networking",
    "src/Bloomtown.Server/Persistence",
    "src/Bloomtown.Server/World",
    "src/Bloomtown.Server/Configuration",
    "src/Bloomtown.Shared/Protocol",
    "src/Bloomtown.Shared/Math",
    "src/Bloomtown.Shared/Constants",
    "src/Bloomtown.Shared/Extensions",
    "content/world",
    "content/schedules",
    "content/actions",
    "db/migrations",
    "docs",
    "scripts",
    "logs"
)

foreach ($folder in $folders) {
    $path = Join-Path $root $folder
    if (-not (Test-Path $path)) {
        New-Item -ItemType Directory -Path $path -Force | Out-Null
        Write-Host "  Created: $folder" -ForegroundColor DarkGray
    }
}

# 2. Create basic documentation files
Write-Host "`n[2/5] Creating documentation files..." -ForegroundColor Green

$progressPath = Join-Path $root "docs/progress.md"
if (-not (Test-Path $progressPath)) {
    @"
# Bloomtown Progress

## Current Phase: P0 Preparation (Week 1)

### Completed
- [x] Project structure created
- [x] .NET solution + projects
- [x] NuGet packages installed

### In Progress
- [ ] S1: Headless Server Tick
- [ ] S2: UDP Movement Replication

### Blockers
- (none)
"@ | Out-File -FilePath $progressPath -Encoding UTF8
    Write-Host "  Created: docs/progress.md" -ForegroundColor DarkGray
}

# 3. Create basic .gitignore (if not exists)
$gitignorePath = Join-Path $root ".gitignore"
if (-not (Test-Path $gitignorePath)) {
    @"
## .NET
bin/
obj/
*.user
*.suo

## Unity
Library/
Temp/
Logs/
Build/
Builds/
UserSettings/
*.csproj
*.sln

## Database
*.db
*.db-shm
*.db-wal

## Logs
logs/

## IDE
.vs/
.idea/
*.swp
.DS_Store
"@ | Out-File -FilePath $gitignorePath -Encoding UTF8
    Write-Host "  Created: .gitignore" -ForegroundColor DarkGray
}

# 4. Create initial GameServer.cs and update Program.cs
Write-Host "`n[3/5] Creating initial code files for S1..." -ForegroundColor Green

$programPath = Join-Path $root "src/Bloomtown.Server/Program.cs"
$gameServerPath = Join-Path $root "src/Bloomtown.Server/GameServer.cs"

# Create GameServer.cs
@"
using Serilog;

namespace Bloomtown.Server;

public class GameServer
{
    private const int TickRate = 20;           // 20 ticks per second
    private const int TickIntervalMs = 1000 / TickRate;

    public void Run()
    {
        Log.Information("GameServer started. Tick rate: {TickRate} Hz", TickRate);

        var lastTick = DateTime.UtcNow;
        var accumulator = 0.0;

        while (true)
        {
            var currentTime = DateTime.UtcNow;
            var frameTime = (currentTime - lastTick).TotalMilliseconds;
            lastTick = currentTime;

            accumulator += frameTime;

            while (accumulator >= TickIntervalMs)
            {
                Tick();
                accumulator -= TickIntervalMs;
            }

            Thread.Sleep(1);
        }
    }

    private void Tick()
    {
        // TODO: Add WorldTimeSystem, Needs, Schedules, etc. here later
        Log.Debug("Tick executed");
    }
}
"@ | Out-File -FilePath $gameServerPath -Encoding UTF8 -Force

# Update Program.cs
@"
using Serilog;

namespace Bloomtown.Server;

internal class Program
{
    static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File("logs/server-.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        Log.Information("=== Bloomtown Server Starting ===");

        try
        {
            var server = new GameServer();
            server.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Server crashed unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
"@ | Out-File -FilePath $programPath -Encoding UTF8 -Force

Write-Host "  Created/Updated: Program.cs and GameServer.cs" -ForegroundColor DarkGray

# 5. Final message
Write-Host "`n[4/5] Setup completed!" -ForegroundColor Green
Write-Host "`nNext steps:" -ForegroundColor Yellow
Write-Host "1. Run: dotnet build" -ForegroundColor White
Write-Host "2. Run: dotnet run (to test S1)" -ForegroundColor White
Write-Host "3. Tell me when you're ready to continue to the next step." -ForegroundColor White

Write-Host "`n=== Setup Finished ===" -ForegroundColor Cyan