using System;
using System.Collections;
using System.Collections.Generic;
using Dreamteck.Splines;
#if UNITY_EDITOR
using Gumball.Editor;
#endif
using MyBox;
using UnityEngine;

namespace Gumball
{
    [RequireComponent(typeof(Rigidbody))]
    public class TrafficCar : AICar
    {
        

        private const float blockedPathDetectorDistance = 20;
        private const float desiredDistanceToBlockage = 5;
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private bool isPathBlocked;
        [SerializeField, ReadOnly] private float distanceToBlockage;

        private readonly RaycastHit[] blockingObjects = new RaycastHit[10];
        

        public override void Initialise()
        {
            base.Initialise();
            
            TrafficCarSpawner.TrackCar(this);
            
            //spawn at max speed once chunk exists
            this.PerformAfterTrue(() => CurrentChunk != null, SetMaxSpeed);
            
            gameObject.layer = (int)LayersAndTags.Layer.TrafficCar;
        }

        protected void OnDisable()
        {
            if (!isInitialised)
                return; //didn't get a chance to initialise
            
            TrafficCarSpawner.UntrackCar(this);
        }

        /// <summary>
        /// Sets the car's speed to max speed, and stops accelerating or decelerating.
        /// </summary>
        private void SetMaxSpeed()
        {
            //TODO: set the rigidbody velocity
            float newDesiredSpeed = CurrentChunk.TrafficManager.SpeedLimitKmh;
            OnChangeDesiredSpeed(newDesiredSpeed);
            ForceSetSpeed(newDesiredSpeed);
        }

        protected override void PreMoveChecks()
        {
            base.PreMoveChecks();
            
            isPathBlocked = CheckIfPathIsBlocked();
            if (isPathBlocked)
            {
                //if distance to blockage is greater than stopped distance, set to slow speed, else 0
                const float slowSpeed = 25;
                float decelerationSpeed = distanceToBlockage > desiredDistanceToBlockage ? Mathf.Clamp01(distanceToBlockage / blockedPathDetectorDistance) * slowSpeed : 0;
                OnChangeDesiredSpeed(decelerationSpeed);
            }
            else
            {
                float newDesiredSpeed = currentChunkCached.TrafficManager.SpeedLimitKmh;
                if (!newDesiredSpeed.Approximately(DesiredSpeed))
                {
                    OnChangeDesiredSpeed(newDesiredSpeed);
                }
            }
        }

        private bool CheckIfPathIsBlocked()
        {
            BoxCollider carCollider = GetComponent<BoxCollider>();
            Vector3 boxCastHalfExtents = carCollider.size / 2f;
            int hits = Physics.BoxCastNonAlloc(transform.TransformPoint(carCollider.center.OffsetZ(carCollider.size.z)), boxCastHalfExtents, transform.forward, blockingObjects, transform.rotation, blockedPathDetectorDistance, LayersAndTags.AICarCollisionLayers);

            RaycastHitSorter.SortRaycastHitsByDistance(blockingObjects, hits);

            Collider actualHit = null;
            
            //don't include the car's own collider
            for (int index = 0; index < hits; index++)
            {
                RaycastHit hit = blockingObjects[index];
                if (!ReferenceEquals(hit.transform.gameObject, gameObject.transform.gameObject))
                {
                    actualHit = hit.collider;
                    distanceToBlockage = hit.distance;
                    break;
                }
            }

#if UNITY_EDITOR
            BoxCastUtils.DrawBoxCastBox(transform.TransformPoint(carCollider.center.OffsetZ(carCollider.size.z)), boxCastHalfExtents, transform.rotation, transform.forward, blockedPathDetectorDistance, actualHit != null ? Color.magenta : Color.gray);
#endif
            
            return actualHit != null;
        }

    }
}
