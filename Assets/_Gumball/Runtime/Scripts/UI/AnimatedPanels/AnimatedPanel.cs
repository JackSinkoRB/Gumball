using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Gumball;
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

    [SerializeField] private bool isAddedToPanelStack = true;
    
    [Header("Animation")]
    [SerializeField] private bool ignoreTimescale = true;
    [SerializeField] private bool disableWhenHidden = true;

    [Space(5)]
    [SerializeField] private FadeElement[] fadeElements;
    [SerializeField] private SlideElement[] slideElements;

    private bool isInitialised;
    private Sequence currentTween;

    public bool IsShowing { get; private set; }
    public bool IsTransitioning => currentTween != null && currentTween.IsActive() && currentTween.IsPlaying();

    private void OnEnable()
    {
        if (!isInitialised)
            Initialise();
    }

    protected virtual void Initialise()
    {
        isInitialised = true;
    }
    
    private void OnDisable()
    {
        if (IsShowing && !IsTransitioning)
            Hide(instant: true);
        
        currentTween?.Kill(); //ensure tween is killed
    }
    
    //shortcuts for unity events:
    public void Show() => Show(null);
    public void Hide() => Hide(false, false, null);

    public Sequence Show(Action onComplete = null)
    {
        if (IsShowing)
        {
            Debug.LogWarning($"Tried showing panel {gameObject.name} but it is already showing.");
            return null;
        }
        
        gameObject.SetActive(true);
        currentTween?.Kill();
        currentTween = DOTween.Sequence();

        //do slides
        foreach (SlideElement slideElement in slideElements)
        {
            Tween slideTween = slideElement.TryShow();
            if (slideTween != null)
                currentTween.Join(slideTween);
        }

        //do fades
        foreach (FadeElement fadeElement in fadeElements)
        {
            Tween fadeTween = fadeElement.TryShow();
            if (fadeTween != null)
                currentTween.Join(fadeTween);
        }

        currentTween.OnComplete(() =>
        {
            OnShowComplete();
            onComplete?.Invoke();
        });

        if (ignoreTimescale)
            currentTween.SetUpdate(true);

        IsShowing = true;

        if (isAddedToPanelStack && !PanelManager.Instance.PanelStack.Contains(this))
            PanelManager.Instance.AddToStack(this);
        
        OnShow();
        
        GlobalLoggers.PanelLogger.Log($"Showing {gameObject.name}.");

        return currentTween;
    }

    public Sequence Hide(bool keepInStack = false, bool instant = false, Action onComplete = null)
    {
        if (!IsShowing && !instant)
        {
            Debug.LogWarning($"Tried hiding panel {gameObject.name} but it is not already showing.");
            return null;
        }

        //animate:
        currentTween?.Kill();
        currentTween = DOTween.Sequence();

        //do slides
        foreach (SlideElement slideElement in slideElements)
        {
            Tween slideTween = slideElement.TryHide();
            if (slideTween != null)
                currentTween.Join(slideTween);
        }

        //do fades
        foreach (FadeElement fadeElement in fadeElements)
        {
            Tween fadeTween = fadeElement.TryHide();
            if (fadeTween != null)
                currentTween.Join(fadeTween);
        }

        currentTween.OnComplete(() =>
        {
            if (disableWhenHidden)
                gameObject.SetActive(false);

            onComplete?.Invoke();
            OnHideComplete();
        });
        
        if (ignoreTimescale)
            currentTween.SetUpdate(true);

        if (instant)
            currentTween.Complete();

        IsShowing = false;

        if (!keepInStack && PanelManager.ExistsRuntime && PanelManager.Instance.PanelStack.Contains(this))
            PanelManager.Instance.RemoveFromStack(this);
        
        OnHide();
        
        GlobalLoggers.PanelLogger.Log($"Hiding {gameObject.name}.");

        return currentTween;
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
}