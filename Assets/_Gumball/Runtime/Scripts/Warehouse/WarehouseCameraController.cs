using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Gumball
{
    public class WarehouseCameraController : MonoBehaviour
    {

        [SerializeField] private float tweenDuration = 0.3f;
        [SerializeField] private Ease tweenEase = Ease.InOutSine;
        
        private Sequence currentTween;
        private bool hasMovedBefore;

        private void OnEnable()
        {
            WarehouseSceneManager.Instance.onSelectSlot += OnSelectCarSlot;
        }

        private void OnDisable()
        {
            WarehouseSceneManager.Instance.onSelectSlot -= OnSelectCarSlot;
        }
        
        private void OnSelectCarSlot(WarehouseCarSlot slot)
        {
            Move(slot.CameraPosition.Position, slot.CameraPosition.Rotation);
        }

        public void Move(Vector3 position, Quaternion rotation)
        {
            currentTween?.Kill();

            Tween positionTween = Camera.main.transform.DOMove(position, tweenDuration).SetEase(tweenEase);
            Tween rotationTween = Camera.main.transform.DORotate(rotation.eulerAngles, tweenDuration).SetEase(tweenEase);
            currentTween = DOTween.Sequence()
                .Join(positionTween)
                .Join(rotationTween);

            //complete instantly if first movement
            if (!hasMovedBefore)
            {
                currentTween?.Complete();
                hasMovedBefore = true;
            }
        }
        
    }
}
