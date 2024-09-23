using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class NearMissSkillCheckUI : SkillCheckUI
    {

        [SerializeField] private Image background;
        [SerializeField] private float backgroundInTweenDuration = 1;
        [SerializeField] private float backgroundOutTweenDuration = 0.4f;
        [SerializeField] private Ease backgroundInTweenEase;
        [SerializeField] private Ease backgroundOutTweenEase;
        
        [Space(5)]
        [SerializeField] private TextMeshProUGUI titleLabel;
        [SerializeField] private float titleLabelInTweenDuration = 1;
        [SerializeField] private float titleLabelOutTweenDuration = 0.4f;
        [SerializeField] private Ease titleLabelTweenEase;

        [Space(5)]
        [SerializeField] protected TextMeshProUGUI pointBonusLabel;
        [SerializeField] private float pointsLabelInTweenDuration = 1;
        [SerializeField] private float pointsLabelOutTweenDuration = 0.4f;
        [SerializeField] private Ease pointsLabelTweenEase;
        
        private Sequence tween;

        [ButtonMethod]
        public void ShowTest()
        {
            Show(1000);
        }
        [ButtonMethod]
        public void HideTest()
        {
            Hide();
        }

        public override void Show(int pointBonus)
        {
            base.Show(pointBonus);
                
            tween?.Kill();
            tween = DOTween.Sequence();
            
            tween.Join(GetBackgroundTween(true));
            tween.Join(GetTitleLabelTween(true));
            tween.Join(GetPointsLabelTween(true, pointBonus));
        }
        
        public override void Hide()
        {
            tween?.Kill();
            tween = DOTween.Sequence();
            
            tween.Join(GetBackgroundTween(false));
            tween.Join(GetTitleLabelTween(false));
            tween.Join(GetPointsLabelTween(false));
            
            tween.OnComplete(() => base.Hide());
        }

        private Tween GetBackgroundTween(bool show)
        {
            if (show)
            {
                //reset
                background.rectTransform.localScale = background.rectTransform.localScale.SetX(0);
            }
            
            return background.rectTransform.DOScaleX(show ? 1 : 0, show ? backgroundInTweenDuration : backgroundOutTweenDuration).SetEase(show ? backgroundInTweenEase : backgroundOutTweenEase);
        }

        private Tween GetTitleLabelTween(bool show)
        {
            if (show)
            {
                //reset
                titleLabel.maxVisibleCharacters = 0;
            }

            int charactersToShow = show ? titleLabel.textInfo.characterCount : 0;
            
            return DOTween.To(() => titleLabel.maxVisibleCharacters, x => titleLabel.maxVisibleCharacters = x, charactersToShow, show ? titleLabelInTweenDuration : titleLabelOutTweenDuration)
                .SetEase(titleLabelTweenEase);
        }
        
        private Tween GetPointsLabelTween(bool show, int points = 0)
        {
            if (show)
            {
                //reset
                pointBonusLabel.text = "0";
            }
            
            return DOTween.To(() => int.Parse(pointBonusLabel.text), x => pointBonusLabel.text = $"+{x}", points, show ? pointsLabelInTweenDuration : pointsLabelOutTweenDuration)
                .SetEase(pointsLabelTweenEase);
        }
        
    }
}
