# Android build

Use Unity **6000.6.7f1** with **Android Build Support**, **Android SDK & NDK Tools**, and **OpenJDK** installed through Unity Hub.

1. Open the project in Unity and let it finish importing.
2. Choose **Build > Bullet Conveyor > Android APK**.
3. Find the APK at `Builds/Android/BulletConveyor.apk`.

The build checks the project, exports a Gradle project, and packages the APK. Build output is ignored by Git. On Windows, keep the project path reasonably short to avoid CMake path limits.

## Configuration

- Android 8.0 or newer; target API 36.
- IL2CPP with ARMv7 and ARM64 support.
- Portrait orientation; MainMenu followed by GameScene.
- Development mode and script debugging disabled.
- Package name: `com.AOGames.BulletConveyor`.

The APK uses local debug signing for direct installation. Publishing to a store requires a separate release keystore. Application updates preserve local progress; Android backup and device transfer are disabled.

For build failures, check the Unity Console and `Logs/android-gradle.log`.
