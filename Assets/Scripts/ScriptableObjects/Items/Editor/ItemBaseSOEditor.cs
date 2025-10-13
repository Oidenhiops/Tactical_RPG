using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(ItemBaseSO), true)]
[CanEditMultipleObjects]
public class ItemBaseSOEditor : Editor
{
    private float cellSize = 30f;

    private SerializedProperty typeObjectProp;
    private SerializedProperty typeWeaponProp;
    private SerializedProperty positionsToAttackProp;
    private SerializedProperty gridSizeProp;
    private void OnEnable()
    {
        typeObjectProp = serializedObject.FindProperty("typeObject");
        typeWeaponProp = serializedObject.FindProperty("typeWeapon");
        positionsToAttackProp = serializedObject.FindProperty("positionsToAttack");
        gridSizeProp = serializedObject.FindProperty("gridSize");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(typeObjectProp);
        ItemBaseSO.TypeObject selectedType = (ItemBaseSO.TypeObject)typeObjectProp.enumValueIndex;

        if (selectedType == ItemBaseSO.TypeObject.Weapon || selectedType == ItemBaseSO.TypeObject.Consumable)
        {
            EditorGUILayout.PropertyField(typeWeaponProp);
        }
        else
        {
            typeWeaponProp.enumValueIndex = 0;

            foreach (var t in targets)
            {
                var so = (ItemBaseSO)t;
                if (so.positionsToAttack != null && so.positionsToAttack.Length > 0)
                {
                    so.positionsToAttack = new Vector3Int[0];
                    EditorUtility.SetDirty(so);
                }
            }
        }

        DrawPropertiesExcluding(serializedObject, "m_Script", "typeObject", "typeWeapon", "positionsToAttack", "gridSize");

        if (selectedType == ItemBaseSO.TypeObject.Weapon || selectedType == ItemBaseSO.TypeObject.Consumable)
        {
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(positionsToAttackProp, true);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Positions To Attack Grid", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(gridSizeProp);
            int gridSize = gridSizeProp.intValue;

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();

            ItemBaseSO referenceItem = (ItemBaseSO)target;

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
                    else GUI.backgroundColor = Color.gray;

                    if (GUILayout.Button("", GUILayout.Width(cellSize), GUILayout.Height(cellSize)))
                    {
                        List<Vector3Int> newPositions = new List<Vector3Int>(referenceItem.positionsToAttack);

                        if (newPositions.Contains(pos)) newPositions.Remove(pos);
                        else newPositions.Add(pos);

                        foreach (var t in targets)
                        {
                            var so = (ItemBaseSO)t;
                            so.positionsToAttack = newPositions.ToArray();
                            EditorUtility.SetDirty(so);
                        }
                    }

                    GUI.backgroundColor = original;
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        serializedObject.ApplyModifiedProperties();
    }
}