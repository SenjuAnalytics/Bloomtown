# Bloomtown — Technical Architecture

**Version:** 1.0  
**Status:** Draft  
**Audience:** Engineering (solo developer / small team)  
**Aligns with:** High Level System Design, NPC AI, Reputation, Communal Projects, Multiplayer Foundation, Economy System, Sprint Plan P0–P1, Overall Implementation Roadmap

---

## Document Purpose

This document translates Bloomtown's game-design decisions into concrete technical architecture. It is written for **incremental delivery**: each phase ships a playable slice without requiring later phases to be designed upfront. Terminology matches the NPC AI and decision-system specs (Hot/Warm/Cold tiers, reputation layers, communal projects, stockpiles).

---

## 1. Overall Architecture Overview

### 1.1 Recommended high-level architecture

Bloomtown uses a **dedicated authoritative game server** with **thin clients**. The server owns all gameplay truth; clients render, capture input, and apply server corrections.

```
┌─────────────────────────────────────────────────────────────────────────┐
│                              CLIENTS (N)                                 │
│  Input → Send commands    │    Receive snapshots/deltas → Interpolate  │
│  Local prediction (player movement only)  │  Cosmetic animation BTs     │
└───────────────────────────────┬─────────────────────────────────────────┘
                                │ UDP (+ reliable ordered channels)
                                ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                         GAME SERVER (authoritative)                      │
│  ┌─────────────┐  ┌──────────────┐  ┌─────────────┐  ┌──────────────┐ │
│  │ Sim Tick    │  │ Net Tick     │  │ Persist     │  │ Admin/API    │ │
│  │ (fixed dt)  │  │ (AOI, encode)│  │ (async)     │  │ (optional)   │ │
│  └──────┬──────┘  └──────┬───────┘  └──────┬──────┘  └──────────────┘ │
│         │                │                  │                            │
│         └────────────────┴──────────────────┘                            │
│                          │                                               │
│              Entity-Component World + Event Bus                          │
│   NPC AI │ Economy │ Reputation │ Projects │ Schedules │ World Time     │
└───────────────────────────────┬─────────────────────────────────────────┘
                                │
                                ▼
                    ┌───────────────────────┐
                    │  SQLite (P0–P1)       │
                    │  PostgreSQL (P2+)     │
                    └───────────────────────┘
```

### 1.2 Client–server responsibility split

| Responsibility | Server | Client |
|----------------|--------|--------|
| World time & day rollover | ✓ | Display only |
| NPC needs, schedules, AI decisions | ✓ | — |
| Pathfinding & collision resolution | ✓ | Optional visual navmesh preview |
| Inventory, stockpiles, economy ledgers | ✓ | UI mirror |
| Reputation & relationships | ✓ | UI mirror |
| Communal project state & contributions | ✓ | UI + build placement preview |
| Player movement validation | ✓ | Predict + reconcile |
| Combat / interaction outcomes | ✓ | Play reactions |
| Dialogue selection & quest logic | ✓ | Render lines from server payload |
| Animation & VFX | Replicate state | ✓ Drive from sync bundle |
| Gossip propagation | ✓ | — |

**Rule:** If it affects another player or an NPC's future behavior, it runs on the server.

### 1.3 Why this architecture

Bloomtown is a **multiplayer life simulation** where NPCs have persistent schedules, needs, relationships, and economic output that must remain coherent when players are absent. That requires:

1. **Authoritative simulation** — Cold-tier abstract NPC economy and reputation must not depend on any client being online.
2. **Shared world truth** — Two players watching Elsie skip harvest for lunch must see the same decision outcome.
3. **Scalable NPC cost** — Hot/Warm/Cold tiering only works if the server controls tick cadence and hydration.
4. **Cheat resistance** — Shop prices, stockpiles, and quest rewards cannot be client-side.
5. **Solo-dev pragmatism** — A single-process server with one world zone matches the 20–50 player / 20 NPC target through P2; zone workers are a later scaling lever, not a day-one requirement.

---

## 2. Server Architecture

### 2.1 Server model recommendation

| Phase | Model | Rationale |
|-------|-------|-----------|
| **P0–P2** | **Single process, single zone** | One village, ≤20 NPCs, ≤50 players. Simple deploy, easy debug. |
| **P3** | Single process + **async persistence worker** | Communal projects and reputation generate more write load; offload DB I/O. |
| **P4+** | **Optional multi-process zone workers** | Only when player count or world size exceeds single-core sim budget. |

**Start with one process.** Bloomtown's design already optimizes cost via Cold abstraction (30–60 s ticks, no pathfinding). The performance target of ≤8 full-AI NPCs per player cluster is achievable on a single zone worker through P3.

