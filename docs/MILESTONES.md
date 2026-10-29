# Bullet Conveyor: staged fixes

Only one milestone is implemented at a time. The next milestone starts after the user verifies and approves the current one.

## 1. Main menu initialization

Status: implemented; awaiting user verification. Android version: 0.0.2-m1 (version code 2).

Scope: the reported LevelDisplay exception, initialization order, level selection and repeated menu opening.

Changes: Play waits for an initialized level; saved level IDs are resolved by ID; event handlers unsubscribe individually; the same level preview is reused when reopening the menu. Scene transition saves the selected level first.

Verification: run menu-first and display-first initialization in Play mode with domain/scene reload disabled, reopen the menu three times, check preview count and navigation, preserve other button listeners, and block locked levels. Also run the existing build checks and build APK version 0.0.2-m1.

Result: 15 Play-mode lifecycle checks and 13 general build checks passed. The reported exception did not recur in the open development editor or the Android emulator. The emulator also passed level navigation, locked-level blocking and three main/weapons panel cycles. The two menu source fixes were applied to the open development copy for immediate user testing.

User acceptance: open MainMenu, confirm no new LevelDisplay exception, navigate through levels, switch between the main and weapons panels several times, and enter an available level.

## 2. Player progress after reinstall

Inspect debug profile settings, saved weapon upgrades, Android backup/restore and first-install defaults. Preserve existing player data while diagnosing. Verify clean install and reinstall separately.

## 3. Enemy spawning and movement

Check obstacle clearance, NavMesh placement, separation between spawn positions, colliders, rigidbodies and agent movement. Optimize the spawn checks where profiling and measurements justify it.

## 4. Audio

Inspect available clips, listeners, sources, volume settings and playback triggers. Verify menu and combat sounds on Android.

## 5. Focused cleanup and performance

Measure the remaining expensive paths and make small local refactors. Remove comments from project-owned scripts under Assets/Project and build tools. Third-party plugin sources are outside that cleanup scope. Finish with Android verification.

All code added or changed during these milestones must have no comments. Progress, spawning, audio and broader cleanup are outside milestone 1.
