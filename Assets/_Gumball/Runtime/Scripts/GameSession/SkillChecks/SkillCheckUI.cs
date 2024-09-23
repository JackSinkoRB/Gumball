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
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }
        
    }
}
