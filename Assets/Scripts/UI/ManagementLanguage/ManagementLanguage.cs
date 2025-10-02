using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ManagementLanguage : MonoBehaviour
{
    public GameData.TypeLOCS typeLOCS;
    public TMP_Text dialogText;
    public int id = 0;
    public bool initByCode;
    [NonSerialized] public string[] dialogIds = { };
    void OnValidate()
    {
        if (dialogText == null) dialogText = GetComponent<TMP_Text>();
    }
    void OnDestroy()
    {
        GameData.Instance.systemDataInfo.configurationsInfo.OnLanguageChange -= RefreshText;
    }
    void Awake()
    {
        GameData.Instance.systemDataInfo.configurationsInfo.OnLanguageChange += RefreshText;
        if (!initByCode) RefreshText();
    }
    public void RefreshText(GameData.TypeLanguage language = GameData.TypeLanguage.English)
    {
        dialogText.text = GameData.Instance.GetDialog(id, typeLOCS);
    }
    public void ChangeTextById(int textId)
    {
        id = textId;
        RefreshText();
    }
}