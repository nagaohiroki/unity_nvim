using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Unity.CodeEditor;
[InitializeOnLoad]
public class NeovimExternalCodeEditor : IExternalCodeEditor
{
	const string nvimName = "nvim-qt";
	const string keyNvimCmd = "nvim_cmd";
	const string keyNvimArgs = "nvim_args";
	const string keyNvimVS = "nvim_vs";
	const string keyNvimExt = "nvim_ext";
	const string defaultExt = ".cs,.shader,.json,.xml,.txt,.yml,.yaml,.md";
	string[] Extensions => GetNvimExt().Split(',');
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
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Arguments");
		EditorPrefs.SetString(keyNvimArgs, EditorGUILayout.TextField(EditorPrefs.GetString(keyNvimArgs)));
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Extensions");
		EditorPrefs.SetString(keyNvimExt, EditorGUILayout.TextField(GetNvimExt()));
		EditorGUILayout.EndHorizontal();
		var paths = CodeEditor.Editor.GetFoundScriptEditorPaths();
		var vs = EditorPrefs.GetString(keyNvimVS);
		var vsList = new List<string>();
		var vsPathList = new List<string>();
		int index = 0;
		foreach(var path in paths)
		{
			if(path.Value.Contains("Visual Studio"))
			{
				if(path.Key == vs)
				{
					index = vsList.Count;
				}
				vsPathList.Add(path.Key);
				vsList.Add(path.Value);
			}
		}
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Visual Studio");
		index = EditorGUILayout.Popup(index, vsList.ToArray());
		EditorPrefs.SetString(keyNvimVS, vsPathList[index]);
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
		if(GUILayout.Button("Regenerate project files"))
		{
			Sync();
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndVertical();
	}
	public bool OpenProject(string filePath, int line, int column)
	{
		if(!IsCodeAsset(filePath, Extensions))
		{
			return false;
		}
		var args = EditorPrefs.GetString(keyNvimArgs).
			Replace("$(File)", filePath).
			Replace("$(Line)", Mathf.Max(0, line).ToString()).
			Replace("$(Column)", Mathf.Max(0, column).ToString());
		var info = new System.Diagnostics.ProcessStartInfo();
		info.FileName = GetNvimExe();
		info.CreateNoWindow = false;
		info.UseShellExecute = false;
		info.Arguments = args;
		System.Diagnostics.Process.Start(info);
		return true;
	}
	string GetNvimExe()
	{
		var exe = EditorPrefs.GetString(keyNvimCmd);
		if(exe.EndsWith(".app"))
		{
			return Path.Combine(exe, "Contents", "MacOS", Path.GetFileNameWithoutExtension(exe));
		}
		return exe;
	}
	public void SyncIfNeeded(string[] addedFiles, string[] deletedFiles, string[] movedFiles, string[] movedFromFiles, string[] importedFiles)
	{
		var ext = Extensions;
		if(IsCodeAssets(addedFiles, ext) ||
		   IsCodeAssets(deletedFiles, ext) ||
		   IsCodeAssets(movedFiles, ext) ||
		   IsCodeAssets(movedFromFiles, ext) ||
		   IsCodeAssets(importedFiles, ext))
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
	void Sync()
	{
		var vs = EditorPrefs.GetString(keyNvimVS);
		if(string.IsNullOrEmpty(vs))
		{
			Debug.Log("No Visual Studio found.");
			return;
		}
		CodeEditor.SetExternalScriptEditor(vs);
		CodeEditor.Editor.CurrentCodeEditor.SyncAll();
		CodeEditor.SetExternalScriptEditor(EditorPrefs.GetString(keyNvimCmd));
	}
	bool IsCodeAssets(string[] files, string[] ext)
	{
		foreach(var file in files)
		{
			if(IsCodeAsset(file, ext))
			{
				return true;
			}
		}
		return false;
	}
	bool IsCodeAsset(string filePath, string[] extList)
	{
		var ext = Path.GetExtension(filePath);
		foreach(var targetExt in extList)
		{
			if(ext == targetExt)
			{
				return true;
			}
		}
		return false;
	}
	string GetNvimExt()
	{
		var ext = EditorPrefs.GetString(keyNvimExt);
		if(!string.IsNullOrEmpty(ext))
		{
			return ext;
		}
		EditorPrefs.SetString(keyNvimExt, defaultExt);
		return defaultExt;
	}
}
