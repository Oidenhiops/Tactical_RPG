using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(ItemBaseSO), true)]
[CanEditMultipleObjects]
public class ItemBaseSOEditor : Editor
{
    private float cellSize = 30f;
    private SerializedProperty positionsToAttackProp;

    private void OnEnable()
    {
        positionsToAttackProp = serializedObject.FindProperty("positionsToAttack");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawPropertiesExcluding(serializedObject, "m_Script", "positionsToAttack");

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(positionsToAttackProp, true);

        ItemBaseSO referenceItem = (ItemBaseSO)target;
        int gridSize = referenceItem.gridSize;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"Positions To Attack Grid (Grid Size: {gridSize})", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical();

        for (int y = gridSize; y >= -gridSize; y--)
        {
            GUILayout.BeginHorizontal();
            for (int x = -gridSize; x <= gridSize; x++)
            {
                Vector3Int pos = new Vector3Int(x, 0, y);
                bool isOrigin = pos == Vector3Int.zero;
                bool isActive = System.Array.Exists(referenceItem.positionsToAttack, p => p == pos);

                Color original = GUI.backgroundColor;
                if (isOrigin) GUI.backgroundColor = isActive ? Color.green : new Color(1f, 0.6f, 0f);
                else if (isActive) GUI.backgroundColor = Color.red;
                else GUI.backgroundColor = Color.white;

                if (GUILayout.Button("", GUILayout.Width(cellSize), GUILayout.Height(cellSize)))
                {
                    Undo.RecordObject(referenceItem, "Edit Grid Positions");

                    List<Vector3Int> positions = new List<Vector3Int>(referenceItem.positionsToAttack);

                    if (isActive) positions.Remove(pos);
                    else positions.Add(pos);

                    referenceItem.positionsToAttack = positions.ToArray();

                    EditorUtility.SetDirty(referenceItem);
                }

                GUI.backgroundColor = original;
            }
            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();
    }
}