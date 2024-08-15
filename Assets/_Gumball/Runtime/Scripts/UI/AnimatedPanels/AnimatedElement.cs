using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MyBox;
using UnityEngine;

[Serializable]
public abstract class AnimatedElement
{

    [SerializeField] protected bool ignoreTimescale = true;
    
    [SerializeField] protected bool startHidden = true;
    [ConditionalField(nameof(doHideAnimation)), SerializeField] protected bool disableWhenHidden = true;
    
    [Space(5)]
    [SerializeField] protected bool doShowAnimation = true;
    [SerializeField] protected bool doHideAnimation = true;
    
    [Space(5)]
    [ConditionalField(nameof(doShowAnimation)), SerializeField] protected float showDelay;
    [ConditionalField(nameof(doHideAnimation)), SerializeField] protected float hideDelay;
    
    [Space(5)]
    [ConditionalField(nameof(doShowAnimation)), PositiveValueOnly] public float showDuration = 0.5f;
    [ConditionalField(nameof(doHideAnimation)), PositiveValueOnly] public float hideDuration = 0.5f;

    private bool isInitialised;
        
    private void Initialise()
    {
        if (isInitialised)
            return;

        if (startHidden)
            TryHide().Complete();
            
        isInitialised = true;
    }
        
    public Tween TryShow()
    {
        if (!doShowAnimation)
            return null;
        
        if (startHidden)
            DoHideAnimation().Complete();

        Initialise();
            
        Tween tween = DoShowAnimation().SetDelay(showDelay);

        if (ignoreTimescale)
            tween.SetUpdate(true);

        return tween;
    }

    public Tween TryHide()
    {
        if (!doHideAnimation)
            return null;

        Tween tween = DoHideAnimation().SetDelay(hideDelay);

        if (ignoreTimescale)
            tween.SetUpdate(true);

        return tween;
    }
        
    protected abstract Tween DoShowAnimation();
    
    protected abstract Tween DoHideAnimation();
    
}
