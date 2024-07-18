using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Gumball
{
    public class PointsUI : MonoBehaviour
    {

        [SerializeField] private TextMeshProUGUI pointsLabel;

        private void LateUpdate()
        {
            pointsLabel.text = $"{Mathf.RoundToInt(SkillCheckManager.Instance.CurrentPoints)}";
        }

    }
}
