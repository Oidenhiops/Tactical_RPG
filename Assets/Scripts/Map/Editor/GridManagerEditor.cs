using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GenerateMap))]
public class GridManagerEditor : Editor
{
    private Vector2 scrollPos;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GenerateMap manager = (GenerateMap)target;

        if (manager.grid != null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Grid Info", EditorStyles.boldLabel);

            int width = manager.grid.GetLength(0);
            int height = manager.grid.GetLength(1);

            float gridWidth = width * 45f;

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(300));

            EditorGUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayout.Width(gridWidth));

            for (int y = height - 1; y >= 0; y--)
            {
                EditorGUILayout.BeginHorizontal();

                for (int x = 0; x < width; x++)
                {
                    var cell = manager.grid[x, y];
                    if (cell != null)
                    {
                        GUI.backgroundColor = cell.isWalkable ? Color.green : Color.red;

                        if (GUILayout.Button($"{cell.pos.x},{cell.pos.y},{cell.pos.z}", GUILayout.Width(50), GUILayout.Height(50)))
                        {
                            cell.isWalkable = !cell.isWalkable;
                            EditorUtility.SetDirty(manager);
                        }

                        GUI.backgroundColor = Color.white;
                    }
                    else
                    {
                        GUI.backgroundColor = Color.gray;
                        GUILayout.Button("N/A", GUILayout.Width(50), GUILayout.Height(50));
                        GUI.backgroundColor = Color.white;
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();
        }
        else
        {
            EditorGUILayout.HelpBox("Grid no generado todavía.", MessageType.Info);
        }
    }
}