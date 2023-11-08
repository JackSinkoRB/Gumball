using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class SelectedDecalUI : MonoBehaviour
    {

        [SerializeField] private Image invalidDecal;
        [Space(5)]
        [SerializeField] private Image image;
        [SerializeField] private Color validColor;
        [SerializeField] private Color invalidColor;
        
        [Header("Fade when modifying")]
        [SerializeField] private float fadeWhenModifying = 0.2f;
        [SerializeField] private float fadeDuration = 0.25f;
        [SerializeField] private Ease fadeEase = Ease.InOutSine;

        [Header("Scale/Rotation handle")]
        [SerializeField] private ButtonEvents scaleRotationHandle;
        [SerializeField] private float scaleSpeed = 1;

        private LiveDecal selectedDecal => DecalManager.Instance.CurrentSelected;

        public ButtonEvents ScaleRotationHandle => scaleRotationHandle;

        private Tween currentFadeTween;
        private bool isFaded;

        private void OnEnable()
        {
            scaleRotationHandle.onDrag += OnDragScaleRotationHandle;
        }

        private void OnDisable()
        {
            scaleRotationHandle.onDrag -= OnDragScaleRotationHandle;
        }

        public void Update()
        {
            if (selectedDecal == null)
            {
                if (isFaded)
                    Fade(false);
                
                gameObject.SetActive(false);
                return;
            }
            
            gameObject.SetActive(true);

            MovePosition();
        }
        
        private void OnDragScaleRotationHandle(Vector2 offset)
        {
            //move right from initial click = scale up
            //move left from initial click = scale down
            //move up from initial click = rotate ccw
            //move down from initial click = rotate cw

            float scaleOffset = offset.x * (scaleSpeed * Time.deltaTime);
            float rotationOffset = offset.y;

            float newScale = selectedDecal.Scale.x + scaleOffset;
            selectedDecal.SetScale(newScale);
        }

        public void Fade(bool fade)
        {
            if (isFaded == fade)
                return; //already faded
            
            isFaded = fade;
            currentFadeTween?.Kill();
            currentFadeTween = this.GetComponent<CanvasGroup>(true)
                .DOFade(fade ? fadeWhenModifying : 1, fadeDuration)
                .SetEase(fadeEase);
        }
        
        private void MovePosition()
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(selectedDecal.transform.position);
            transform.position = screenPos;
            CheckIfValidPosition();
        }

        private void CheckIfValidPosition()
        {
            image.color = selectedDecal.IsValidPosition ? validColor : invalidColor;
            
            invalidDecal.gameObject.SetActive(!selectedDecal.IsValidPosition);
            if (selectedDecal.IsValidPosition)
                invalidDecal.sprite = selectedDecal.Sprite;
        }
        
    }
}
