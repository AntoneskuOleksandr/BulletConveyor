# Bullet Conveyor: portfolio milestones

The user authorized all remaining milestones together on 2026-10-30, including a full tutorial UX rework. Changes are kept on test/android-build-2026-10-29.

## 1. Main menu

Completed. Startup guards, initialized-level readiness, stable preview reuse, level ID lookup, individual event cleanup and dynamic level names. The original 15 lifecycle checks still run separately with domain and scene reload disabled.

## 2. Progress and upgrades

Completed. Every weapon previously had an empty upgrade array, so the UI displayed MAX at level zero. Eight weapons now have five priced tiers. Upgrades and their cost are saved together; legacy out-of-range values are clamped. Save files use temporary writes, replacement and a backup; corrupt originals are preserved before backup recovery. Editor fixtures remain separate from real saves. APK packaging excludes cloud backup and device-transfer data; updating an existing install preserves its local progress.

## 3. Spawning, movement and game flow

Completed. Spawn candidates must be on the correct NavMesh, inside the requested bounds, away from edges, clear of obstacle capsules and living enemies, and connected to a turret. Bounded candidate batches retry over time rather than placing an invalid enemy or dropping a request. NavMeshAgent owns movement; Rigidbody is kinematic and animation root motion is off. Waves finish only after spawning and combat finish. Dead turrets stop updating/firing and queued requests are cancelled safely. Five map previews now have complete game configurations; maps without baked navigation build it once at load.

## 4. Audio

Completed. A persistent service plays existing music, UI clicks and weapon shots plus generated placement, merge, wave and result feedback. It manages the active listener, limits overlapping shot feedback, streams music and preserves the Sound setting between scenes and launches.

## 5. Targeted cleanup

Completed. Removed LINQ enumerators from repeated weapon modifier calculations, cached the grid's tile list, used squared target distances, bounded generated ammunition to 150 objects and gave fired ammunition a lifetime. Fixed stale UI listeners, repeated currency initialization, upgrade-panel reopening and animation cleanup when a scene is unloaded. UI animations run during pause. Removed an ObjectPool wrapper that never received returned objects. Removed comments from all project-owned C# and build code; vendor plugins were left alone. These are local refactors, without changing the strategy/DI architecture. No device FPS improvement is claimed without a physical-phone profile.

## 6. Tutorial UX

Completed. Six action-based steps teach buying, valid placement, buying a match, merging, replacing the stronger block and automatic defense. Waves wait for preparation. The pointer projects actual UI/world targets into a screen-space canvas every frame, demonstrates a source-to-destination gesture and hides during touch/pause. A safe-area instruction card gives short copy and progress. Failed drops do not advance; purchases advance only when a block is created. Skip supplies a valid starting ammo block; Tutorial in the menu replays training without resetting permanent progress or unlocking maps for a replay.

## Verification

- 17 content, menu, persistence and build checks.
- 194 checks in the real scenes in Play Mode: purchases, placement, merge, pause, real turret victory through two training waves, reward persistence, five-map spawning, defeat, menu return, replay and skip.
- 15 dedicated menu lifecycle checks with reload disabled.
- Android APK, installation, interaction, save/reinstall and audio-runtime verification are recorded in the release report.

Phone playtesting and final balance feedback remain with the user. Automated map checks validate navigation/spawning; they do not claim every weapon/ability combination or a complete playthrough of every normal map.


## Follow-up 0.1.1

User feedback: missing enemies after Skip, missing loadout platforms, speed reset after ability selection, unreliable speed-button touches, repeated enemy pairs and dull lighting.

Implemented: combat starts before tutorial teardown; training uses a two-second countdown and visible spawn area. Navigation is rebuilt from current colliders at level load, with deterministic fallback placement and safe recovery before the first enemy. A crowded wave queues enemies without rebuilding the navigation under active actors. All five maps are now present in GameManager's serialized list.

Three menu platforms show and update the selected loadout. A single GameSpeed component owns the selected multiplier and independent pause owners. Booster and pause windows preserve the selected speed. The full speed-button rectangle is in screen space; decorative graphics do not intercept input.

The enemy chain is Cactus -> Goblin -> Small Golem -> Fish -> Big Golem -> Cyclops. Each level uses one adjacent pair. The second type starts after floor(totalWaves / 2) first-type-only waves; its chance grows linearly to 100 percent on the last wave. The four old unused prefabs now use current enemy, health and status components. Both scenes have an explicit real-time sun and brighter three-color ambient lighting.

Verification: 64 build/content checks, 234 real-scene checks including first/last enemy types and health/death on every map, 72 Skip checks across six stages and three loadouts, plus 15 dedicated menu checks. Android interaction and release details are in the 0.1.1 report. The initial Skip failure did not reproduce in the baseline editor probe; actual enemy creation is now awaited and checked, rather than only coroutine startup.
