using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

[InitializeOnLoad]
public static class MutinyBuild
{
    private const string MenuRoot = "Mutiny X/Build/";
    private const string QueueKey = "MutinyX.Build.Queue";
    private const string QueueActiveKey = "MutinyX.Build.QueueActive";

    [Serializable]
    private class BuildQueueState
    {
        public int[] targets;
        public int index;
        public int buildNumber;
        public string version;
    }

    static MutinyBuild()
    {
        if (SessionState.GetBool(QueueActiveKey, false))
        {
            EditorApplication.delayCall += ContinueQueue;
        }
    }

    [MenuItem(MenuRoot + "Build Windows", priority = 10)]
    public static void BuildWindows()
    {
        StartQueue(BuildTarget.StandaloneWindows64);
    }

    [MenuItem(MenuRoot + "Build Android", priority = 11)]
    public static void BuildAndroid()
    {
        StartQueue(BuildTarget.Android);
    }

    [MenuItem(MenuRoot + "Build iOS", priority = 12)]
    public static void BuildIOS()
    {
        StartQueue(BuildTarget.iOS);
    }

    [MenuItem(MenuRoot + "Build All Platforms", priority = 20)]
    public static void BuildAll()
    {
        StartQueue(
            BuildTarget.StandaloneWindows64,
            BuildTarget.Android,
            BuildTarget.iOS
        );
    }

    [MenuItem(MenuRoot + "Open Build Folder", priority = 40)]
    public static void OpenBuildFolder()
    {
        string path = Path.GetFullPath("Builds");
        Directory.CreateDirectory(path);
        EditorUtility.RevealInFinder(path);
    }

    private static void StartQueue(params BuildTarget[] targets)
    {
        if (BuildPipeline.isBuildingPlayer || EditorApplication.isCompiling)
        {
            Debug.LogWarning("[Mutiny Build] Unity is currently compiling or building. Try again when it finishes.");
            return;
        }

        if (SessionState.GetBool(QueueActiveKey, false))
        {
            Debug.LogWarning("[Mutiny Build] A multi-platform build is already running.");
            return;
        }

        int buildNumber = GetNextBuildNumber();
        string version = string.IsNullOrWhiteSpace(PlayerSettings.bundleVersion)
            ? "1.0.0"
            : PlayerSettings.bundleVersion.Trim();

        ApplySharedVersion(version, buildNumber);
        AssetDatabase.SaveAssets();

        var state = new BuildQueueState
        {
            targets = targets.Select(t => (int)t).ToArray(),
            index = 0,
            buildNumber = buildNumber,
            version = version
        };

        SessionState.SetString(QueueKey, JsonUtility.ToJson(state));
        SessionState.SetBool(QueueActiveKey, true);

        Debug.Log($"[Mutiny Build] Starting build {version}+{buildNumber}: {string.Join(", ", targets.Select(t => t.ToString()))}");
        ContinueQueue();
    }

    private static int GetNextBuildNumber()
    {
        int android = Mathf.Max(0, PlayerSettings.Android.bundleVersionCode);
        int ios = 0;
        int.TryParse(PlayerSettings.iOS.buildNumber, out ios);
        return Mathf.Max(android, ios) + 1;
    }

    private static void ApplySharedVersion(string version, int buildNumber)
    {
        PlayerSettings.bundleVersion = version;
        PlayerSettings.Android.bundleVersionCode = buildNumber;
        PlayerSettings.iOS.buildNumber = buildNumber.ToString();

        Debug.Log($"[Mutiny Build] Version synchronized: version={version}, build={buildNumber}");
    }

