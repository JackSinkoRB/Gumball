using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Gumball;
using MyBox;
using UnityEngine;

/// <summary>
/// Easy way to animate UI when enabled and disabled.
/// </summary>
public abstract class AnimatedPanel : MonoBehaviour
{

    public event Action onShow;
    public event Action onHide;
    public event Action onShowComplete;
    public event Action onHideComplete;
    
    [Tooltip("Is the panel tracked in the PanelManager stack, or is it separate from it?")]
    [SerializeField] private bool isAddedToPanelStack = true;
    
    [Header("Animation")]
    [SerializeField] private bool ignoreTimescale = true;
    [SerializeField] private bool disableWhenHidden = true;

    [Space(5)]
    [SerializeField] private FadeElement[] fadeElements;
    [SerializeField] private SlideElement[] slideElements;

    private bool isInitialised;
    private readonly HashSet<Tween> currentTweens = new();
    private AnimatedElement[] allAnimatedElements;

    public bool IsShowing { get; private set; }

    public bool IsTransitioning
    {
        get
        {
            foreach (Tween tween in currentTweens)
            {
                if (tween != null && tween.IsActive() && tween.IsPlaying())
                    return true;
            }

            return false;
        }
    }
    
    protected virtual void Initialise()
    {
        isInitialised = true;

        CacheAllAnimatedElements();
    }

    private void OnDisable()
    {
        if (IsShowing && !IsTransitioning)
            Hide(instant: true);
        
        KillCurrentTweens();
    }
    
    //shortcuts for unity events:
    [ButtonMethod] public void Show() => Show(null);
    [ButtonMethod] public void Hide() => Hide(false, false, null);

    public void Show(Action onComplete = null)
    {
        if (!Application.isPlaying)
            throw new InvalidOperationException("Must be in play made to show panel.");
        
        if (!isInitialised)
            Initialise();
        
        if (IsShowing)
        {
            Debug.LogWarning($"Tried showing panel {gameObject.name} but it is already showing.");
            return;
        }
        
        gameObject.SetActive(true);
        KillCurrentTweens();

        Tween longestTween = null;
        foreach (AnimatedElement animatedElement in allAnimatedElements)
        {
            Tween tween = animatedElement.TryShow();
            if (tween == null)
                continue;
            
            currentTweens.Add(tween);

            float totalDuration = tween.Duration() + tween.Delay();
            float longestDuration = longestTween == null ? 0 : longestTween.Duration() + longestTween.Delay();
            if (totalDuration > longestDuration)
                longestTween = tween;
        }

        if (longestTween == null)
        {
            //complete instantly
            OnShowComplete();
            onComplete?.Invoke();
        }
        else
        {
            //complete when longest tween is complete
            longestTween.OnComplete(() =>
            {
                OnShowComplete();
                onComplete?.Invoke();
            });
        }

        if (ignoreTimescale)
        {
            foreach (Tween tween in currentTweens)
                tween.SetUpdate(true);
        }

        IsShowing = true;

        if (isAddedToPanelStack && !PanelManager.Instance.PanelStack.Contains(this))
            PanelManager.Instance.AddToStack(this);
        
        OnShow();
        
        GlobalLoggers.PanelLogger.Log($"Showing {gameObject.name}.");
    }

    public void Hide(bool keepInStack = false, bool instant = false, Action onComplete = null)
    {
        if (!Application.isPlaying)
            throw new InvalidOperationException("Must be in play made to hide panel.");
        
        if (!isInitialised)
            Initialise();
        
        if (!IsShowing && !instant)
        {
            Debug.LogWarning($"Tried hiding panel {gameObject.name} but it is not already showing.");
            return;
        }

        bool wasShowing = IsShowing;
        IsShowing = false;

        KillCurrentTweens();

        Tween longestTween = null;
        foreach (AnimatedElement animatedElement in allAnimatedElements)
        {
            Tween tween = animatedElement.TryHide();
            if (tween == null)
                continue;
            
            currentTweens.Add(tween);

            float totalDuration = tween.Duration() + tween.Delay();
            float longestDuration = longestTween == null ? 0 : longestTween.Duration() + longestTween.Delay();
            if (totalDuration > longestDuration)
                longestTween = tween;
        }

        if (longestTween == null)
        {
            //complete instantly
            if (disableWhenHidden)
                gameObject.SetActive(false);

            if (wasShowing)
            {
                onComplete?.Invoke();
                OnHideComplete();
            }
        }
        else
        {
            //complete when longest tween is complete
            longestTween.OnComplete(() =>
            {
                if (disableWhenHidden)
                    gameObject.SetActive(false);

                if (wasShowing)
                {
                    onComplete?.Invoke();
                    OnHideComplete();
                }
            });
        }

        if (ignoreTimescale)
        {
            foreach (Tween tween in currentTweens)
                tween.SetUpdate(true);
        }

        if (instant)
        {
            foreach (Tween tween in currentTweens)
                tween.Complete();
        }
        
        if (!keepInStack && PanelManager.ExistsRuntime && PanelManager.Instance.PanelStack.Contains(this))
            PanelManager.Instance.RemoveFromStack(this);
        
        if (wasShowing)
            OnHide();
        
        GlobalLoggers.PanelLogger.Log($"Hiding {gameObject.name}.");
    }

    public virtual void OnAddToStack()
    {
        
    }

    public virtual void OnRemoveFromStack()
    {
        
    }
    
    protected virtual void OnShow()
    {
        onShow?.Invoke();
    }

    protected virtual void OnHide()
    {
        onHide?.Invoke();
    }

    protected virtual void OnShowComplete()
    {
        onShowComplete?.Invoke();
    }

    protected virtual void OnHideComplete()
    {
        onHideComplete?.Invoke();
    }
    
    private void CacheAllAnimatedElements()
    {
        allAnimatedElements = new AnimatedElement[slideElements.Length + fadeElements.Length];
        Array.Copy(slideElements, allAnimatedElements, slideElements.Length);
        Array.Copy(fadeElements, 0, allAnimatedElements, slideElements.Length, fadeElements.Length);
    }
    
    private void KillCurrentTweens()
    {
        foreach (Tween tween in currentTweens)
            tween?.Kill();
        currentTweens.Clear();
    }

    public virtual void OnAddToPanelLookup()
    {
        
    }
    
}