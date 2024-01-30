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
            [SerializeField] private Vector3 positionMale;
            [SerializeField] private Vector3 rotationEulerMale;

            [SerializeField] private Vector3 positionFemale;
            [SerializeField] private Vector3 rotationEulerFemale;

            private bool useMalePosition => AvatarEditor.Instance.CurrentSelectedAvatar.CurrentBodyType == AvatarBodyType.MALE;
            
            public Vector3 Position => useMalePosition ? positionMale : positionFemale;
            public Vector3 RotationEuler => useMalePosition ? rotationEulerMale : rotationEulerFemale;
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
            AvatarEditor.onSessionStart += OnSessionStart;
            AvatarCosmeticSelector.onSelectCosmetic += OnSelectCosmetic;
            AvatarCosmeticSelector.onDeselectCosmetic += OnDeselectCosmetic;
        }

        private void OnDisable()
        {
            AvatarEditor.onSessionStart -= OnSessionStart;
            AvatarCosmeticSelector.onSelectCosmetic -= OnSelectCosmetic;
            AvatarCosmeticSelector.onDeselectCosmetic -= OnDeselectCosmetic; 
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
        
        private void OnSessionStart()
        {
            SetPosition(CameraPositionType.FULL_BODY, true);
        }
        
        private void OnSelectCosmetic(AvatarCosmetic cosmetic)
        {
            SetPosition(cosmetic.CameraPosition);
        }
        
        private void OnDeselectCosmetic()
        {
            SetPosition(CameraPositionType.FULL_BODY);
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
