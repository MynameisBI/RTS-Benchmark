using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    string[] mapFiles;
    int selectedMapIndex = 0;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GameManager gameManager = (GameManager)target;

        string folderPath = Path.Combine(Application.dataPath, "Maps");
        if (!Directory.Exists(folderPath))
        {
            EditorGUILayout.HelpBox("Folder 'Assets/Maps' not found.", MessageType.Warning);
            return;
        }

        mapFiles = Directory.GetFiles(folderPath, "*.json");
        List<string> mapNames = new List<string>();
        foreach (var path in mapFiles)
        {
            mapNames.Add(Path.GetFileNameWithoutExtension(path));
        }

        selectedMapIndex = Mathf.Max(0, mapNames.IndexOf(gameManager.selectedMapName));

        selectedMapIndex = EditorGUILayout.Popup("Map Select", selectedMapIndex, mapNames.ToArray());

        string selectedMapFile = mapNames.Count > 0 ? mapNames[selectedMapIndex] : null;
        if (selectedMapFile != gameManager.selectedMapName && selectedMapFile != null)
        {
            gameManager.selectedMapName = selectedMapFile;
            gameManager.selectedMapFile = mapFiles[selectedMapIndex];
            Debug.Log($"Loaded map: {gameManager.selectedMapName}");
        }

        EditorUtility.SetDirty(gameManager);
    }
}
