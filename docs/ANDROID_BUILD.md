# Android test build

Use Unity **6000.6.7f1** with Android Build Support, SDK/NDK tools and OpenJDK installed through Unity Hub. This branch upgrades the package lock and imported assets to that editor version.

## Build

On Windows, use the PowerShell wrapper below from a closed checkout. It launches Unity with a bounded worker count. An open development checkout can stay open while a separate checkout builds.

For a separate, closed checkout, run from PowerShell (adjust the editor and project paths):

```powershell
.\Build-Android.ps1 -UnityPath 'E:\Work\6000.6.7f1\Editor\Unity.exe'
```

The output defaults to `Builds/Android/BulletConveyor.apk`; override it with `-OutputPath`. The command first runs `BuildValidation.RunAll` and aborts if any check fails. Unity exports the IL2CPP Gradle project to the short path `Builds/g`, then the build command packages and copies the APK to the requested output. Keep the checkout path short enough for CMake's Windows path limit. `Logs/android-build.log` must contain `BUILD_VALIDATION_SUCCEEDED` and `ANDROID_BUILD_SUCCEEDED`; packaging diagnostics are in `Logs/android-gradle.log`. The adjacent `.build.txt` file records both stages, Unity's error count and the APK size. APKs and generated build caches are ignored by Git. The editor also exposes **Build > Bullet Conveyor > Android APK** using the same pipeline.

## Configuration

- Package: `com.AOGames.BulletConveyor`; portrait orientation.
- Android 8.0/API 26 or newer; target API 36.
- IL2CPP; ARMv7 and ARM64 in one APK. Unity 6000.6 no longer produces x86-64 Android players; an emulator needs ARM translation to run this APK.
- MainMenu first, GameScene second; development build and script debugging disabled.
- Local Android debug signing for installation and testing. Store publishing requires a separately managed release keystore; no private signing keys are committed.
- Editor debug player data is compiled out of the player. Editor fixtures use a separate save file.
- Gradle is limited to two workers and a 2 GB heap. The packaging process receives an OpenJDK socket-directory override so local Java pipes fall back to TCP on Windows. Explicitly packaging the exported project preserves this child environment, which Unity's built-in Gradle launcher clears. No machine-wide settings are changed.

## Checks

`BuildValidation.RunAll` checks fresh player data, currency persistence, first-launch access to the tutorial, normal loadout requirements, starter weapon availability, tutorial completion persistence, isolation of editor fixture data, legacy data normalization, releasing a drag over empty space, missing scripts in both shipped scenes, and tutorial overlays passing taps through. Temporary save files are isolated from real progress.

For an Android smoke test, install the APK, launch on a fresh profile, start the tutorial, buy/place/merge blocks and observe combat. Also check pause/resume, menu return, background/resume and cold relaunch. Collect application logs and check for managed exceptions and native crashes. Emulator results do not establish performance on a physical phone.


## Portfolio validation

Use a separate closed checkout with Unity 6000.6.7f1. Run the editor in batch mode with `-executeMethod PortfolioValidation.Run` and without `-quit`; the runner exits with its result. It uses isolated temporary save data and never resets a real player profile. Look for `PORTFOLIO_CHECKS_SUCCEEDED` and the actual check count. `MenuLifecycleValidation.Run` separately validates menu reload behavior. A normal APK build runs `BuildValidation.RunAll` before export.

The generated application manifest disables backup and uses explicit cloud/device-transfer exclusions. This follows the Android [Auto Backup documentation](https://developer.android.com/identity/data/autobackup), including the Android 12 device-transfer distinction. Local progress is retained when installing an APK update; uninstalling is intended to start a fresh profile.
