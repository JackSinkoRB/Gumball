using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class NearMissSkillCheckUI : SkillCheckUI
    {

        [SerializeField] private Image background;
        [SerializeField] private float backgroundTweenDuration;
        [SerializeField] private Ease backgroundTweenEase;

        public override void Show(float pointBonus)
        {
            base.Show(pointBonus);
            
            
        }

        private Tween GetBackgroundTween()
        {
            return background.rectTransform.DOScaleX(1, backgroundTweenDuration).SetEase(backgroundTweenEase);
        }
    }
}
