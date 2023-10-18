using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MyBox;
using UnityEngine;

[Serializable]
public class FadeElement : AnimatedElement
{
    
    public RectTransform rectTransform;

    [Space(5)]
    [ConditionalField(nameof(doShowAnimation))] public Ease showEase = Ease.InOutSine;
    [ConditionalField(nameof(doHideAnimation))] public Ease hideEase = Ease.InOutSine;

    protected override Tween DoShowAnimation()
    {
        rectTransform.gameObject.SetActive(true);
        CanvasGroup canvasGroup = rectTransform.GetOrAddComponent<CanvasGroup>(); 
            
        if (showDuration == 0)
        {
            canvasGroup.alpha = 1;
            return null;
        }
            
        return canvasGroup.DOFade(1, showDuration).SetEase(showEase);
    }
        
    protected override Tween DoHideAnimation()
    {
        CanvasGroup canvasGroup = rectTransform.GetOrAddComponent<CanvasGroup>();

        if (hideDuration == 0)
        {
            canvasGroup.alpha = 0;
            if (disableWhenHidden)
                rectTransform.gameObject.SetActive(false);
            return null;
        }

        Tween tween = canvasGroup.DOFade(0, hideDuration).SetEase(hideEase);

        if (disableWhenHidden)
            tween.OnComplete(() => rectTransform.gameObject.SetActive(false));

        return tween;
    }
    
}