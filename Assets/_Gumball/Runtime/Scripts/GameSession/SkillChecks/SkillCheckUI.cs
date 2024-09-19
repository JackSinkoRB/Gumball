using System.Collections;
using System.Collections.Generic;
using MyBox;
using TMPro;
using UnityEngine;

namespace Gumball
{
    public class SkillCheckUI : MonoBehaviour
    {

        [SerializeField] private bool autoHide = true;
        [SerializeField, ConditionalField(nameof(autoHide))] private float secondsToShow = 2;

        [Space(5)]
        [SerializeField] private TextMeshProUGUI pointBonusLabel;

        private Coroutine hideCoroutine;
        
        public virtual void Show(float pointBonus)
        {
            gameObject.SetActive(true);
            
            if (autoHide)
            {
                if (hideCoroutine != null)
                    StopCoroutine(hideCoroutine);
                hideCoroutine = this.PerformAfterDelay(secondsToShow, Hide);
            }

            pointBonusLabel.text = Mathf.RoundToInt(pointBonus).ToString();
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }
        
    }
}
