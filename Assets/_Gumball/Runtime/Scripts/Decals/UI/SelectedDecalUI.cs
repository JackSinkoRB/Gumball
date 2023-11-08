using System;
using System.Collections;
using System.Collections.Generic;
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
        
        [Header("Scale/Rotation handle")]
        [SerializeField] private ButtonEvents scaleRotationHandle;
        [SerializeField] private float scaleSpeed = 1;

        private LiveDecal selectedDecal => DecalManager.Instance.CurrentSelected;

        public ButtonEvents ScaleRotationHandle => scaleRotationHandle;

        private void OnEnable()
        {
            scaleRotationHandle.onPressMove += OnPressMoveScaleRotationHandle;
        }

        private void OnDisable()
        {
            scaleRotationHandle.onPressMove -= OnPressMoveScaleRotationHandle;
        }

        public void Update()
        {
            if (selectedDecal == null)
            {
                gameObject.SetActive(false);
                return;
            }
            
            gameObject.SetActive(true);

            MovePosition();
        }
        
        private void OnPressMoveScaleRotationHandle(Vector2 offset)
        {
            //move right from initial click = scale up
            //move left from initial click = scale down
            //move up from initial click = rotate ccw
            //move down from initial click = rotate cw

            float scaleOffset = offset.x * scaleSpeed;
            float rotationOffset = offset.y;

            float newScale = selectedDecal.Scale.x + scaleOffset;
            selectedDecal.SetScale(newScale);
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
