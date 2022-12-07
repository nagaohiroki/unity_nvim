using System.IO;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using Unity.CodeEditor;
[InitializeOnLoad]
public class NeovimExternalCodeEditor : IExternalCodeEditor
{
	const string keyNvimCmd = "nvim_cmd";
	const string keyNvimArgs = "nvim_args";
	const string keyNvimVisualStudioPath = "nvim_vs_path";
	public CodeEditor.Installation[] Installations => new[]
	{
		new CodeEditor.Installation
		{
			Name = "nvim-qt",
			Path = EditorPrefs.GetString(keyNvimCmd)
		}
	};
	static NeovimExternalCodeEditor()
	{
		CodeEditor.Register(new NeovimExternalCodeEditor());
	}
	public void Initialize(string editorInstallationPath) { }
	public void SyncAll() { }
	public void OnGUI()
	{
		EditorGUILayout.BeginVertical();
		ItemGUI("execute", keyNvimCmd);
		ItemGUI("arguments", keyNvimArgs);
		ItemGUI("visual studio", keyNvimVisualStudioPath);
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
		var info = new ProcessStartInfo();
		info.FileName = EditorPrefs.GetString(keyNvimCmd);
		info.CreateNoWindow = false;
		info.UseShellExecute = false;
		info.Arguments = args;
		Process.Start(info);
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
		installation = Installations[0];
		return true;
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
		CodeEditor.SetExternalScriptEditor(EditorPrefs.GetString(keyNvimVisualStudioPath));
		CodeEditor.Editor.CurrentCodeEditor.SyncAll();
		CodeEditor.SetExternalScriptEditor(EditorPrefs.GetString(keyNvimCmd));
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
	bool IsCodeAsset(string filePath) => Path.GetExtension(filePath) == ".cs";
}
