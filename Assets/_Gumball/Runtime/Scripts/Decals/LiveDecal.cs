using System;
using System.Collections;
using System.Collections.Generic;
using PaintIn3D;
using UnityEngine;
using MyBox;

namespace Gumball
{
    public class LiveDecal : MonoBehaviour
    {

        [SerializeField] private P3dPaintDecal paintDecal;
        [SerializeField] private LayerMask raycastLayers;

        private Vector3 clickOffset;
        private Vector3 lastKnownPosition;
        private Quaternion lastKnownRotation;

        public P3dPaintDecal PaintDecal => paintDecal;

        private void OnEnable()
        {
            UpdatePosition();
            PrimaryContactInput.onPerform += OnPrimaryContactPerformed;
        }

        private void OnDisable()
        {
            PrimaryContactInput.onPerform -= OnPrimaryContactPerformed;
        }

        private void Update()
        {
            DrawPreview();
        }
        
        public void Apply()
        {
            paintDecal.HandleHitPoint(false, int.MaxValue, 1, 0, lastKnownPosition, lastKnownRotation);
        }
        
        private void OnPrimaryContactPerformed()
        {
            if (DecalManager.Instance.CurrentSelectedDecal == this)
                UpdatePosition();
        }

        private void UpdatePosition()
        {
            Ray ray = Camera.main.ScreenPointToRay(PrimaryContactInput.Position);
    
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, raycastLayers))
            {
                if (hit.collider.GetComponent<P3dPaintable>())
                {
                    lastKnownPosition = hit.point;
                    lastKnownRotation = Quaternion.LookRotation(-hit.normal);

                    transform.position = lastKnownPosition;
                }
            }
        }

        private void DrawPreview()
        {
            paintDecal.HandleHitPoint(true, int.MaxValue, 1, 0, lastKnownPosition, lastKnownRotation);
        }

    }
}
