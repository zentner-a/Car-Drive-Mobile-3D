using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class AndroidBuild
{
	[MenuItem("Tools/Build Android APK")]
	private static void BuildAndroid()
	{
		if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
		{
			Debug.LogError("Stop Play Mode and wait for script compilation before building.");
			return;
		}
		if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android &&
			!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
		{
			Debug.LogError("Could not switch the active platform to Android.");
			return;
		}
		string defines = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Android);
		if (Array.IndexOf(defines.Split(';'), "APP_UI_EDITOR_ONLY") < 0)
		{
			PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Android,
				string.IsNullOrEmpty(defines) ? "APP_UI_EDITOR_ONLY" : defines + ";APP_UI_EDITOR_ONLY");
			Debug.Log("App UI was excluded from Android builds. Wait for recompilation, then run Build Android APK again.");
			return;
		}
		Directory.CreateDirectory("Builds");
		BuildReport report = BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, "Builds/CarDrive-test.apk", BuildTarget.Android, BuildOptions.Development);
		if (report.summary.result != BuildResult.Succeeded)
			Debug.LogError("Android build did not succeed: " + report.summary.result + "; errors: " + report.summary.totalErrors);
		else
			Debug.Log("Android build result: " + report.summary.result + "; errors: " + report.summary.totalErrors);
	}
}
