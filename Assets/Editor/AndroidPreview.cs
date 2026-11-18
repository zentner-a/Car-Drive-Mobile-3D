using System;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class AndroidPreview
{
	private const string PreviousQualityKey = "CarDrive.AndroidPreview.PreviousQuality";

	static AndroidPreview()
	{
		EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
	}

	private static void OnPlayModeStateChanged(PlayModeStateChange state)
	{
		if (state == PlayModeStateChange.ExitingEditMode)
		{
			SessionState.SetInt(PreviousQualityKey, QualitySettings.GetQualityLevel());
		}
		else if (state == PlayModeStateChange.EnteredPlayMode)
		{
			int mobile = Array.IndexOf(QualitySettings.names, "Mobile");
			if (mobile < 0)
				Debug.LogWarning("Android preview requires the Mobile quality level (not found, skipping).");
			else
				QualitySettings.SetQualityLevel(mobile, true);
		}
		else if (state == PlayModeStateChange.EnteredEditMode)
		{
			int previous = SessionState.GetInt(PreviousQualityKey, -1);
			if (previous >= 0 && previous < QualitySettings.names.Length)
				QualitySettings.SetQualityLevel(previous, true);
			SessionState.EraseInt(PreviousQualityKey);
		}
	}
}
