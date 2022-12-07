using Unity.CodeEditor;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Diagnostics;
[InitializeOnLoad]
public class NeovimExternalCodeEditor : IExternalCodeEditor
{
	static NeovimExternalCodeEditor()
	{
		CodeEditor.Register(new NeovimExternalCodeEditor());
	}
	public CodeEditor.Installation[] Installations => new[]
	{
		new CodeEditor.Installation
		{
			Name ="nvim-qt",
			Path = EditorPrefs.GetString("nvim_cmd")
		}
	};
	public void Initialize(string editorInstallationPath)
	{
	}
	public void OnGUI()
	{
		EditorGUILayout.BeginVertical();

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("execute");
		EditorPrefs.SetString("nvim_cmd", EditorGUILayout.TextField(EditorPrefs.GetString("nvim_cmd")));
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("arguments");
		EditorPrefs.SetString("nvim_args", EditorGUILayout.TextField(EditorPrefs.GetString("nvim_args")));
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("visual studio");
		EditorPrefs.SetString("nvim_vs_path", EditorGUILayout.TextField(EditorPrefs.GetString("nvim_vs_path")));
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.EndVertical();
	}
	bool IsCodeAsset(string filePath)
	{
		return Path.GetExtension(filePath) == ".cs";
	}
	public bool OpenProject(string filePath, int line, int column)
	{
		var args = EditorPrefs.GetString("nvim_args").
			Replace("$(File)", filePath).
			Replace("$(Line)", Mathf.Max(0, line).ToString()).
			Replace("$(Column)", Mathf.Max(0, column).ToString());
		var info = new ProcessStartInfo();
		info.FileName = EditorPrefs.GetString("nvim_cmd");
		info.CreateNoWindow = false;
		info.UseShellExecute = false;
		info.Arguments = args;
		Process.Start(info);
		return true;
	}
	public void SyncAll() { }
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
	private void Sync()
	{
		CodeEditor.SetExternalScriptEditor(EditorPrefs.GetString("nvim_vs_path"));
		CodeEditor.Editor.CurrentCodeEditor.SyncAll();
		CodeEditor.SetExternalScriptEditor(EditorPrefs.GetString("nvim_cmd"));
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
}
