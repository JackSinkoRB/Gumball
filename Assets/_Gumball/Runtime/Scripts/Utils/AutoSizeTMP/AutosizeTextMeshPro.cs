using TMPro;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
public static class AutosizeTextMeshProCreator
{
    [MenuItem("GameObject/UI/Text - TextMeshPro - Autosize")]
    private static void Create()
    {
        GameObject gameObject = new GameObject("AutosizeTextMeshPro");
        gameObject.transform.SetParent(Selection.activeTransform);
        AutosizeTextMeshPro autosizeTmp = gameObject.AddComponent<AutosizeTextMeshPro>();
        autosizeTmp.SetText("Autosizing Text");
    }
}
#endif

[RequireComponent(typeof(AutosizeTMPSettings))]
public class AutosizeTextMeshPro : TextMeshProUGUI
{
    
    private bool isDirty;

    private AutosizeTMPSettings settingsCached;
    private AutosizeTMPSettings settings
    {
        get
        {
            if (settingsCached == null)
                settingsCached = GetComponent<AutosizeTMPSettings>();
            return settingsCached;
        }
    }

    protected override void UpdateMaterial()
    {
        base.UpdateMaterial();
        
        Resize();
    }

    public void Resize()
    {
        Vector2 preferredValues = GetPreferredValues();
        Vector2 textSize = new Vector2(preferredValues.x, preferredValues.y) + settings.extraPadding;
        enableWordWrapping = false;
        
        if (settings.otherRectsToModify != null && settings.otherRectsToModify.Length > 0)
        {
            foreach (RectTransform rect in settings.otherRectsToModify)
                rect.sizeDelta = textSize;
        }
        
        //just do for the current object
        rectTransform.sizeDelta = textSize;
    }
    
}