#### Future zone-worker topology (P4+)

```
                    ┌─────────────────┐
                    │  Gateway /      │
                    │  Matchmaker     │
                    └────────┬────────┘
                             │
           ┌─────────────────┼─────────────────┐
           ▼                 ▼                 ▼
    ┌─────────────┐   ┌─────────────┐   ┌─────────────┐
    │ Zone Worker │   │ Zone Worker │   │ Zone Worker │
    │  (Village)  │   │  (Forest)   │   │  (Farmland) │
    └──────┬──────┘   └──────┬──────┘   └──────┬──────┘
           │                 │                 │
           └─────────────────┴─────────────────┘
                             │
                    ┌────────▼────────┐
                    │ Shared DB +     │
                    │ Cross-zone bus  │
                    └─────────────────┘
```

Cross-zone NPCs (e.g., Tom chopping forest, delivering to village) use **abstract transit**: Cold-tier record updates position at zone boundary; Hot promotion triggers hydration when a player enters AOI.

### 2.2 Tick loop design

Use a **fixed-timestep simulation loop** decoupled from networking and persistence.

```
┌──────────────────────────────────────────────────────────────────┐
│                     MAIN SERVER LOOP (each frame)                 │
├──────────────────────────────────────────────────────────────────┤
│ 1. Ingest network packets (commands, acks)                       │
│ 2. SimTick(fixedDt)           — 20 Hz (50 ms) recommended      │
│ 3. NetTick()                  — 10–20 Hz, staggered per client   │
│ 4. PersistDrain()             — non-blocking queue, 0–N per frame│
│ 5. Metrics / admin heartbeat                                     │
└──────────────────────────────────────────────────────────────────┘
```

#### SimTick pipeline (ordered sub-steps)

Each `SimTick`, run systems in dependency order:

| Order | System | Notes |
|-------|--------|-------|
| 1 | `WorldTimeSystem` | Advance game clock; fire hour/day boundaries |
| 2 | `EventBusDrain` | Process queued domain events from prior tick |
| 3 | `EnvironmentSystem` | Weather, season, zone danger level (shared cache) |
| 4 | `NeedsDecaySystem` | Game-time minutes; all tiers, simplified for Cold |
| 5 | `ScheduleSystem` | Resolve active block + override stack |
| 6 | `EconomyAbstractSystem` | Cold NPC production/consumption formulas |
| 7 | `NpcTierPromotionSystem` | AOI enter/exit → Hot/Warm/Cold transitions + hydrate |
| 8 | `NpcDecisionSystem` | Hot/Warm only; staggered via `tickSlot` |
| 9 | `BehaviorTreeRunner` | Hot NPCs only; max N path requests/frame |
| 10 | `InteractionSystem` | Player↔NPC locks, trade sessions |
| 11 | `ReputationSystem` | Apply deferred rep deltas from events |
| 12 | `CommunalProjectSystem` | Progress, slot assignments |
| 13 | `StockpileSystem` | Deposits/withdrawals from completed activities |
| 14 | `ReplicationSnapshotBuilder` | Mark dirty entities per AOI cell |

**Separation of concerns:**

- **Simulation** — Steps 1–13; deterministic given inputs; no socket I/O.
- **Networking** — `NetTick`: AOI diff, encode, send; handle disconnects.
- **Persistence** — `PersistDrain`: async writes from a lock-free queue; never blocks SimTick.

### 2.3 Hot / Warm / Cold tiers in the server loop

Tier membership is computed each SimTick from **AOI manager** + **activity locks** (conversation, event, combat).

| Tier | Membership rule | Tick integration |
|------|-----------------|------------------|
| **Hot** | Inside any player AOI (64 m), or in active conversation/event | Full `NpcDecisionSystem` every 0.5–1 s (staggered); BT runner; pathfinding budget |
| **Warm** | Same zone, outside all AOIs | Needs + schedule every 2–4 s; utility only if off-routine |
| **Cold** | Distant slice / no nearby players | Abstract sim every 30–60 s: schedule block → yield/consumption formulas; no BT, no path |

#### Tier state machine (per NPC)

```
                    ┌─────────────┐
         ┌─────────►│    COLD     │◄─────────┐
         │          │ (abstract)  │          │
  no AOI │          └──────┬──────┘          │ no AOI
         │                 │ player AOI      │
         │                 ▼                 │
         │          ┌─────────────┐          │
         │          │    WARM     │──────────┤
         │          └──────┬──────┘          │
         │                 │ AOI overlap     │
         │                 ▼                 │
         │          ┌─────────────┐          │
         └──────────│     HOT     │──────────┘
                    │ (full AI)   │
                    └─────────────┘
```

