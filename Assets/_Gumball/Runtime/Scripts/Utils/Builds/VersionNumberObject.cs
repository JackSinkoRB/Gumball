using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VersionNumberObject : MonoBehaviour
{
    
    private void Awake()
    {
#if UNITY_EDITOR
        GetComponent<TextMeshProUGUI>().text = $"EDITOR_{VersionManager.Instance.ShortBuildName}";
#else
        GetComponent<TextMeshProUGUI>().text = VersionManager.Instance.FullBuildName;
#endif
    }
    
}
