using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class SkillCheckUI : MonoBehaviour
    {

        [Header("Settings")]
        [SerializeField] private bool autoHide = true;
        [SerializeField, ConditionalField(nameof(autoHide))] private float secondsToShow = 2;

        [Header("Animation")]
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
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private bool isShowing;
        
        private Coroutine hideCoroutine;
        private Sequence tween;
        
        public bool IsShowing => isShowing;
        public TextMeshProUGUI PointBonusLabel => pointBonusLabel;
        
        private void OnEnable()
        {
            GameSession.onSessionEnd += OnSessionEnd;
        }

        private void OnDisable()
        {
            GameSession.onSessionEnd -= OnSessionEnd;
        }
        
        public virtual void Show(int pointBonus)
        {
            gameObject.SetActive(true);
            isShowing = true;
            
            tween?.Kill();
            tween = DOTween.Sequence();
            
            tween.Join(GetBackgroundTween(true));
            tween.Join(GetTitleLabelTween(true));
            tween.Join(GetPointsLabelTween(true, pointBonus));

            if (autoHide)
            {
                if (hideCoroutine != null)
                    StopCoroutine(hideCoroutine);
                hideCoroutine = this.PerformAfterDelay(secondsToShow, Hide);
            }
        }

        public virtual void Hide()
        {
            isShowing = false;

            tween?.Kill();
            tween = DOTween.Sequence();
            
            tween.Join(GetBackgroundTween(false));
            tween.Join(GetTitleLabelTween(false));
            tween.Join(GetPointsLabelTween(false));
            
            tween.OnComplete(() =>
            {
                gameObject.SetActive(false);
            });
        }
        
        private void OnSessionEnd(GameSession session, GameSession.ProgressStatus progress)
        {
            Hide();
        }
        
        protected virtual Tween GetBackgroundTween(bool show)
        {
            if (show)
            {
                //reset
                background.rectTransform.localScale = background.rectTransform.localScale.SetX(0);
            }
            
            return background.rectTransform.DOScaleX(show ? 1 : 0, show ? backgroundInTweenDuration : backgroundOutTweenDuration).SetEase(show ? backgroundInTweenEase : backgroundOutTweenEase);
        }

        protected virtual Tween GetTitleLabelTween(bool show)
        {
            if (show)
            {
                //reset
                titleLabel.maxVisibleCharacters = 0;
            }

            int charactersToShow = show ? 99999 : 0;
            
            return DOTween.To(() => titleLabel.maxVisibleCharacters, x => titleLabel.maxVisibleCharacters = x, charactersToShow, show ? titleLabelInTweenDuration : titleLabelOutTweenDuration)
                .SetEase(titleLabelTweenEase);
        }
        
        protected virtual Tween GetPointsLabelTween(bool show, int points = 0)
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