**Hydration (Cold → Hot):** On promotion, reconstruct spatial state from abstract record: position at activity anchor, partial work progress, current coarse state, BT root from schedule block. Run one immediate full decision tick to avoid visible pop-in.

**Dehydration (Hot → Warm/Cold):** Snapshot position, blackboard keys, partial activity progress into abstract record; release BT instance to pool.

#### Staggering (from NPC decision spec)

```text
decisionDue = (currentTick - lastDecisionTick) >= tierInterval
           && (currentTick % STAGGER_BUCKETS) == (npcIndex % STAGGER_BUCKETS)
```

`STAGGER_BUCKETS = 10` spreads Hot evaluations across frames.

---

## 3. Database & Persistence Strategy

### 3.1 Database recommendation

| Phase | Database | Why |
|-------|----------|-----|
| **P0–P1** | **SQLite** (single file) | Zero ops overhead; perfect for local dev and solo testing; WAL mode for concurrent readers |
| **P2** | SQLite + migration tooling | Prove schemas before ops complexity |
| **P3+** | **PostgreSQL** | Concurrent player sessions, communal project transactions, analytics queries, backups |

Use an **ORM-agnostic repository layer** (`IWorldRepository`, `IPlayerRepository`) so the swap is a config change, not a rewrite.

### 3.2 Key entity schemas

Relational core + JSON blobs for high-churn NPC state (matches NPC save blob spec).

#### `worlds`

| Column | Type | Notes |
|--------|------|-------|
| `id` | UUID PK | |
| `name` | TEXT | |
| `seed` | INT | Procedural/static placement |
| `game_day` | INT | Current in-game day |
| `game_minute` | INT | Minutes since midnight |
| `season` | TEXT | spring/summer/autumn/winter |
| `weather` | JSONB | `{ type, intensity, endsAt }` |
| `version` | INT | Schema version |
| `updated_at` | TIMESTAMPTZ | |

#### `players`

| Column | Type | Notes |
|--------|------|-------|
| `id` | UUID PK | |
| `account_id` | UUID FK | Auth layer (external) |
| `world_id` | UUID FK | |
| `display_name` | TEXT | |
| `position` | JSONB | `{ x, y, z, zoneId }` |
| `rotation` | FLOAT | |
| `inventory` | JSONB | Item stacks |
| `village_reputation` | INT | 0–100 |
| `role_reputation` | JSONB | `{ "farmers": 42, "merchants": 10 }` |
| `last_seen_at` | TIMESTAMPTZ | |

#### `npcs`

| Column | Type | Notes |
|--------|------|-------|
| `id` | TEXT PK | e.g. `elsie` |
| `world_id` | UUID FK | |
| `role` | TEXT | farmer, merchant, child, … |
| `tier` | TEXT | hot/warm/cold (runtime cache, optional persist) |
| `coarse_state` | TEXT | Sleeping, Routine, NeedDriven, … |
| `position` | JSONB | Nullable when cold-abstract |
| `needs` | JSONB | `{ hunger: 72, energy: 45, … }` |
| `personality` | JSONB | Trait weights 0.0–1.0 |
| `schedule_template` | JSONB | Weekly blocks |
| `schedule_overrides` | JSONB | Override stack |
| `relationships` | JSONB | Sparse map `entityId → RelationshipState` |
| `memory` | JSONB | Recent events, gossip queue |
| `inventory` | JSONB | Personal + role resources |
| `abstract_state` | JSONB | `lastAbstractSimTime`, partial work, activity id |
| `active_quest_ids` | JSONB | |
| `updated_at` | TIMESTAMPTZ | |

#### `stockpiles`

| Column | Type | Notes |
|--------|------|-------|
| `id` | TEXT PK | e.g. `granary`, `lumber_yard` |
| `world_id` | UUID FK | |
| `location` | JSONB | Zone + anchor |
| `capacity` | INT | |
| `items` | JSONB | `{ wheat: 120, logs: 45 }` |
| `updated_at` | TIMESTAMPTZ | |

#### `communal_projects`

