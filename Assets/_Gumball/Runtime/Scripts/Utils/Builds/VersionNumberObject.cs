using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Gumball
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class VersionNumberObject : MonoBehaviour
    {

        private TextMeshProUGUI label => GetComponent<TextMeshProUGUI>();

        private void Awake()
        {
#if UNITY_EDITOR
            label.text = $"EDITOR_{VersionManager.Instance.ShortBuildName}";
#else
            label.text = VersionManager.Instance.FullBuildName;
#endif
        }

    }
}