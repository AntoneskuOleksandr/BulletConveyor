# Bullet Conveyor

**A Unity 3D defense-game prototype built around a physical ammunition economy and modular weapon systems.**

Instead of giving every turret unlimited ammunition, **Bullet Conveyor** connects combat to production: arrange and merge blocks to produce bullets, move them along a conveyor, and supply weapons while surviving enemy waves.

This is a **personal, playable development prototype**, with an Android portfolio build. Its main purpose here is to demonstrate gameplay programming, extensible C# systems, and Unity architecture.

## Android portfolio snapshot 0.1.1

Five configured environments with six progressively introduced enemy types, selected-weapon previews, a shared 1x/3x clock, eight weapons with five upgrade tiers each, persistent upgrades, music and combat feedback, and a six-step interactive tutorial with dynamic gestures, skip and replay. Enemy placement checks navigation, obstacles and other agents; waves complete after the last living enemy.

Start from MainMenu. A fresh profile enters guided training when Play is pressed. The Tutorial button replays it, and Sound toggles audio without changing progress. See [milestones and verification](docs/MILESTONES.md) and [Android build instructions](docs/ANDROID_BUILD.md).

## Gameplay video

[![Watch Bullet Conveyor gameplay on YouTube](https://img.youtube.com/vi/8ZuzkB-nJ4s/hqdefault.jpg)](https://www.youtube.com/watch?v=8ZuzkB-nJ4s)

**[Watch the gameplay video on YouTube](https://www.youtube.com/watch?v=8ZuzkB-nJ4s)**

## Gameplay at a glance

1. **Build your ammunition supply.** Buy, drag, position, and merge production blocks on a grid; placed blocks contribute to periodic projectile generation.
2. **Feed your defenses.** Projectiles move through a conveyor-and-stack system. Weapons draw from this limited shared ammunition supply.
3. **Defend against waves.** Equip weapons with different shooting behaviours and survive enemies that navigate toward them.
4. **Improve your loadout.** Unlock and select weapons, upgrade their stats, and choose in-run abilities that affect projectile effects.

## Engineering highlights

### Data-driven, modular weapons

Weapon configuration is defined in [`WeaponSO`](Assets/Project/Scripts/Weapons/WeaponSO.cs), including stats, firing parameters, strategies, modifiers, and upgrade levels. [`WeaponFactory`](Assets/Project/Scripts/Weapons/WeaponFactory.cs) composes the runtime `Weapon` from that configuration rather than requiring a dedicated weapon script for every variation.

The implementation separates several independently configurable behaviours:

| Concern | Implementations in the repository |
| --- | --- |
| **Shooting** | Basic and multi-barrel |
| **Aiming** | Basic and ballistic, with idle aiming support |
| **Targeting** | Nearest-enemy selection |
| **Projectile movement** | Straight, arc, instant beam, continuous beam |
| **Hit handling** | Direct hit and area-of-effect |
| **Weapon modifiers** | Spread, multiple pellets, periodic damage boost, ramping fire rate |

Explore: [weapon code](Assets/Project/Scripts/Weapons/) · [strategy interfaces](Assets/Project/Scripts/Weapons/Interfaces/) · [strategy implementations](Assets/Project/Scripts/Weapons/Strategies/) · [modifiers](Assets/Project/Scripts/Weapons/Modifiers/).

### A shared ammunition economy

Weapons cannot simply fire whenever their cooldown expires. The project connects block production, a conveyor belt, a projectile stack, and weapon firing requests:

```text
Production blocks on board
          |
          v
Periodic projectile generation
          |
          v
Conveyor belt -> projectile stack
                        |
                        v
                 ProjectileManager
                        |
                        v
          FireDispatcher (priority + order)
                        |
                        v
              Weapon shooting strategy
                        |
                        v
          Movement / hit / status effects
```

[`FireDispatcher`](Assets/Project/Scripts/Weapons/FireDispatcher.cs) queues requests and orders them by weapon priority, then request sequence. [`ProjectileManager`](Assets/Project/Scripts/Bullets/ProjectileManager.cs) allocates available projectiles within requested minimum/maximum counts. This keeps ammunition allocation separate from a weapon's shooting behaviour.

Explore: [block interactions](Assets/Project/Scripts/BlockRelated/) · [drag state machine](Assets/Project/Scripts/Managers/Input/) · [conveyor](Assets/Project/Scripts/ConveyorBelt.cs) · [projectile generator](Assets/Project/Scripts/ProjectileGenerator.cs) · [projectile stack](Assets/Project/Scripts/ProjectileStack.cs).

### Combat effects and enemy waves

- **Status effects:** fire, ice, freeze, and lightning, with configurable effect data and chance-based application.
- **Wave management:** level-configured enemy types/counts, combat-complete waves, validated enemy spawning, and progress events.
- **Enemy behaviour:** NavMesh-based movement, target selection, health, damage, and death handling.
- **Game flow:** a tutorial, weapon loadout selection, upgrades, ability choices, and win/lose UI.

Explore: [effects](Assets/Project/Scripts/Debuffs/) · [waves](Assets/Project/Scripts/Managers/WaveManager.cs) · [enemies](Assets/Project/Scripts/Enemies/) · [level configuration](Assets/Project/Scripts/Level/LevelConfig.cs).

### Dependency management and progression

- [Zenject installers](Assets/Project/Scripts/Installers/) bind shared services and gameplay components.
- [Weapon and progression services](Assets/Project/Scripts/Services/) separate purchasing, selection, upgrades, and persistent player progress from combat behaviours.
- [`DataRepository`](Assets/Project/Scripts/DataManager/DataRepository.cs) uses JSON files under `Application.persistentDataPath` for player-data storage.
- DOTween animates UI and ammunition transport. Live generated ammunition is capped, and fired projectiles have a finite lifetime.

## Technology

| Area | Technology |
| --- | --- |
| Engine | **Unity 6 — 6000.6.7f1** |
| Language | **C#** |
| Dependency injection | **Zenject** (included in `Assets/Plugins`) |
| Animation | **DOTween** (included in `Assets/Plugins`) |
| Navigation | Unity AI Navigation / NavMesh |
| UI | Unity UI, TextMesh Pro |
| Data | ScriptableObjects, JSON persistence |

## Run the project locally

1. Install **Unity 6000.6.7f1** through Unity Hub (the version recorded in [`ProjectVersion.txt`](ProjectSettings/ProjectVersion.txt)). Include **Android Build Support**, **Android SDK & NDK Tools**, and **OpenJDK** for APK builds.
2. Clone the repository:
   ```bash
   git clone https://github.com/AntoneskuOleksandr/BulletConveyor.git
   ```
3. In Unity Hub, choose **Add project from disk**, select the cloned repository, and open it with the matching editor version.
4. Allow Unity to import assets and resolve packages. Zenject and DOTween source files are already included in the project.
5. Open **`Assets/Project/Scenes/MainMenu.unity`** and press **Play**. Use the menu/tutorial to enter gameplay.
6. For a build, the checked-in [Build Settings](ProjectSettings/EditorBuildSettings.asset) list **`MainMenu.unity`** followed by **`GameScene.unity`**.

To create an installable Android APK on Windows, run `Build-Android.ps1` from a closed checkout. See [Android build instructions](docs/ANDROID_BUILD.md) for the editor path argument, configuration and verification.

## Repository layout

```text
Assets/
  Project/
    Scenes/             Main menu and gameplay scenes
    Scripts/
      BlockRelated/     Production blocks and placement
      Bullets/          Projectile state and allocation
      Debuffs/          Status effects and their rules
      Enemies/          Enemies and wave spawning
      Installers/       Zenject composition
      Managers/         Game, board, wave, and input systems
      Services/         Economy, weapon and progression services
      Weapons/          Configurable weapons and strategies
    ScriptableObjects/  Weapon and level configurations
    Prefabs/            Gameplay objects
  Plugins/              Bundled third-party dependencies
Packages/               Unity package manifest
ProjectSettings/        Unity project configuration
```

## Project status and attribution

**Status:** Playable personal portfolio prototype; Android snapshot 0.1.1. The repository is shared to demonstrate implemented systems and engineering decisions, not presented as a shipped or production-hardened product.

**Developer:** [Oleksandr Antonesku](https://github.com/AntoneskuOleksandr)

The project includes third-party plugins and art assets, which remain subject to their respective licenses. No general open-source license is specified for the repository.
