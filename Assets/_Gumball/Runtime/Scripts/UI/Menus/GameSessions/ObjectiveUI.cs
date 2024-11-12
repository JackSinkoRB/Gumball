using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    [ExecuteAlways]
    public class ObjectiveUI : MonoBehaviour
    {

        [Serializable]
        public struct FakeChallengeData
        {
            [SerializeField] private string displayName;
            [SerializeField] private Sprite icon;

            public string DisplayName => displayName;
            public Sprite Icon => icon;
        }
        
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI nameLabel;
        [SerializeField] private TextMeshProUGUI goalValueLabel;

        private void LateUpdate()
        {
            GetComponent<RectTransform>().SetWidth(transform.parent.GetComponent<RectTransform>().sizeDelta.x);
        }

        public void Initialise(Challenge challenge)
        {
            nameLabel.text = challenge.Tracker.DisplayName;
            icon.sprite = challenge.Icon;
            
            ChallengeTracker.Listener challengeListener = challenge.Tracker.GetListener(challenge.UniqueID);
            goalValueLabel.text = challenge.Tracker.GetValueFormatted(challengeListener.Goal);
        }

        public void Initialise(FakeChallengeData objectiveData, string objectiveValue)
        {
            nameLabel.text = objectiveData.DisplayName;
            icon.sprite = objectiveData.Icon;
            goalValueLabel.text = objectiveValue;
        }
        
        public void Initialise(SkillCheck skillCheck)
        {
            nameLabel.text = skillCheck.DisplayName;
            icon.sprite = skillCheck.Icon;
            goalValueLabel.text = Mathf.RoundToInt(skillCheck.PointsSinceSessionStart).ToString(CultureInfo.InvariantCulture);
        }

    }
}
