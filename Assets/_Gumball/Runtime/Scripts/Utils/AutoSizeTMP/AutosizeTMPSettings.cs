using UnityEngine;

/// <summary>
/// Instead of modifying TMP editor class, just add our own settings
/// </summary>
[RequireComponent(typeof(AutosizeTextMeshPro))]
[ExecuteInEditMode]
public class AutosizeTMPSettings : MonoBehaviour
{

    public Vector2 extraPadding;
    
    [Tooltip("If other RectTransforms need to be the same size as the text, add them here.")]
    public RectTransform[] otherRectsToModify;
    
    private Vector2 lastKnownExtraPadding;
    private bool isDirty;
    
    private AutosizeTextMeshPro autosizeTmpCached;
    private AutosizeTextMeshPro autosizeTmp
    {
        get
        {
            if (autosizeTmpCached == null)
                autosizeTmpCached = GetComponent<AutosizeTextMeshPro>();
            return autosizeTmpCached;
        }
    }
    
    private void OnValidate()
    {
        CheckToSetDirty();
    }
    
    private void LateUpdate()
    {
        //we check in late update to modify the rect size, otherwise it will spam warnings trying to modify rect size in OnValidate
        if (isDirty)
        {
            autosizeTmp.Resize();
            isDirty = false;
        }
    }

    private void CheckToSetDirty()
    {
        if (lastKnownExtraPadding != extraPadding)
        {
            lastKnownExtraPadding = extraPadding;
            isDirty = true;
        }
    }
    
}
