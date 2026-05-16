# ClassicSteveMobs

> Adds a classic blocky Steve-style test mob to CastleMiner Z using CastleForge ModLoader.  
> Spawn them with a command, watch them wander around with rd-132328-inspired movement, and optionally allow rare natural aboveground spawns.

**Current mod version shown in source:** `0.0.1.0`

<p align="center">
  <img src="ClassicSteveMobs/_Images/Showcase1.gif" alt="Circle Mode" width="49%" />
  <img src="ClassicSteveMobs/_Images/Showcase2.gif" alt="Square mode" width="49%" />
</p>

---

## Contents

- [Overview](#overview)
- [Why this mod stands out](#why-this-mod-stands-out)
- [Features at a glance](#features-at-a-glance)
- [Requirements](#requirements)
- [Installation](#installation)
- [Quick start](#quick-start)
- [Command reference](#command-reference)
- [Configuration](#configuration)
- [Behavior notes](#behavior-notes)
- [Multiplayer notes](#multiplayer-notes)
- [Technical overview](#technical-overview)
- [Known limitations](#known-limitations)
- [Credits](#credits)

---

## Overview

**ClassicSteveMobs** is a small experimental CastleForge gameplay mod that adds a simple blocky humanoid test mob inspired by the very early Minecraft pre-classic / rd-132328 mob style.

The goal is to make a goofy early-game creature that feels different from normal CastleMiner Z zombies without needing a full new enemy system. It keeps CastleMiner Z's existing enemy handling for things like spawning, networking, collision, damage, death, and despawn behavior, but replaces the visible model and chase behavior for the test enemy.

At its core, the mod:

- registers the unused `EnemyTypeEnum.TREASURE_ZOMBIE` slot as a custom test mob,
- draws the mob as a simple six-part blocky character,
- replaces normal zombie chasing with random wandering,
- lets you spawn mobs directly with `/spawnsteve`,
- supports large square-ish command spawn batches,
- includes configurable movement, jumping, enemy stats, rendering scale, and hot-reload settings,
- can optionally replace a small percentage of aboveground natural spawns.

This makes it useful as both a fun test mod and a working example for future custom mob experiments in CastleForge.

---

## Why this mod stands out

ClassicSteveMobs is intentionally simple, but it demonstrates several important CastleForge modding ideas:

- **Custom enemy slot registration**  
  The mod reuses `TREASURE_ZOMBIE` as a test enemy slot instead of adding an unsafe new network enum value.

- **Custom rendering without a skinned model pipeline**  
  The blocky mob is rendered with simple cuboid parts instead of requiring a full CMZ skinned XNB model with animation clips.

- **Different behavior from normal zombies**  
  The mob does not constantly chase the player. It uses a random wander state with occasional hops and wall-bump turns.

- **Safe command-driven testing**  
  Natural spawning is disabled by default, so you can test the mob with commands before allowing it into normal gameplay.

- **Runtime tuning**  
  Movement, jumping, spawn chance, scale, and other values can be edited in the INI file and reloaded while in game.

- **Large group spawning**  
  The command spawner lays mobs out in a square-ish grid so spawning hundreds does not create a single long line.

---

## Features at a glance

| Feature                   | What it does                                                             |
|---------------------------|--------------------------------------------------------------------------|
| `/spawnsteve` command     | Spawns one or more classic Steve mobs in front of the local player       |
| `/steve` alias            | Short alias for `/spawnsteve`                                            |
| Square-ish batch spawning | Large spawn counts are arranged into a compact grid                      |
| Custom blocky renderer    | Draws a simple six-cube humanoid model instead of a vanilla zombie model |
| Random wander AI          | Replaces normal zombie chasing with random movement                      |
| Random hopping            | Configurable jump chance and cooldown for early-test-mob style movement  |
| Wall bump behavior        | Mobs turn and hop when they hit terrain                                  |
| Optional natural spawns   | Can replace a small percent of aboveground enemy spawns when enabled     |
| Config hot-reload         | Reloads settings with `/stevemob reload` or the configured hotkey        |
| Runtime info command      | Shows current enabled/spawn/rendering state with `/stevemob info`        |

---

## Requirements

ClassicSteveMobs is built for the CastleForge ecosystem and requires:

- **CastleMiner Z**
- **CastleForge ModLoader**
- **ModLoaderExtensions**

The mod declares `ModLoaderExtensions` as a required dependency because it uses the shared CastleForge command infrastructure.

Recommended target environment:

| Item         | Recommended value                                                                |
|--------------|----------------------------------------------------------------------------------|
| Game version | `1.9.9.8`                                                                        |
| Framework    | CastleForge / ModLoader setup                                                    |
| Dependency   | `ModLoaderExtensions`                                                            |
| Multiplayer  | All clients should install the mod before joining a world that spawns this enemy |

---

## Installation

1. Install and verify **CastleForge ModLoader**.
2. Install **ModLoaderExtensions**.
3. Place the ClassicSteveMobs DLL in your CastleMiner Z `!Mods` folder.
4. Launch the game once.
5. The mod will create its config folder and extract/use its resources.
6. Enter a world and test with `/spawnsteve`.

### Expected mod folder

After first launch, the mod uses:

```text
!Mods\ClassicSteveMobs\
```

Expected files include:

```text
!Mods\ClassicSteveMobs\ClassicSteveMobs.ini
!Mods\ClassicSteveMobs\Textures\char.png
```

> Depending on your packaging flow, the main `ClassicSteveMobs.dll` may sit directly in `!Mods`, while the config and texture files live under `!Mods\ClassicSteveMobs\`.

---

## Quick start

Once you are in a world, spawn one mob:

```text
/spawnsteve
```

Spawn a small group:

```text
/spawnsteve 10
```

Spawn a large test group:

```text
/spawnsteve 200
```

Reload the config after editing the INI file:

```text
/stevemob reload
```

Check runtime settings:

```text
/stevemob info
```

---

## Command reference

| Command                | Usage              | What it does                                                                   |
|------------------------|--------------------|--------------------------------------------------------------------------------|
| `/spawnsteve`          | `/spawnsteve`      | Spawns one classic Steve mob in front of you                                   |
| `/spawnsteve <amount>` | `/spawnsteve 25`   | Spawns multiple classic Steve mobs in a square-ish grid                        |
| `/steve`               | `/steve 10`        | Alias for `/spawnsteve`                                                        |
| `/stevemob reload`     | `/stevemob reload` | Reloads the ClassicSteveMobs config file                                       |
| `/stevemob info`       | `/stevemob info`   | Shows current enabled, natural spawn, spawn chance, scale, and enemy slot info |

### Spawn count behavior

The source currently allows large command batches and clamps command spawns internally. Large values are useful for stress testing, but they can hurt FPS and create heavy enemy load.

Recommended test values:

```text
/spawnsteve 1
/spawnsteve 10
/spawnsteve 50
/spawnsteve 200
```

Very large values should be treated as stress tests, not normal gameplay.

---

## Configuration

ClassicSteveMobs creates this config on first launch:

```text
!Mods\ClassicSteveMobs\ClassicSteveMobs.ini
```

### Default config

```ini
; ClassicSteveMobs - test rd-132328-style mob config
; Uses EnemyTypeEnum.TREASURE_ZOMBIE as the custom test slot.

[ClassicSteveMobs]
; Master toggle for patches, command spawning, and natural spawning.
Enabled              = true
; If true, some aboveground night spawns become classic Steve mobs.
NaturalSpawns        = false
; 0.02 = 2% chance when NaturalSpawns is enabled.
NaturalSpawnChance   = 0.02
; Distance in front of the local player for /spawnsteve.
CommandSpawnDistance = 5

[EnemyStats]
; Low test health to match early-game/simple mob behavior.
Health          = 2
SpawnRadius     = 20
DistanceLimit   = 40
SlowSpeed       = 1.75
RandomSlowSpeed = 0.5
FastSpeed       = 3.0
HasRunFast      = false

[Behavior]
; Random wandering values. The mob does not chase the player in this test version.
WanderTurnIntervalMin   = 0.5
WanderTurnIntervalMax   = 2.0
; Maximum signed heading change when the wander timer rolls over. 1.5 radians is about 86 degrees.
WanderTurnAmountRadians = 1.5
; Vertical velocity added when the mob bumps a wall or randomly hops.
WanderJumpSpeed         = 7.0
WanderJumpCooldown      = 0.35
; Chance per second to hop while walking on flat ground. 0 disables random hops.
WanderRandomJumpChance  = 1.25

[Rendering]
; Classic model height is 32 texture/model units. 0.065 makes it about CMZ-player sized.
ModelScale       = 0.065
; Original rd-132328 style animation uses time * 10.
AnimationSpeed   = 10
; If the model faces backward in-game, try 180.
YawOffsetDegrees = 0

[Hotkeys]
; Reload this config while in-game:
ReloadConfig = Ctrl+Shift+R
```

### Config reference

| Section            | Key                       | Default        | What it controls                                                              |
|--------------------|---------------------------|---------------:|-------------------------------------------------------------------------------|
| `ClassicSteveMobs` | `Enabled`                 | `true`         | Master toggle for the mod                                                     |
| `ClassicSteveMobs` | `NaturalSpawns`           | `false`        | Allows rare natural aboveground Steve spawns                                  |
| `ClassicSteveMobs` | `NaturalSpawnChance`      | `0.02`         | Chance to replace an aboveground enemy spawn when natural spawning is enabled |
| `ClassicSteveMobs` | `CommandSpawnDistance`    | `5`            | Distance in front of the player used by `/spawnsteve`                         |
| `EnemyStats`       | `Health`                  | `2`            | Enemy health value                                                            |
| `EnemyStats`       | `SpawnRadius`             | `20`           | Enemy spawn radius setting applied to the custom enemy type                   |
| `EnemyStats`       | `DistanceLimit`           | `40`           | Distance limit before the enemy gives up/despawns                             |
| `EnemyStats`       | `SlowSpeed`               | `1.75`         | Base wander movement speed                                                    |
| `EnemyStats`       | `RandomSlowSpeed`         | `0.5`          | Extra randomized movement speed range                                         |
| `EnemyStats`       | `FastSpeed`               | `3.0`          | Fast speed value retained on the enemy type                                   |
| `EnemyStats`       | `HasRunFast`              | `false`        | Whether the enemy type reports fast-run support                               |
| `Behavior`         | `WanderTurnIntervalMin`   | `0.5`          | Minimum seconds between random turn decisions                                 |
| `Behavior`         | `WanderTurnIntervalMax`   | `2.0`          | Maximum seconds between random turn decisions                                 |
| `Behavior`         | `WanderTurnAmountRadians` | `1.5`          | Maximum signed heading change on normal turns                                 |
| `Behavior`         | `WanderJumpSpeed`         | `7.0`          | Upward velocity added when the mob hops                                       |
| `Behavior`         | `WanderJumpCooldown`      | `0.35`         | Minimum time between jumps                                                    |
| `Behavior`         | `WanderRandomJumpChance`  | `1.25`         | Per-second random jump chance while grounded                                  |
| `Rendering`        | `ModelScale`              | `0.065`        | Visual scale of the blocky model                                              |
| `Rendering`        | `AnimationSpeed`          | `10`           | Arm/leg swing animation speed                                                 |
| `Rendering`        | `YawOffsetDegrees`        | `0`            | Extra yaw correction if the model faces the wrong way                         |
| `Hotkeys`          | `ReloadConfig`            | `Ctrl+Shift+R` | Hotkey used to reload the INI while in game                                   |

### Hot-reload workflow

After editing the INI, use either:

```text
/stevemob reload
```

or press the configured hotkey:

```text
Ctrl + Shift + R
```

Hot-reload is intended for config/runtime values only. If you rebuild or change C# code, restart the game after replacing the DLL.

---

## Behavior notes

ClassicSteveMobs does **not** use normal CMZ zombie chasing for this custom enemy.

Instead, the custom enemy type replaces the chase/restart state with a random wandering state:

1. The mob picks a heading.
2. It walks forward at a randomized speed.
3. It occasionally changes direction.
4. It can randomly hop while grounded.
5. If it bumps terrain, it turns away and may hop.
6. If it gets too far away from the local target, it gives up/despawns using normal CMZ enemy behavior.

### Making the mob jump more often

For a more chaotic early-test-mob feel, increase the random jump chance and lower the cooldown:

```ini
[Behavior]
WanderJumpSpeed        = 7.25
WanderJumpCooldown     = 0.35
WanderRandomJumpChance = 1.5
```

For a very active test swarm:

```ini
[Behavior]
WanderJumpSpeed        = 7.5
WanderJumpCooldown     = 0.20
WanderRandomJumpChance = 2.0
```

### Natural spawning

Natural spawning is disabled by default:

```ini
NaturalSpawns = false
```

To allow rare natural aboveground spawns:

```ini
[ClassicSteveMobs]
NaturalSpawns      = true
NaturalSpawnChance = 0.02
```

A value of `0.02` means roughly a 2% replacement chance when the patched aboveground spawn selection runs.

---

## Multiplayer notes

ClassicSteveMobs is safest when **every player has the mod installed**.

The mod reuses `EnemyTypeEnum.TREASURE_ZOMBIE` as a test slot. Modded clients know how to register and render that slot, but an unmodded client may not understand the custom behavior/rendering setup.

Recommended multiplayer usage:

- Host and clients should all install the same version of the mod.
- Keep `NaturalSpawns = false` until everyone has confirmed the mod loads correctly.
- Use `/spawnsteve` for controlled testing before enabling natural spawns.
- Avoid very large spawn batches on public or lower-end sessions.

---

## Technical overview

ClassicSteveMobs works by patching a few focused points in the CMZ enemy/rendering flow.

### Enemy slot

The mod registers:

```text
EnemyTypeEnum.TREASURE_ZOMBIE
```

as the classic Steve test mob slot.

This is safer than inventing a brand-new enum value because CastleMiner Z serializes enemy types over the network as a small enum/byte-style value.

### Enemy type

The custom enemy type derives from CMZ's zombie enemy type so it can keep the normal enemy lifecycle:

- health,
- damage,
- hit detection,
- collision,
- death,
- despawn,
- enemy manager registration,
- spawn messaging.

The main behavior difference is that the chase/restart state returns the custom wander state instead of the normal target-following chase state.

### Rendering

The mod patches the skinned model draw path and checks whether the current enemy is the custom Steve mob.

If it is not the custom mob, vanilla rendering continues normally.

If it is the custom mob, the mod skips the vanilla zombie model and draws a simple six-cuboid character instead.

### Spawning

Command spawning uses CMZ's normal `EnemyManager.SpawnEnemy(...)` path. This keeps enemy IDs and world ownership consistent with the rest of the game.

For multiple command spawns, the mod computes a square-ish grid using the total requested amount. For example:

```text
20 mobs  -> about 5 x 4
100 mobs -> about 10 x 10
200 mobs -> about 15 x 14
```

---

## Known limitations

- This is still an experimental custom enemy test mod.
- Natural spawning is disabled by default and should be enabled carefully.
- Very large spawn counts can reduce performance.
- Multiplayer sessions should require all clients to install the mod.
- The custom renderer is intentionally simple and does not use CMZ's skinned model animation system.
- If the model faces backward, set `YawOffsetDegrees = 180` in the config.

---

## Credits

- **RussDev7** - CastleForge / ClassicSteveMobs implementation
- **CastleForge ModLoader** - mod loading and runtime framework
- **ModLoaderExtensions** - shared command infrastructure