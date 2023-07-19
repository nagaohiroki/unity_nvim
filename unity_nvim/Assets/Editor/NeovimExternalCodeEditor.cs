using System.IO;
using UnityEditor;
using UnityEngine;
using Unity.CodeEditor;
using Microsoft.Unity.VisualStudio.Editor;
[InitializeOnLoad]
public class NeovimExternalCodeEditor : IExternalCodeEditor
{
	const string keyNvimCmd = "nvim_cmd";
	const string keyNvimArgs = "nvim_args";
	IGenerator _generator = new ProjectGeneration();
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
		ItemGUI("Execute", keyNvimCmd);
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
		Sync();
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
		_generator.Sync();
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
