using System.IO;
using ReactUnity.UGUI;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class ReactHotReloader
{
    static FileSystemWatcher watcher;
    static volatile bool changeDetected;
    static double reloadAt = -1;
    const double DebounceSeconds = 2.0;

    static ReactHotReloader()
    {
        var srcPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../react/src"));
        if (!Directory.Exists(srcPath)) return;

        watcher = new FileSystemWatcher(srcPath)
        {
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
            Filter = "*.*",
            EnableRaisingEvents = true,
        };

        watcher.Changed += OnSourceChanged;
        watcher.Created += OnSourceChanged;
        watcher.Renamed += OnSourceChanged;

        EditorApplication.update += CheckReload;
        AssemblyReloadEvents.beforeAssemblyReload += Cleanup;
    }

    static void OnSourceChanged(object sender, FileSystemEventArgs e)
    {
        var ext = Path.GetExtension(e.FullPath);
        if (ext != ".ts" && ext != ".tsx" && ext != ".json") return;
        changeDetected = true;
    }

    static void CheckReload()
    {
        if (changeDetected)
        {
            changeDetected = false;
            reloadAt = EditorApplication.timeSinceStartup + DebounceSeconds;
        }

        if (reloadAt < 0 || EditorApplication.timeSinceStartup < reloadAt) return;
        reloadAt = -1;

        if (!Application.isPlaying) return;

        foreach (var renderer in Object.FindObjectsByType<ReactRendererUGUI>(FindObjectsSortMode.None))
            renderer.Render();
    }

    static void Cleanup()
    {
        watcher?.Dispose();
        EditorApplication.update -= CheckReload;
        AssemblyReloadEvents.beforeAssemblyReload -= Cleanup;
    }
}
