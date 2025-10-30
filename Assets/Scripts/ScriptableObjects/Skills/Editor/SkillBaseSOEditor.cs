using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(SkillsBaseSO), true)]
[CanEditMultipleObjects]
public class SkillBaseSOEditor : Editor
{
    private float cellSize = 30f;
    private SerializedProperty positionsToMakeSkillProp;
    private SerializedProperty skillRadiusProp;
    private SerializedProperty positionsSkillFormProp;
    private SerializedProperty skillInnerRadiusProp;

    private void OnEnable()
    {
        positionsToMakeSkillProp = serializedObject.FindProperty("positionsToMakeSkill");
        skillRadiusProp = serializedObject.FindProperty("skillRadius");

        positionsSkillFormProp = serializedObject.FindProperty("positionsSkillForm");
        skillInnerRadiusProp = serializedObject.FindProperty("skillInnerRadius");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawPropertiesExcluding(serializedObject, "m_Script",
            "positionsToMakeSkill", "skillRadius",
            "positionsSkillForm", "skillInnerRadius");

        DrawGridSection(
            label: "Positions To Make Skill Grid",
            arrayProp: positionsToMakeSkillProp,
            gridSizeProp: skillRadiusProp,
            getPositions: skill => skill.positionsToMakeSkill,
            setPositions: (skill, arr) => skill.positionsToMakeSkill = arr
        );

        EditorGUILayout.Space(20);

        DrawGridSection(
            label: "Positions Skill Form Grid",
            arrayProp: positionsSkillFormProp,
            gridSizeProp: skillInnerRadiusProp,
            getPositions: skill => skill.positionsSkillForm,
            setPositions: (skill, arr) => skill.positionsSkillForm = arr
        );

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawGridSection(
        string label,
        SerializedProperty arrayProp,
        SerializedProperty gridSizeProp,
        System.Func<SkillsBaseSO, Vector3Int[]> getPositions,
        System.Action<SkillsBaseSO, Vector3Int[]> setPositions
    )
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(gridSizeProp);

        int gridSize = gridSizeProp.intValue;
        SkillsBaseSO referenceSkill = (SkillsBaseSO)target;

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
                bool isActive = System.Array.Exists(getPositions(referenceSkill), p => p == pos);

                Color original = GUI.backgroundColor;
                if (isOrigin) GUI.backgroundColor = isActive ? Color.green : new Color(1f, 0.6f, 0f);
                else if (isActive) GUI.backgroundColor = Color.red;
                else GUI.backgroundColor = Color.gray;

                if (GUILayout.Button("", GUILayout.Width(cellSize), GUILayout.Height(cellSize)))
                {
                    List<Vector3Int> newPositions = new List<Vector3Int>(getPositions(referenceSkill));

                    if (newPositions.Contains(pos)) newPositions.Remove(pos);
                    else newPositions.Add(pos);

                    foreach (var t in targets)
                    {
                        var so = (SkillsBaseSO)t;
                        setPositions(so, newPositions.ToArray());
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

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(arrayProp, true);
    }
}