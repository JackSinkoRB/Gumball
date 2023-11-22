using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Codice.CM.Client.Differences;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [RequireComponent(typeof(Rigidbody))]
    public class TrafficCar : MonoBehaviour
    {

        private const float timeBetweenDelayedUpdates = 1;
        private const float carActivationRange = 100;

        [SerializeField] private Transform[] frontWheels;
        [SerializeField] private Transform[] wheels;
        [SerializeField] private float wheelRotateSpeed = 10;
        
        [SerializeField] private float moveSpeed = 10; //todo: remove and use the chunk's speed limit
        [SerializeField] private float turnSpeed = 10;

        [Header("Debugging")]
        [SerializeField] private bool debug;
        [SerializeField, ReadOnly] private bool activated;
        [SerializeField, ReadOnly] private Chunk currentChunk;

        private float timeSinceLastDelayedUpdate;
        private float currentLaneDistance;
        private readonly RaycastHit[] terrainRaycastHits = new RaycastHit[1];

        private Rigidbody rigidbody => GetComponent<Rigidbody>();

        public void Initialise(Chunk currentChunk)
        {
            this.currentChunk = currentChunk;
        }
        
        private void OnEnable()
        {
            DelayedUpdate();
        }
        
        private void Update()
        {
            TryDelayedUpdate();
            
            Move();
            if (activated)
            {
                RotateWheels();
            }
        }
        
        private void DelayedUpdate()
        {
            //ChunkCheck();
            ActivationRangeCheck();
        }
        
        public void SetLaneDistance(float laneDistance)
        {
            currentLaneDistance = laneDistance;
        }
        
        private void ActivationRangeCheck()
        {
            float carActivationRangeSqr = carActivationRange * carActivationRange;
            float distanceToPlayerSqr = Vector3.SqrMagnitude(PlayerCarManager.Instance.CurrentCar.transform.position - transform.position);
            if (!activated && distanceToPlayerSqr < carActivationRangeSqr)
                OnActivate();
            else if (activated && distanceToPlayerSqr > carActivationRangeSqr)
                OnDeactivate();
        }

        private void OnActivate()
        {
            activated = true;
        }

        private void OnDeactivate()
        {
            activated = false;
        }
        
        private void RotateWheels()
        {
            float speed = transform.InverseTransformDirection(rigidbody.velocity).z;

            foreach (Transform wheel in wheels)
            {
                wheel.Rotate(Vector3.up, wheelRotateSpeed * speed * Time.deltaTime, Space.Self);
            }

            foreach (Transform wheel in frontWheels)
            {
                const int turningDistance = 5; //the number of spline samples
                
                int closestSplineIndex = currentChunk.GetClosestSampleIndexOnSpline(transform.position, true).Item1;
                bool faceForward = currentChunk.TrafficManager.DriveOnLeft && currentLaneDistance < 0;
                int desiredSplineSampleIndex = faceForward ? closestSplineIndex + turningDistance : closestSplineIndex - turningDistance;
                if (desiredSplineSampleIndex >= currentChunk.SplineSamples.Length || desiredSplineSampleIndex < 0)
                {
                    //todo: get next chunk
                    break;
                }
                
                var (position, rotation) = currentChunk.TrafficManager.GetLanePosition(currentChunk.SplineSamples[desiredSplineSampleIndex], currentLaneDistance);
                Vector3 directionAhead = (position - transform.position).normalized;
                
                float angle = Vector2.SignedAngle(transform.forward.FlattenAsVector2(), directionAhead.FlattenAsVector2());

                if (debug)
                {
                    GlobalLoggers.TrafficLogger.Log("Angle is " + angle);
                    Debug.DrawRay(wheel.transform.position, transform.forward.Flatten() * 5, Color.blue);
                    Debug.DrawRay(wheel.transform.position, directionAhead.Flatten() * 5, Color.red);
                }
                
                wheel.LookAt(wheel.transform.position + directionAhead, wheel.transform.up);
            }

        }
        
        private void Move()
        {
            var targetPos = GetTargetPosition();
            if (targetPos == null)
            {
                Despawn();
                return;
            }
            
            var (targetPosition, targetRotation) = targetPos.Value;
            
            Vector3 direction = (targetPosition - transform.position).normalized;
            Vector3 targetVelocity = direction * moveSpeed;
            rigidbody.velocity = Vector3.Lerp(rigidbody.velocity, targetVelocity, moveSpeed * Time.deltaTime);

            rigidbody.MoveRotation(Quaternion.Slerp(rigidbody.rotation, targetRotation, turnSpeed * Time.deltaTime));
        }

        /// <summary>
        /// Get the next desired position and rotation relative to the sample on the next chunk's spline.
        /// </summary>
        /// <returns>The spline sample's position and rotation, or null if no more loaded chunks in the desired direction.</returns>
        private (Vector3, Quaternion)? GetTargetPosition()
        {
            if (currentChunk.TrafficManager == null)
            {
                Debug.LogWarning($"A traffic car is on the chunk {currentChunk.gameObject.name}, but it doesn't have a traffic manager.");
                return null;
            }
            
            int closestSplineIndex = currentChunk.GetClosestSampleIndexOnSpline(transform.position, true).Item1;
            bool faceForward = currentChunk.TrafficManager.DriveOnLeft && currentLaneDistance < 0;
            int desiredSplineSampleIndex = faceForward ? closestSplineIndex + 1 : closestSplineIndex - 1;

            int chunkIndex = ChunkManager.Instance.GetMapIndexOfLoadedChunk(currentChunk);
            while (desiredSplineSampleIndex >= currentChunk.SplineSamples.Length || desiredSplineSampleIndex < 0) //has reached end of chunk
            {
                //get the next chunk
                chunkIndex = faceForward ? chunkIndex + 1 : chunkIndex - 1;

                if (!ChunkManager.Instance.IsChunkLoaded(chunkIndex))
                {
                    //no more loaded chunks
                    return null;
                }

                Chunk newChunk = ChunkManager.Instance.GetLoadedChunkByMapIndex(chunkIndex);
                currentChunk = newChunk;
                closestSplineIndex = newChunk.GetClosestSampleIndexOnSpline(transform.position, true).Item1;
                desiredSplineSampleIndex = faceForward ? closestSplineIndex + 1 : closestSplineIndex - 1;
            }
            
            var (position, rotation) = currentChunk.TrafficManager.GetLanePosition(currentChunk.SplineSamples[desiredSplineSampleIndex], currentLaneDistance);

            return (position, rotation);
        }

        private void TryDelayedUpdate()
        {
            timeSinceLastDelayedUpdate += Time.deltaTime;

            if (timeSinceLastDelayedUpdate > timeBetweenDelayedUpdates)
            {
                timeSinceLastDelayedUpdate = 0;
                DelayedUpdate();
            }
        }

        private void Despawn()
        {
            gameObject.Pool();
        }

    }
}
