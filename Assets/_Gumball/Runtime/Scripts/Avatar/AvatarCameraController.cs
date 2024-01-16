using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Gumball
{
    public class AvatarCameraController : MonoBehaviour
    {

        public enum CameraPositionType
        {
            FULL_BODY,
            HEAD,
            UPPER_BODY,
            LOWER_BODY,
            FEET
        }

        [Serializable]
        public struct CameraPosition
        {
            [SerializeField] private Vector3 position;
            [SerializeField] private Vector3 rotationEuler;

            public Vector3 Position => position;
            public Vector3 RotationEuler => rotationEuler;
            public Quaternion Rotation => Quaternion.Euler(rotationEuler);
        }

        [SerializeField] private float cameraTweenDuration = 0.4f;
        [SerializeField] private Ease cameraEase = Ease.InOutSine;
        
        [Header("Camera positions")]
        [SerializeField] private CameraPosition fullBodyPosition;
        [SerializeField] private CameraPosition headPosition;
        [SerializeField] private CameraPosition upperBodyPosition;
        [SerializeField] private CameraPosition lowerBodyPosition;
        [SerializeField] private CameraPosition feetPosition;

        private Sequence currentTween;
        
        private void OnEnable()
        {
            AvatarCosmeticDisplay.onSelectCosmetic += OnSelectCosmetic;
            
            SetPosition(CameraPositionType.FULL_BODY, true);
        }
        
        private void OnDisable()
        {
            AvatarCosmeticDisplay.onSelectCosmetic -= OnSelectCosmetic;
        }

        public void SetPosition(CameraPositionType type, bool instant = false) => SetPosition(GetCameraPositionFromType(type), instant);

        public void SetPosition(CameraPosition cameraPosition, bool instant = false)
        {
            currentTween?.Kill();

            Tween positionTween = Camera.main.transform
                .DOMove(cameraPosition.Position, cameraTweenDuration)
                .SetEase(cameraEase);
            
            Tween rotationTween = Camera.main.transform
                .DORotate(cameraPosition.RotationEuler, cameraTweenDuration)
                .SetEase(cameraEase);
            
            currentTween = DOTween.Sequence()
                .Join(positionTween)
                .Join(rotationTween);
            
            if (instant)
                currentTween.Complete();
        }
        
        private void OnSelectCosmetic(AvatarCosmetic cosmetic)
        {
            SetPosition(cosmetic.CameraPosition);
        }

        private CameraPosition GetCameraPositionFromType(CameraPositionType type)
        {
            return type switch
            {
                CameraPositionType.FULL_BODY => fullBodyPosition,
                CameraPositionType.HEAD => headPosition,
                CameraPositionType.UPPER_BODY => upperBodyPosition,
                CameraPositionType.LOWER_BODY => lowerBodyPosition,
                CameraPositionType.FEET => feetPosition,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, $"Unknown position type {type.ToString()}")
            };
        }

    }
}