| Column | Type | Notes |
|--------|------|-------|
| `id` | UUID PK | |
| `world_id` | UUID FK | |
| `type` | TEXT | bridge, well, festival_stage, … |
| `organizer_id` | TEXT | NPC or player id |
| `resources_needed` | JSONB | `{ wood: 50, stone: 20 }` |
| `resources_contributed` | JSONB | Running totals |
| `slots` | INT | Max concurrent workers |
| `assigned_workers` | JSONB | NPC + player ids |
| `deadline_day` | INT | In-game day |
| `status` | TEXT | proposed/active/completed/failed |
| `progress` | FLOAT | 0.0–1.0 |
| `created_at` | TIMESTAMPTZ | |

#### `world_events` (append-only audit)

| Column | Type | Notes |
|--------|------|-------|
| `id` | BIGSERIAL PK | |
| `world_id` | UUID FK | |
| `event_type` | TEXT | quest_complete, theft, project_complete, … |
| `payload` | JSONB | |
| `game_day` | INT | |
| `created_at` | TIMESTAMPTZ | Drives rep aggregation + gossip |

#### `player_npc_relationships` (optional normalize from P3)

If JSONB sparse maps become heavy, extract rows: `(player_id, npc_id, affinity, trust, familiarity)`.

### 3.3 Save strategy

| Trigger | What is saved | Priority |
|---------|---------------|----------|
| **Periodic** (every 5 min) | Full world snapshot: all NPCs, stockpiles, projects, game time | Background queue |
| **Graceful shutdown** | Full flush + checkpoint marker | Blocking |
| **Event-driven** | Dirty entities: quest complete, relationship threshold, project milestone, day rollover | Enqueue immediately; batch per frame |
| **Day rollover** | All NPCs (drift, override refresh, gossip prune) + economy reconciliation | Single transaction |
| **Player logout** | No special case — world persists independently | — |

**Write path:** SimTick marks entities dirty → `PersistQueue` → async writer coalesces by entity id (last-write-wins per 5 s window for non-critical fields).

**Crash recovery:** On startup, load last checkpoint + replay `world_events` since checkpoint if event sourcing is enabled (optional P3 enhancement).

### 3.4 Versioning and migration

```text
schema_version: integer in worlds table
migration_scripts: /db/migrations/0001_initial.sql, 0002_add_communal_projects.sql, …
```

| Practice | Detail |
|----------|--------|
| **Forward-only migrations** | Numbered SQL files; never edit shipped migrations |
| **Dual-write window** | When renaming fields, write both keys for one release |
| **NPC blob versioning** | `abstract_state.version` for in-JSON evolution |
| **Save compatibility** | Server refuses join if `client_build < min_supported`; world loads with migration on boot |
| **Rollback** | Keep previous DB backup; migrations are not auto-reversed |

---

## 4. Networking & Replication

### 4.1 Recommended networking approach

