using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Xml.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class AndroidBuild
{
    public static readonly string[] Scenes =
    {
        "Assets/Project/Scenes/MainMenu.unity",
        "Assets/Project/Scenes/GameScene.unity"
    };

    [MenuItem("Build/Bullet Conveyor/Android APK")]
    public static void Apk()
    {
        if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android))
            throw new BuildFailedException("Install Android Build Support, SDK, NDK and OpenJDK for this Unity version.");

        BuildValidation.RunAll();
        PlayerSettings.productName = "Bullet Conveyor";
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.AOGames.BulletConveyor");
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Android, ManagedStrippingLevel.Minimal);
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
        PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)36;

        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 |
            AndroidArchitecture.ARM64;
        PlayerSettings.Android.useCustomKeystore = false;
        EditorUserBuildSettings.buildAppBundle = false;
        EditorUserBuildSettings.exportAsGoogleAndroidProject = true;
        EditorUserBuildSettings.development = false;
        EditorUserBuildSettings.allowDebugging = false;
        EditorUserBuildSettings.connectProfiler = false;
        EditorBuildSettings.scenes = Array.ConvertAll(Scenes, scene => new EditorBuildSettingsScene(scene, true));
        AssetDatabase.SaveAssets();

        string output = Environment.GetEnvironmentVariable("BULLET_APK_PATH");
        if (string.IsNullOrEmpty(output)) output = "Builds/Android/BulletConveyor.apk";
        output = Path.GetFullPath(output);
        Directory.CreateDirectory(Path.GetDirectoryName(output));

        string gradleProject = Path.GetFullPath("Builds/g");
        BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = Scenes,
            target = BuildTarget.Android,
            locationPathName = gradleProject,
            options = BuildOptions.CompressWithLz4HC
        });
        var summary = report.summary;
        if (summary.result != BuildResult.Succeeded)
            throw new BuildFailedException($"Android export failed with {summary.totalErrors} errors. See the Editor log.");

        ConfigureBackup(gradleProject);
        PackageApk(gradleProject);
        File.Copy(Path.Combine(gradleProject, "launcher/build/outputs/apk/release/launcher-release.apk"), output, true);
        long size = new FileInfo(output).Length;
        File.WriteAllText(output + ".build.txt",
            $"Result: Succeeded\nUnity export: {summary.result}\nGradle packaging: Succeeded\nUnity: {Application.unityVersion}\n" +
            $"Errors: {summary.totalErrors}\nWarnings: {summary.totalWarnings}\n" +
            $"Bytes: {size}\nUnity export duration: {summary.totalTime}\n" +
            "Package: com.AOGames.BulletConveyor\nMin SDK: 26\nTarget SDK: 36\n" +
            "Architectures: ARMv7, ARM64\nBackend: IL2CPP\nDevelopment: false\n");
        Debug.Log($"ANDROID_BUILD_SUCCEEDED: {output} ({size} bytes)");
    }

    private static void PackageApk(string project)
    {
        string java = Path.Combine(UnityEditor.Android.AndroidExternalToolsSettings.jdkRootPath, "bin/java.exe");
        string gradle = Directory.GetFiles(Path.Combine(UnityEditor.Android.AndroidExternalToolsSettings.gradlePath,
            "lib"), "gradle-launcher-*.jar").Single();
        var start = new ProcessStartInfo
        {
            FileName = java,
            Arguments = $"-classpath \"{gradle}\" org.gradle.launcher.GradleMain --no-daemon --max-workers=2 " +
                "\"-Dorg.gradle.jvmargs=-Xmx2048m\" assembleRelease",
            WorkingDirectory = project,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {

            string socketDirectory = Path.Combine(Path.GetTempPath(), "BulletConveyorSockets-" + Guid.NewGuid().ToString("N"));
            start.EnvironmentVariables["JAVA_TOOL_OPTIONS"] =
                (Environment.GetEnvironmentVariable("JAVA_TOOL_OPTIONS") +
                " -Djdk.net.unixdomain.tmpdir=\"" + socketDirectory + "\"").Trim();
        }
        var log = new StringBuilder();
        using (var process = new Process { StartInfo = start })
        {
            DataReceivedEventHandler receive = (sender, args) =>
            {
                if (args.Data == null) return;
                lock (log) log.AppendLine(args.Data);
            };
            process.OutputDataReceived += receive;
            process.ErrorDataReceived += receive;
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            Directory.CreateDirectory("Logs");
            File.WriteAllText("Logs/android-gradle.log", log.ToString());
            if (process.ExitCode != 0)
                throw new BuildFailedException("APK packaging failed. See Logs/android-gradle.log.\n" + log);
        }
    }

    private static void ConfigureBackup(string project)
    {
        XNamespace android = "http://schemas.android.com/apk/res/android";
        XNamespace tool = "http://schemas.android.com/tools";
        foreach (string module in new[] { "launcher", "unityLibrary" })
        {
            string manifest = Path.Combine(project, module, "src/main/AndroidManifest.xml");
            var document = XDocument.Load(manifest);
            var application = document.Root.Element("application");
            application.SetAttributeValue(android + "allowBackup", "false");
            application.SetAttributeValue(android + "fullBackupContent", "false");
            application.SetAttributeValue(android + "dataExtractionRules", "@xml/bullet_data_rules");
            application.SetAttributeValue(tool + "replace", "android:allowBackup,android:fullBackupContent,android:dataExtractionRules");
            document.Save(manifest);
        }
        var rules = new XElement("data-extraction-rules");
        foreach (string mode in new[] { "cloud-backup", "device-transfer" })
        {
            var group = new XElement(mode);
            foreach (string domain in new[] { "root", "file", "database", "sharedpref", "external" })
                group.Add(new XElement("exclude", new XAttribute("domain", domain), new XAttribute("path", ".")));
            rules.Add(group);
        }
        string resource = Path.Combine(project, "unityLibrary/src/main/res/xml/bullet_data_rules.xml");
        Directory.CreateDirectory(Path.GetDirectoryName(resource));
        new XDocument(rules).Save(resource);
    }
}
