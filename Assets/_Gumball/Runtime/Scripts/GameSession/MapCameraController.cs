using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MyBox;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class MapCameraController : MonoBehaviour
    {
        
        [SerializeField] private Vector2 movementSpeed = new(10, 5);
        [SerializeField] private float decelerationDuration = 0.2f;
        [SerializeField] private Ease decelerationEase = Ease.InOutSine;

        private Vector3 velocity;
        private Tween decelerationTween;
        
        private void OnEnable()
        {
            PrimaryContactInput.onDrag += OnDrag;
            PrimaryContactInput.onPress += OnPress;
            PrimaryContactInput.onRelease += OnRelease;
        }
        
        private void OnDisable()
        {
            PrimaryContactInput.onDrag -= OnDrag;
            PrimaryContactInput.onPress -= OnPress;
            PrimaryContactInput.onRelease -= OnRelease;
        }

        private void OnDrag(Vector2 offset)
        {
            if (IsGraphicUnderPointer())
                return;
            
            offset *= movementSpeed;
            velocity = MapSceneManager.Instance.CurrentSelectedMap.CameraMovementPlane.TransformDirection(Vector3.left * offset.x + Vector3.down * offset.y);
            MoveCamera(velocity);
        }

        private void MoveCamera(Vector3 offset)
        {
            Camera.main.transform.Translate(offset, Space.World);
        }
        
        private void OnPress()
        {
            decelerationTween?.Kill();
            velocity = Vector2.zero;
        }
        
        private void OnRelease()
        {
            decelerationTween = DOTween.To(() => velocity, x => velocity = x, Vector3.zero, decelerationDuration)
                .OnUpdate(() => MoveCamera(velocity)).SetEase(decelerationEase);
        }

        private bool IsGraphicUnderPointer()
        {
            foreach (Graphic graphic in PrimaryContactInput.GetClickableGraphicsUnderPointer())
            {
                //don't include nodes
                bool isNode = graphic.GetComponent<GameSessionNode>() != null || graphic.transform.GetComponentInAllParents<GameSessionNode>() != null;
                if (!isNode)
                    return true;
            }

            return false;
        }
    }
}