    private static void ContinueQueue()
    {
        if (!SessionState.GetBool(QueueActiveKey, false))
            return;

        if (EditorApplication.isCompiling || EditorApplication.isUpdating || BuildPipeline.isBuildingPlayer)
        {
            EditorApplication.delayCall += ContinueQueue;
            return;
        }

        string json = SessionState.GetString(QueueKey, string.Empty);
        if (string.IsNullOrEmpty(json))
        {
            FinishQueue();
            return;
        }

        var state = JsonUtility.FromJson<BuildQueueState>(json);
        if (state == null || state.targets == null || state.index >= state.targets.Length)
        {
            FinishQueue();
            return;
        }

        BuildTarget target = (BuildTarget)state.targets[state.index];
        BuildTargetGroup group = BuildPipeline.GetBuildTargetGroup(target);

        if (!BuildPipeline.IsBuildTargetSupported(group, target))
        {
            Debug.LogError($"[Mutiny Build] {target} build support is not installed. Install the platform module in Unity Hub.");
            AdvanceQueue(state);
            return;
        }

        if (EditorUserBuildSettings.activeBuildTarget != target)
        {
            Debug.Log($"[Mutiny Build] Switching active build target to {target}...");
            SessionState.SetString(QueueKey, JsonUtility.ToJson(state));

            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(group, target))
            {
                Debug.LogError($"[Mutiny Build] Failed to switch active build target to {target}.");
                AdvanceQueue(state);
                return;
            }

            // Switching platforms can trigger compilation/domain reload.
            // Queue state is kept in SessionState and resumes automatically.
            EditorApplication.delayCall += ContinueQueue;
            return;
        }

        bool success = BuildTargetNow(target, state.version, state.buildNumber);
        if (!success)
            Debug.LogError($"[Mutiny Build] {target} build failed. Continuing with the remaining queued platforms.");

        AdvanceQueue(state);
    }

    private static void AdvanceQueue(BuildQueueState state)
    {
        state.index++;
        SessionState.SetString(QueueKey, JsonUtility.ToJson(state));

        if (state.index >= state.targets.Length)
        {
            FinishQueue();
        }
        else
        {
            EditorApplication.delayCall += ContinueQueue;
        }
    }

    private static void FinishQueue()
    {
        string json = SessionState.GetString(QueueKey, string.Empty);
        var state = string.IsNullOrEmpty(json) ? null : JsonUtility.FromJson<BuildQueueState>(json);

        SessionState.EraseString(QueueKey);
        SessionState.SetBool(QueueActiveKey, false);

        if (state != null)
            Debug.Log($"[Mutiny Build] Build queue finished: {state.version}+{state.buildNumber}. Output: {Path.GetFullPath("Builds")}");
        else
            Debug.Log("[Mutiny Build] Build queue finished.");
    }

    private static bool BuildTargetNow(BuildTarget target, string version, int buildNumber)
    {
        string[] scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        if (scenes.Length == 0)
        {
            Debug.LogError("[Mutiny Build] No enabled scenes found in Build Settings.");
            return false;
        }

        string location = GetOutputPath(target, version, buildNumber);
        string directory = target == BuildTarget.iOS ? location : Path.GetDirectoryName(location);

        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = location,
            target = target,
            options = BuildOptions.None
        };

        Debug.Log($"[Mutiny Build] Building {target} -> {location}");
        BuildReport report = BuildPipeline.BuildPlayer(options);

        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[Mutiny Build] {target} succeeded. Size: {report.summary.totalSize} bytes, Time: {report.summary.totalTime}");
            return true;
        }

        Debug.LogError($"[Mutiny Build] {target} result: {report.summary.result}");
        return false;
    }

    private static string GetOutputPath(BuildTarget target, string version, int buildNumber)
    {
        string safeVersion = version.Replace('/', '-').Replace('\\', '-');
        string label = $"MutinyX-{safeVersion}+{buildNumber}";

        switch (target)
        {
            case BuildTarget.StandaloneWindows64:
                return Path.Combine("Builds", "Windows", label, "Mutiny X.exe");

            case BuildTarget.Android:
                string extension = EditorUserBuildSettings.buildAppBundle ? ".aab" : ".apk";
                return Path.Combine("Builds", "Android", label + extension);

            case BuildTarget.iOS:
                return Path.Combine("Builds", "iOS", label);

            default:
                throw new ArgumentOutOfRangeException(nameof(target), target, null);
        }
    }
}