| Layer | Recommendation |
|-------|----------------|
| Transport | **UDP** |
| Library | **LiteNetLib** (C#) or **ENet** — both mature, low overhead, channel support |
| Serialization | **MemoryPack**, **NetSerializer**, or hand-rolled `Span<byte>` structs for hot paths |
| Unity client | LiteNetLib directly, or **Netcode for GameObjects** only if you need its tooling and accept heavier stack |

**Why not TCP:** Head-of-line blocking hurts position updates; unreliable channels for frequent state are essential.

**Why not WebSocket (P0–P3):** Higher latency and framing overhead; reconsider only if browser client is a hard requirement.

### 4.2 Interest management / AOI

**Grid-based AOI** with cell size 32 m; subscribe radius 2 cells (64 m matches NPC Hot tier).

```
Zone divided into 32×32 m cells
Each entity registered in cell(x, z)
Each client subscribes to cells within 64 m of player position
On cell crossing: subscribe/unsubscribe + spawn/despawn messages
```

| Entity type | AOI behavior |
|-------------|--------------|
| Players | Always replicate to subscribed clients |
| Hot NPCs | Full sync bundle |
| Warm NPCs | Low-frequency position + state (2–4 s) |
| Cold NPCs | Not replicated until promotion |
| Stockpiles / projects | Replicate when within AOI or on change notification |
| Global events | Reliable broadcast (weather change, festival start) |

**Conversation lock:** When two NPCs (or NPC + player) enter dyad, both are force-promoted to Hot for subscribing clients regardless of strict distance.

### 4.3 Reliable vs unreliable channels

| Channel | Delivery | Use for |
|---------|----------|---------|
| **Unreliable** | Best-effort | Player/NPC position, rotation, animation phase, look-at |
| **Reliable ordered** | Guaranteed | Inventory changes, trade confirm, quest accept/complete, project contribute, dialogue lines |
| **Reliable unordered** | Guaranteed, no order | Chat, bark triggers, UI notifications |

**Rule of thumb:** High frequency + stale-is-worse-than-missing → unreliable. Economic or progression state → reliable ordered.

### 4.4 Snapshot vs delta replication

Use **hybrid**:

| Scenario | Strategy |
|----------|----------|
| Client joins / zone enter | **Full snapshot** of subscribed AOI (entities + baseline seq) |
| Steady state | **Delta** from last ack'd sequence per entity |
| Infrequent entities (projects, stockpiles) | **Dirty-flag snapshot** on change only |
| Hot NPC | Position delta @ 10 Hz; sync bundle @ 1 Hz or on state change |

#### Entity sync bundle (NPC)

Matches `NpcSyncState` component from NPC AI spec:

```text
entityId, position, velocity, coarseState, currentActionId,
animState, facing, needBands (coarse, not exact), interactionLock
```

Exact need values are server-only; clients receive bands (Fine/Low/Critical) for UI cues.

#### Delta encoding

```text
EntityDelta {
  entityId: uint
  seq: uint
  fieldMask: ushort     // which fields changed
  payload: bytes
}
```

Client acks `lastSeq` per entity; server keeps ring buffer of last 30 ticks for loss recovery. If gap detected, request full entity snapshot.

---

## 5. Entity & Component System

### 5.1 Recommended architecture

Use a **lightweight ECS** on the server: entities are integer IDs; components are plain structs/classes in sparse dictionaries or archetype chunks. **Do not** use Unity DOTS on the server unless the team already knows it—custom ECS in shared C# is sufficient and portable.

```
World
├── EntityManager        (create/destroy, component add/remove)
├── SystemScheduler      (ordered tick pipeline)
├── ComponentStores      (Transform, NpcIdentity, NpcNeeds, …)
└── EventBus             (cross-system messaging)
```

### 5.2 Core components

#### Shared (players + NPCs)

| Component | Key fields | Systems |
|-----------|------------|---------|
| `EntityIdentity` | `id`, `kind` (player/npc/object) | All |
| `Transform` | `position`, `rotation`, `zoneId`, `cellId` | Replication, AOI, pathfinding |
| `Inventory` | slots, weight | Economy, trade, projects |
| `ReplicationState` | `dirty`, `lastSeq`, `subscribers` | NetTick |

#### Player-specific

| Component | Key fields | Systems |
|-----------|------------|---------|
| `PlayerConnection` | `peerId`, `latency`, `lastInputSeq` | NetTick |
| `PlayerReputation` | `village`, `roleMap` | Reputation, dialogue, prices |
| `PlayerInput` | pending commands | Interaction, movement |

#### NPC-specific (from NPC AI spec)

| Component | Key fields | Systems |
|-----------|------------|---------|
| `NpcIdentity` | `role`, `homeId`, `workplaceId` | Schedule, economy |
| `NpcNeeds` | 4–8 need values, bands | NeedsDecay, utility |
| `NpcSchedule` | template, overrides, active block | Schedule |
| `NpcPersonality` | trait weights | UtilityEvaluator |
| `NpcRelationships` | sparse relationship map | Reputation, dialogue, gossip |
| `NpcMemory` | events, gossip queue | Dialogue, utility MemoryFit |
| `NpcStateMachine` | coarse state | PreemptionScanner |
| `NpcUtilityBrain` | last action, scores cache | NpcDecisionSystem |
| `NpcPathAgent` | nav state, BT runner ref | BehaviorTreeRunner |
| `NpcSyncState` | client-facing bundle | Replication |
| `NpcTier` | hot/warm/cold, last tick times | NpcTierPromotion |
| `NpcProduction` | role hooks, partial work | EconomyAbstract |

#### World / infrastructure

| Component | Key fields | Systems |
|-----------|------------|---------|
| `Stockpile` | items, capacity | StockpileSystem |
| `CommunalProject` | resources, slots, workers | CommunalProjectSystem |
| `Interactable` | radius, type, locks | InteractionSystem |
| `ScheduleAnchor` | location id, activity bindings | Schedule, BT |

### 5.3 Component interaction with simulation loop

Components are **passive data**. **Systems** read/write them in the scheduled order (§2.2). Cross-cutting reactions use the **EventBus** rather than systems calling each other directly.

```
NpcDecisionSystem
  reads:  NpcNeeds, NpcSchedule, NpcPersonality, NpcRelationships, NpcStateMachine, NpcTier
  writes: NpcUtilityBrain, NpcStateMachine, NpcPathAgent (action dispatch)
  emits:  ActionSelected, StateChanged

BehaviorTreeRunner
  reads:  NpcPathAgent, Transform, ScheduleAnchor
  writes: Transform, NpcProduction (work progress), NpcSyncState
  emits:  ActivityCompleted, PathFailed, DepositProduced

ActivityCompleted → StockpileSystem (deposit) → EconomySystem (ledger)
ActivityCompleted → EventBus → ReputationSystem (role trust)
```

Keep `UtilityEvaluator` **pure** (no side effects) for unit testing, per NPC decision spec.

---

## 6. Data Flow & Integration Points

### 6.1 Cross-system data flow

```
┌─────────────┐     gossip / social      ┌─────────────┐
│   NPC AI    │◄────────────────────────►│ Reputation  │
│ (needs,     │  player deeds, theft     │ (village,   │
│  schedule,  │─────────────────────────►│  personal,  │
│  utility)   │  attitude for dialogue   │  role)      │
└──────┬──────┘                          └──────┬──────┘
       │                                          │
       │ production/consumption                   │ price modifiers
       ▼                                          ▼
┌─────────────┐     stock levels        ┌─────────────┐
│  Economy    │◄───────────────────────►│  Merchant / │
│ (abstract + │  low stock → schedule   │  Shop       │
│  hot hooks) │  override triggers      └─────────────┘
└──────┬──────┘
       │ deposits
       ▼
┌─────────────┐     join utility        ┌─────────────┐
│ Stockpiles  │────────────────────────►│  Communal   │
│             │◄────────────────────────│  Projects   │
└─────────────┘  resource drawdown      └──────┬──────┘
                                               │
       ┌───────────────────────────────────────┘
       │ NPC Communal state, schedule overrides
       ▼
┌─────────────┐     AOI, player actions   ┌─────────────┐
│ Multiplayer │◄─────────────────────────►│ Interaction │
│ (replication│  commands, snapshots      │ (trade, talk,│
│  AOI)       │                           │  build)     │
└─────────────┘                           └─────────────┘
```

### 6.2 Event bus design

**In-process pub/sub** on the game server. No external message broker through P3.

#### Event categories

| Domain | Example events | Subscribers |
|--------|----------------|-------------|
| **NPC life** | `NeedBecameCritical`, `ActivityCompleted`, `ScheduleOverridePushed` | Economy, Reputation, Project, Replication |
| **Player** | `PlayerInteracted`, `GiftGiven`, `TheftWitnessed` | NPC AI (Reacting), Reputation, Memory |
| **Economy** | `StockpileLow`, `DepositReceived`, `ShopTransaction` | Schedule overrides, Questgen, UI sync |
| **Reputation** | `VillageRepChanged`, `RelationshipThresholdCrossed` | Dialogue, prices, quest availability |
| **Projects** | `ProjectProposed`, `ContributionMade`, `ProjectCompleted` | NPC utility (join), Reputation, Festival |
| **World** | `HourAdvanced`, `DayRollover`, `WeatherChanged` | Needs, schedules, environment cache |

#### Event envelope

```csharp
record DomainEvent(
    Guid EventId,
    string Type,
    int GameDay,
    int GameMinute,
    string SourceEntityId,
    JsonObject Payload,
    EventPriority Priority  // Immediate (same tick) vs Deferred (next tick)
);
```

**Immediate** events (theft witnessed, player interaction lock) process in current SimTick step 2. **Deferred** events (gossip spread, rep drift) process next tick to avoid re-entrancy bugs.

### 6.3 Key integration contracts

| Integration | Contract |
|-------------|----------|
| NPC AI → Economy | `ActivityCompleted` carries `{ npcId, activity, outputs[], zoneId }`; Economy validates against role hooks |
| Economy → NPC AI | `StockpileLow` pushes schedule override onto role-relevant NPCs |
| Reputation → NPC AI | `PlayerReputation` + per-NPC `RelationshipState` feed `RelationshipFit` utility term and greeting selection |
| Reputation → Economy | Village rep maps to price multiplier: `price = base * (1.0 - 0.002 * villageRep)` (tunable) |
| Projects → NPC AI | `CommunalProject` slots exposed to `CandidateGenerator`; join creates override block + `Communal` state |
| Multiplayer → All | Player commands validated → domain events → standard system chain |

---

## 7. Tech Stack Recommendations

### 7.1 Recommended stack

| Layer | Choice | Alternatives |
|-------|--------|--------------|
| **Game client** | **Unity 6** (C#) | Godot 4 (if team prefers OSS; fewer life-sim assets) |
| **Game server** | **.NET 8** headless console | Same Unity project headless (faster prototype, messier long-term) |
| **Shared code** | **.NET class library** (`Bloomtown.Shared`) | Protocol types, utility math, need formulas, serialization |
| **Networking** | **LiteNetLib** | ENet-CSharp, FishNet (if using Netcode) |
| **Pathfinding** | **DotRecast** or exported Unity NavMesh → server mesh | A* on grid (fallback for P0 spike) |
| **Behavior trees** | **Custom minimal BT** or NodeCanvas/BT Designer (export JSON) | |
| **Database** | **SQLite** → **PostgreSQL** | |
| **DB access** | **Dapper** or EF Core (migrations) | |
| **Logging** | **Serilog** (structured) | |
| **Config/data** | JSON + optional **YAML** for schedules | Google Sheets → JSON export for designers |

### 7.2 Solo developer vs small team

| Concern | Solo | Small team (2–4) |
|---------|------|-------------------|
| **Engine** | Unity + shared C# server lib | Same; split client UX vs server sim |
| **Server hosting** | Local + single VPS ($10–20/mo) | Staging + prod VPS or managed container |
| **Pipelines** | Manual deploy; SQLite in dev | CI build client+server; Postgres in staging |
| **NPC authoring** | JSON schedules in repo | Spreadsheet export + validation tool |
| **Testing** | Unit tests for utility/rep; 1-bot integration test | + dedicated soak test (10 simulated clients) |
| **Scope guard** | P0–P1 only until fun proven | Parallel: art + server systems |

**Solo-dev critical path:** Shared protocol library first → one runnable loop with 1 NPC before polishing client visuals.

### 7.3 What to avoid early

- Microservices or Kubernetes (ops cost >> benefit before P4)
- Full DOTS/server ECS frameworks (learning curve)
- TCP-only networking
- Client-authoritative NPCs "for performance"
- Building zone workers before single-zone profiling proves need

---

## 8. Phased Technical Implementation

Aligned with Overall Implementation Roadmap and Sprint Plan P0–P1.

### 8.1 Phase summary

| Phase | Game scope (reminder) | Technical deliverables |
|-------|----------------------|------------------------|
| **P0** | Walkable village, connect, time passes | Server skeleton, net spike, world load, player movement |
| **P1** | 5–10 NPCs, 4 needs, schedules | NPC components, Hot tier AI, AOI replication, SQLite |
| **P2** | 10–15 NPCs, gossip, overrides, quests | Warm/Cold tiers, event bus, quest persistence |
| **P3** | Reputation, relationships, communal projects | Postgres migration, project system, relationship normalization |
| **P4** | Economy loops, weather, personality depth | Abstract economy at scale, weather pipeline, telemetry |

### 8.2 P0 — Foundation (Sprint 0–2)

**Goal:** Prove multiplayer loop in empty village.

| Component | Deliverable |
|-----------|-------------|
| Repo layout | `Bloomtown.Client`, `Bloomtown.Server`, `Bloomtown.Shared` |
| Sim loop | Fixed 20 Hz tick, game clock (accelerated day/night) |
| Networking | LiteNetLib connect, unreliable movement, reliable chat ping |
| Player entity | Spawn, move, AOI subscribe, disconnect cleanup |
| World | Static zone, placed anchors (home, farm, tavern), no NPCs |
| Persistence | SQLite `worlds` + `players`; position save on shutdown |
| Tooling | Headless server + Unity client from same solution |

**Exit criteria:** 2 clients see each other move; server runs 30+ minutes without drift.

### 8.3 P1 — NPC Life Baseline (Sprint 3–6)

**Goal:** Elsie walks a schedule, gets hungry, eats at tavern; visible to multiple players.

| Component | Deliverable |
|-----------|-------------|
| NPC ECS | Components through `NpcSyncState` |
| Needs | Hunger, Energy, Social, Safety decay |
| Schedule | Weekly template, block resolution, activity anchors |
| AI (Hot only) | State machine + utility (Base + NeedUrgency + ScheduleFit) + simple BT (path, eat, work anim) |
| Replication | NPC sync bundle, animation state |
| AOI | 64 m grid; Hot promotion |
| Content | 5–10 NPCs from roster (Elsie, Tom, Mira, Harold, Greta minimum) |
| Persistence | `npcs` table with JSON blobs; 5-min periodic save |

**Exit criteria:** 2 players watch same NPC make lunch decision; server restart restores NPC state.

### 8.4 P2 — Depth & Scale (Sprint 7–10)

| Component | Deliverable |
|-----------|-------------|
| AI tiers | Warm/Cold + abstract sim + hydrate/dehydrate |
| Decision system | Full preemption (PR-01–PR-09), hysteresis, pause/resume BT |
| Needs | + Comfort, Purpose |
| Gossip | `GossipItem` spread on converse |
| Overrides | Weather, stockpile-low, event stack |
| Quests | `NpcRequest` fetch/deliver; event-driven save |
| Economy hooks | Production on activity complete → stockpile deposit |
| Event bus | Domain events with immediate/deferred |
| Performance | Stagger ticks, path queue max 4/frame, 12 candidate cap |

**Exit criteria:** 15 NPCs stable at 20 players; Cold NPCs produce correct stock when unobserved.

### 8.5 P3 — Social Fabric (Sprint 11–15)

| Component | Deliverable |
|-----------|-------------|
| Reputation | Personal, village, role layers; price + dialogue hooks |
| Relationships | Full `RelationshipState`; player↔NPC persistence |
| Communal projects | CRUD, contributions, worker slots, join utility |
| NPC communal | `Communal` state, PR-07, schedule overrides |
| Postgres | Migration from SQLite; connection pooling |
| Replication | Project + stockpile dirty snapshots |
| Quests | Communal donate, repair, escort types |

**Exit criteria:** Player-led bridge project completes with NPC help; village rep affects shop prices.

### 8.6 P4 — Living Economy (Sprint 16+)

| Component | Deliverable |
|-----------|-------------|
| Needs | + Fun, Health; child/elder rules |
| Weather/seasons | Environment cache → override + need multipliers |
| Economy loops | Merchant markup, tool quality, winter fuel |
| Personality | Full utility terms (Relationship, Memory, Environment) |
| Telemetry | Stuck NPC, decision traces, economy dashboards |
| Scale prep | Profile single zone; prototype zone handoff if needed |

**Exit criteria:** 7-day soak sim with stable stockpiles, no NPC stuck >5 min, economy metrics logged.

### 8.7 Early technical spikes (do first)

Run these in **P0 Sprint 0** before feature work:

| Spike | Timebox | Proves |
|-------|---------|--------|
| **S1: Headless server tick** | 3–5 days | Fixed loop, game time, logging |
| **S2: UDP movement replication** | 3–5 days | 2 clients, prediction, reconcile |
| **S3: AOI grid** | 2–3 days | Subscribe/unsubscribe on cell cross |
| **S4: One NPC schedule path** | 5 days | Schedule → BT → transform updates |
| **S5: SQLite round-trip** | 1–2 days | World + NPC save/load |
| **S6: Utility unit tests** | 2 days | Pure evaluator against golden snapshots |

**Order:** S1 → S2 → S4 (local only) → S3 → S5 → S6.

---

## Appendix A — Protocol sketch (P0)

```text
C→S  PlayerInput       { seq, moveDir, lookYaw, buttons }
C→S  InteractRequest   { targetEntityId, interactType }
S→C  WorldSnapshot     { gameTime, entities[] }
S→C  EntityDelta       { entityId, seq, fieldMask, payload }
S→C  EventMessage      { reliable: dialogue, trade, quest, project }
```

## Appendix B — Configuration defaults

| Parameter | Default | Source |
|-----------|---------|--------|
| AOI radius | 64 m | NPC AI spec |
| Hot decision interval | 0.5–1.0 s | NPC AI spec |
| Warm decision interval | 2–4 s | NPC AI spec |
| Cold abstract interval | 30–60 s | NPC AI spec |
| Sim tick rate | 20 Hz | This doc |
| Net send rate | 10 Hz (unreliable) | This doc |
| Periodic save | 5 min | NPC AI spec |
| Max hot NPCs per cluster | 8 | NPC AI perf target |
| Max path requests / frame | 4 | NPC AI perf target |
| Utility hysteresis | 8 | Decision system spec |
| Utility min threshold | 40 | Decision system spec |

## Appendix C — Related documents

| Document | Relationship |
|----------|--------------|
| High Level System Design | Game pillars, scope boundaries |
| NPC AI & Daily Life | Component stack, tiers, schedules, needs |
| NPC Decision System | Preemption, utility formula, BT coupling |
| Reputation System | Three-layer model, aggregation rules |
| Communal Projects | Project schema, join utility, worker slots |
| Multiplayer Foundation | Player count targets, server authority |
| Economy System | Stockpiles, abstract yield, merchant markup |
| Sprint Plan P0–P1 | Sprint-level task breakdown |
| Overall Implementation Roadmap | Phase gates and NPC roster |

---

*This architecture is intentionally conservative: one server process, one village zone, and SQLite until multiplayer persistence proves otherwise. Scale-out paths are documented but deferred until profiling says they are necessary.*