using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MyBox;
using UnityEngine;

[Serializable]
public class SlideElement : AnimatedElement
{

    public enum ScreenPosition
    {
        NONE,
        TOP,
        BOTTOM
    }
    
    public RectTransform rectTransform;

    [Space(5)] [ConditionalField(nameof(doShowAnimation))]
    public Vector3 shownPosition;

    [ConditionalField(nameof(doHideAnimation))]
    public ScreenPosition hideOutsideScreen;

    [ConditionalField(new[] { nameof(doHideAnimation), nameof(hideOutsideScreen) }, new[] { false, true })]
    [SerializeField] private Vector2 hiddenPosition;
    public Vector2 HiddenPosition
    {
        get
        {
            switch (hideOutsideScreen)
            {
                case ScreenPosition.NONE:
                    return hiddenPosition;
                case ScreenPosition.TOP:
                {
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,
                        new Vector2(0, Screen.height), null, out var topOfScreenLocal);
                    return new Vector2(0, topOfScreenLocal.y);
                }
                case ScreenPosition.BOTTOM:
                {
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,
                        new Vector2(0, -Screen.height), null, out var topOfScreenLocal);
                    return new Vector2(0, topOfScreenLocal.y);
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    [Space(5)] [ConditionalField(nameof(doShowAnimation))]
    public Ease showEase;

    [ConditionalField(nameof(doHideAnimation))]
    public Ease hideEase;
    
    public SlideElement(
        RectTransform rectTransform,
        bool doShowAnimation = true,
        bool doHideAnimation = true,
        bool ignoreTimescale = true,
        float showDelay = 0,
        float hideDelay = 0,
        float showDuration = 0.5f,
        float hideDuration = 0.5f,
        Ease showEase = Ease.InOutSine,
        Ease hideEase = Ease.InOutSine,
        bool startHidden = true,
        bool disableWhenHidden = true,
        ScreenPosition hideOutsideScreen = ScreenPosition.NONE,
        Vector3 hiddenPosition = default,
        Vector3 shownPosition = default)
    {
        this.rectTransform = rectTransform;
        this.doShowAnimation = doShowAnimation;
        this.doHideAnimation = doHideAnimation;
        this.ignoreTimescale = ignoreTimescale;
        this.showDelay = showDelay;
        this.hideDelay = hideDelay;
        this.showDuration = showDuration;
        this.hideDuration = hideDuration;
        this.showEase = showEase;
        this.hideEase = hideEase;
        this.startHidden = startHidden;
        this.disableWhenHidden = disableWhenHidden;
        this.hideOutsideScreen = hideOutsideScreen;
        this.hiddenPosition = hiddenPosition;
        this.shownPosition = shownPosition;
    }
    
    protected override Tween DoShowAnimation()
    {
        rectTransform.gameObject.SetActive(true);

        if (showDuration == 0)
        {
            rectTransform.anchoredPosition = shownPosition;
            return null;
        }

        return rectTransform.DOAnchorPos(shownPosition, showDuration).SetEase(showEase);
    }

    protected override Tween DoHideAnimation()
    {
        if (hideDuration == 0)
        {
            rectTransform.anchoredPosition = HiddenPosition;
            if (disableWhenHidden)
                rectTransform.gameObject.SetActive(false);
            return null;
        }

        Tween tween = rectTransform.DOAnchorPos(HiddenPosition, hideDuration).SetEase(hideEase);
         
        if (disableWhenHidden)
            tween.OnComplete(() =>
            {
                if (rectTransform != null) //might be null if app is exiting
                    rectTransform.gameObject.SetActive(false);
            });

        return tween;
    }
    
}