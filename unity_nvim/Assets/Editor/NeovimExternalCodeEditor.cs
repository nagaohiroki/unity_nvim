using System.IO;
using UnityEditor;
using UnityEngine;
using Unity.CodeEditor;
[InitializeOnLoad]
public class NeovimExternalCodeEditor : IExternalCodeEditor
{
	const string nvimName = "nvim-qt";
	const string keyNvimCmd = "nvim_cmd";
	const string keyNvimArgs = "nvim_args";
	readonly string[] vsNames = new[]
	{
		"Visual Studio Community 2022 [17.5.3]",
		"Visual Studio Community 2019 [16.11.25]",
		"Visual Studio Community 2017 [15.9.53]",
		"Visual Studio for Mac [8.10.18]"
	};
	public CodeEditor.Installation[] Installations => new[]
	{
		new CodeEditor.Installation
		{
			Name = nvimName,
			Path = EditorPrefs.GetString(keyNvimCmd)
		}
	};
	static NeovimExternalCodeEditor()
	{
		CodeEditor.Register(new NeovimExternalCodeEditor());
	}
	public void Initialize(string editorInstallationPath) { }
	public void SyncAll()
	{
		Sync();
	}
	public void OnGUI()
	{
		EditorGUILayout.BeginVertical();
		ItemGUI("Arguments", keyNvimArgs);
		if(GUILayout.Button("Regenerate project files"))
		{
			Sync();
		}
		EditorGUILayout.EndVertical();
	}
	public bool OpenProject(string filePath, int line, int column)
	{
		if(!IsCodeAsset(filePath))
		{
			return false;
		}
		var args = EditorPrefs.GetString(keyNvimArgs).
			Replace("$(File)", filePath).
			Replace("$(Line)", Mathf.Max(0, line).ToString()).
			Replace("$(Column)", Mathf.Max(0, column).ToString());
		var info = new System.Diagnostics.ProcessStartInfo();
		info.FileName = EditorPrefs.GetString(keyNvimCmd);
		info.CreateNoWindow = false;
		info.UseShellExecute = false;
		info.Arguments = args;
		System.Diagnostics.Process.Start(info);
		return true;
	}
	public void SyncIfNeeded(string[] addedFiles, string[] deletedFiles, string[] movedFiles, string[] movedFromFiles, string[] importedFiles)
	{
		if(IsCodeAssets(addedFiles) || IsCodeAssets(deletedFiles) || IsCodeAssets(movedFiles) || IsCodeAssets(movedFromFiles) || IsCodeAssets(importedFiles))
		{
			Sync();
		}
	}
	public bool TryGetInstallationForPath(string editorPath, out CodeEditor.Installation installation)
	{
		if(editorPath.Contains("nvim"))
		{
			installation = new CodeEditor.Installation
			{
				Name = nvimName,
				Path = editorPath
			};
			EditorPrefs.SetString(keyNvimCmd, editorPath);
			return true;
		}
		installation = default;
		return false;
	}
	void ItemGUI(string label, string key)
	{
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField(label);
		EditorPrefs.SetString(key, EditorGUILayout.TextField(EditorPrefs.GetString(key)));
		EditorGUILayout.EndHorizontal();
	}
	void Sync()
	{
		CodeEditor.SetExternalScriptEditor(GetVSPath());
		CodeEditor.Editor.CurrentCodeEditor.SyncAll();
		CodeEditor.SetExternalScriptEditor(EditorPrefs.GetString(keyNvimCmd));
	}
	string GetVSPath()
	{
		var paths = CodeEditor.Editor.GetFoundScriptEditorPaths();
		foreach(var vs in vsNames)
		{
			foreach(var path in paths)
			{
				if(path.Value == vs)
				{
					return path.Key;
				}
			}
		}
		return null;
	}
	bool IsCodeAssets(string[] files)
	{
		foreach(var file in files)
		{
			if(IsCodeAsset(file))
			{
				return true;
			}
		}
		return false;
	}
	bool IsCodeAsset(string filePath)
	{
		return Path.GetExtension(filePath) == ".cs";
	}
}
