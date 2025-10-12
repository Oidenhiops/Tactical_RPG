using System;
using TMPro;
using UnityEngine;

public class ManagementLanguage : MonoBehaviour
{
    public GameData.TypeLOCS typeLOCS;
    public TMP_Text dialogText;
    public string id = "";
    public bool isDescription;
    public bool initByCode;
    public string[] otherInfo;
    [NonSerialized] public string[] dialogIds = { };
    void OnValidate()
    {
        if (dialogText == null) dialogText = GetComponent<TMP_Text>();
    }
    void OnDestroy()
    {
        if (!isDescription)
        {
            GameData.Instance.systemDataInfo.configurationsInfo.OnLanguageChange -= RefreshDialog;
        }
        else
        {
            GameData.Instance.systemDataInfo.configurationsInfo.OnLanguageChange -= RefreshDescription;
        }
    }
    void Awake()
    {
        if (!isDescription)
        {
            GameData.Instance.systemDataInfo.configurationsInfo.OnLanguageChange += RefreshDialog;
            if (!initByCode) RefreshDialog();
        }
        else
        {
            GameData.Instance.systemDataInfo.configurationsInfo.OnLanguageChange += RefreshDescription;
            if (!initByCode) RefreshDescription();
        }
    }
    public void RefreshDialog(GameData.TypeLanguage language = GameData.TypeLanguage.English)
    {
        dialogText.text = GameData.Instance.GetDialog(id, typeLOCS).dialog;
    }
    public void RefreshDescription(GameData.TypeLanguage language = GameData.TypeLanguage.English)
    {
        if (otherInfo.Length == 0)
        {
            dialogText.text = GameData.Instance.GetDialog(id, typeLOCS).description;
        }
        else
        {
            dialogText.text = string.Format(GameData.Instance.GetDialog(id, typeLOCS).description, otherInfo);
        }
    }
    public void ChangeTextById(string textId)
    {
        id = textId;
        RefreshDialog();
    }
}