using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class AvatarRotater : MonoBehaviour
    {

        [SerializeField] private float defaultRotation = -180;
        [SerializeField] private float rotateSpeed = 10;

        [SerializeField] private float defaultRotationTweenDuration = 0.2f;
        [SerializeField] private Ease defaultRotationTweenEase = Ease.InOutSine;
        
        private Tween defaultRotationTween;
        
        private void OnEnable()
        {
            PrimaryContactInput.onPress += OnPress;
            AvatarCosmeticSelector.onSelectCosmetic += OnSelectCosmetic;
        }

        private void OnDisable()
        {
            PrimaryContactInput.onPress -= OnPress;
            PrimaryContactInput.onDrag -= OnDragToRotate; //remove listener if disabled mid-press
            AvatarCosmeticSelector.onSelectCosmetic -= OnSelectCosmetic;
            OnRelease();
        }

        private void OnPress()
        {
            if (PrimaryContactInput.IsGraphicUnderPointer())
                return;
            
            PrimaryContactInput.onRelease += OnRelease;
            PrimaryContactInput.onDrag += OnDragToRotate;
        }

        private void OnDragToRotate(Vector2 offset)
        {
            defaultRotationTween?.Kill();
            AvatarEditor.Instance.CurrentSelectedAvatar.transform.Rotate(0, offset.x * rotateSpeed, 0);
        }

        private void OnRelease()
        {
            PrimaryContactInput.onDrag -= OnDragToRotate;
            PrimaryContactInput.onRelease -= OnRelease;
        }
        
        private void OnSelectCosmetic(AvatarCosmetic cosmetic)
        {
            SetDefaultRotation();
        }

        private void SetDefaultRotation()
        {
            defaultRotationTween?.Kill();
            defaultRotationTween = AvatarEditor.Instance.CurrentSelectedAvatar.transform.DORotate(Vector3.zero.SetY(defaultRotation), defaultRotationTweenDuration)
                .SetEase(defaultRotationTweenEase);
        }

    }
}
