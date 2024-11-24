using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class TweenHelper : MonoBehaviour
    {

        [Header("Position")]
        [SerializeField] private bool doPosition;

        [ConditionalField(nameof(doPosition)), SerializeField] private float positionDuration;
        [ConditionalField(nameof(doPosition)), SerializeField] private float positionDelay;
        [ConditionalField(nameof(doPosition)), SerializeField, Range(0, 1)] private float positionRandomisedDurationPercent = 0.1f;
        [ConditionalField(nameof(doPosition)), SerializeField] private float xMovement;
        [ConditionalField(nameof(doPosition)), SerializeField] private float yMovement;
        [ConditionalField(nameof(doPosition)), SerializeField] private float zMovement;
        [ConditionalField(nameof(doPosition)), SerializeField] private Ease positionEase;
        [ConditionalField(nameof(doPosition)), SerializeField, ReadOnly] private Vector3 defaultPosition;
        [ConditionalField(nameof(doPosition)), SerializeField, ReadOnly] private float finalPositionDuration;

        [Header("Scale")]
        [SerializeField] private bool doScale;

        [ConditionalField(nameof(doScale)), SerializeField] private float scaleDuration;
        [ConditionalField(nameof(doScale)), SerializeField] private float scaleDelay;
        [ConditionalField(nameof(doScale)), SerializeField, Range(0, 1)] private float scaleRandomisedDurationPercent = 0.1f;
        [ConditionalField(nameof(doScale)), SerializeField] private Vector3 scaleAmount;
        [ConditionalField(nameof(doScale)), SerializeField] private Ease scaleEase;
        [ConditionalField(nameof(doScale)), SerializeField, ReadOnly] private Vector3 defaultScale;
        [ConditionalField(nameof(doScale)), SerializeField, ReadOnly] private float finalScaleDuration;

        private Sequence currentTween;
        private bool initialised;

        private void Initialise()
        {
            initialised = true;

            if (doPosition)
            {
                if (transform is RectTransform rectTransform)
                    defaultPosition = rectTransform.anchoredPosition;
                else defaultPosition = transform.position;

                float positionDurationAsPercent = positionRandomisedDurationPercent * positionDuration;
                finalPositionDuration = positionDuration *
                                        Random.Range(positionDuration - positionDurationAsPercent, positionDuration + positionDurationAsPercent);
            }

            if (doScale)
            {
                defaultScale = transform.localScale;

                float scaleDurationAsPercent = scaleRandomisedDurationPercent * scaleDuration;
                finalScaleDuration = scaleDuration *
                                     Random.Range(scaleDuration - scaleDurationAsPercent, scaleDuration + scaleDurationAsPercent);
            }
        }

        private void OnEnable()
        {
            StartTween();
        }

        private void OnDisable()
        {
            StopTween();
        }

        private void ResetTransform()
        {
            if (doPosition)
            {
                if (transform is RectTransform rectTransform)
                    rectTransform.anchoredPosition = defaultPosition;
                else transform.position = defaultPosition;
            }

            if (doScale)
                transform.localScale = defaultScale;
        }

        private void StartTween()
        {
            if (!initialised)
                Initialise();

            ResetTransform();

            currentTween?.Kill();

            currentTween = DOTween.Sequence();

            if (doPosition)
                currentTween.Join(GetPositionTween());

            if (doScale)
                currentTween.Join(GetScaleTween());

            currentTween.SetLoops(-1, LoopType.Yoyo);
        }

        private void StopTween()
        {
            currentTween?.Kill();
            currentTween = null;
        }

        private Tween GetPositionTween()
        {
            Tween tween;
            if (transform is RectTransform rectTransform)
                tween = DOTween.To(() => rectTransform.anchoredPosition, x => rectTransform.anchoredPosition = x,
                    new Vector2(defaultPosition.x, defaultPosition.y) + new Vector2(xMovement, yMovement), finalPositionDuration).SetDelay(positionDelay);
            else tween = transform.DOMove(defaultPosition + new Vector3(xMovement, yMovement, zMovement), finalPositionDuration).SetDelay(positionDelay);

            tween.SetEase(positionEase);
            return tween;
        }

        private Tween GetScaleTween()
        {
            Tween tween = transform.DOScale(defaultScale + scaleAmount, finalScaleDuration).SetDelay(scaleDelay);

            tween.SetEase(scaleEase);
            return tween;
        }

    }
}