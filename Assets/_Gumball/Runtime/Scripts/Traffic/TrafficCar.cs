using System;
using System.Collections;
using System.Collections.Generic;
using Dreamteck.Splines;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [RequireComponent(typeof(Rigidbody))]
    public class TrafficCar : MonoBehaviour
    {
        
        private const float timeBetweenDelayedUpdates = 1;
        private const float carActivationRange = 100;
        private const float collisionRecoverDuration = 5;
        private const int movementTargetDistance = 3; //the number of spline samples

        [SerializeField] private Transform[] frontWheels;
        [SerializeField] private Transform[] wheels;
        [SerializeField] private float wheelRotateSpeed = 85; //todo: adjust depending on the moveSpeed 
        [Space(5)]
        [SerializeField] private float maxSpeed = 10; //todo: remove and use the chunk's speed limit
        [SerializeField] private float turnSpeed = 3; //todo: adjust depending on moveSpeed
        [Space(5)]
        [SerializeField] private float accelerationDuration = 3;
        [SerializeField] private float decelerationDuration = 2;
        
        [Header("Debugging")]
        [SerializeField] private bool debug;
        [SerializeField, ReadOnly] private bool isInitialised;
        [SerializeField, ReadOnly] private bool isFrozen;
        [SerializeField, ReadOnly] private bool isActivated = true;
        [SerializeField, ReadOnly] private Chunk currentChunk;
        [Space(5)]
        [SerializeField, ReadOnly] private bool isMoving;
        [SerializeField, ReadOnly] private float speed;
        [SerializeField, ReadOnly] private bool isAccelerating;
        [SerializeField, ReadOnly] private float timeSinceAccelerating;
        [SerializeField, ReadOnly] private bool isDecelerating;
        [SerializeField, ReadOnly]  private float timeSinceDecelerating;
        [Space(5)]
        [SerializeField, ReadOnly] private float timeSinceCollision;
        [SerializeField, ReadOnly] private bool inCollision;

        private readonly List<Collision> collisions = new();
        private float timeSinceLastDelayedUpdate;
        private float currentLaneDistance;
        
        private bool recoveringFromCollision => timeSinceCollision < collisionRecoverDuration;
        private bool faceForward => currentChunk.TrafficManager.DriveOnLeft && currentLaneDistance < 0;
        private Rigidbody rigidbody => GetComponent<Rigidbody>();

        public void Initialise(Chunk currentChunk)
        {
            isInitialised = true;
            this.currentChunk = currentChunk;

            timeSinceCollision = Mathf.Infinity;
            gameObject.layer = (int)GameObjectLayers.Layer.TrafficCar;
            DelayedUpdate();
        }

        private void Update()
        {
            if (!isInitialised)
                return;
            
            CollisionFreezeCheck();
            TryDelayedUpdate();

            if (!isFrozen && !recoveringFromCollision)
            {
                Move();
            }
            
            if (isActivated)
            {
                RotateWheels();
                if (!recoveringFromCollision)
                    TurnFrontWheels();
            }
        }
        
        private void DelayedUpdate()
        {
            FreezeRangeCheck();
            ActivationRangeCheck();
        }

        public void SetLaneDistance(float laneDistance)
        {
            currentLaneDistance = laneDistance;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!GameObjectLayers.TrafficCarCollisionLayers.ContainsLayer(collision.gameObject.layer))
                return;

            GlobalLoggers.TrafficLogger.Log($"{gameObject.name} collided with {collision.gameObject.name}");

            if (collisions.Count == 0)
            {
                OnCollisionStart();
            }
            
            collisions.Add(collision);

            if (collision.rigidbody.Equals(PlayerCarManager.Instance.CurrentCar.Rigidbody))
            {
                GlobalLoggers.TrafficLogger.Log($"Player hit {gameObject.name} at {collision.impulse.magnitude}m/s");
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if (!collisions.Contains(collision))
                return;
            
            collisions.Remove(collision);

            if (collisions.Count == 0)
            {
                OnCollisionEnd();
            }
        }
        
        private void OnCollisionStart()
        {
            inCollision = true;
            OnStopMoving();
        }
        
        private void OnCollisionEnd()
        {
            inCollision = false;
        }
        
        private void CollisionFreezeCheck()
        {
            if (inCollision)
            {
                timeSinceCollision = 0;
                return;
            }

            timeSinceCollision += Time.deltaTime;
        }

        private void FreezeRangeCheck()
        {
            bool shouldBeFrozen = !ChunkManager.Instance.CanPlayerAccessChunk(currentChunk);
            if (!isFrozen && shouldBeFrozen)
                OnFreeze();

            if (isFrozen && !shouldBeFrozen)
                OnUnfreeze();
        }

        private void OnFreeze()
        {
            isFrozen = true;
            
            rigidbody.velocity = Vector3.zero;
        }

        private void OnUnfreeze()
        {
            isFrozen = false;
        }
        
        private void ActivationRangeCheck()
        {
            if (isFrozen)
            {
                if (isActivated)
                    OnDeactivate();
                return;
            }
            
            float carActivationRangeSqr = carActivationRange * carActivationRange;
            float distanceToPlayerSqr = Vector3.SqrMagnitude(PlayerCarManager.Instance.CurrentCar.transform.position - transform.position);
            if (!isActivated && distanceToPlayerSqr < carActivationRangeSqr)
                OnActivate();
            else if (isActivated && distanceToPlayerSqr > carActivationRangeSqr)
                OnDeactivate();
        }

        private void OnActivate()
        {
            isActivated = true;

            rigidbody.useGravity = true;
            
            foreach (Collider carCollider in GetComponents<Collider>())
            {
                carCollider.enabled = true;
            }
        }

        private void OnDeactivate()
        {
            isActivated = false;
            
            rigidbody.useGravity = false;

            foreach (Collider carCollider in GetComponents<Collider>())
            {
                carCollider.enabled = false;
            }
        }

        private void OnStartMoving()
        {
            isMoving = true;
            OnStartAccelerating();
        }

        private void OnStopMoving()
        {
            isMoving = false;
            speed = 0;
            OnStopAccelerating();
            onStopDecelerating();
        }
        
        private void OnStartAccelerating()
        {
            isAccelerating = true;
            timeSinceAccelerating = 0;
        }

        private void OnAccelerate()
        {
            timeSinceAccelerating += Time.deltaTime;
        }

        /// <summary>
        /// Called when at max speed.
        /// </summary>
        private void OnStopAccelerating()
        {
            isAccelerating = false;
            timeSinceAccelerating = 0;
        }
        
        private void OnStartDecelerating()
        {
            isDecelerating = true;
            timeSinceDecelerating = 0;
        }

        private void OnDecelerate()
        {
            timeSinceDecelerating += Time.deltaTime;
        }
        
        private void onStopDecelerating()
        {
            isDecelerating = false;
            timeSinceDecelerating = 0;
        }
        
        private void Move()
        {
            var targetPos = GetTargetPosition();
            if (targetPos == null)
            {
                Despawn();
                return;
            }

            if (!isMoving)
                OnStartMoving();
            
            if (isAccelerating && timeSinceAccelerating < accelerationDuration)
                OnAccelerate();

            if (isDecelerating && timeSinceDecelerating < decelerationDuration)
                OnDecelerate();
            
            float accelerationFactor = isAccelerating ? Mathf.Clamp01(timeSinceAccelerating / accelerationDuration) : 1;
            float decelerationFactor = isDecelerating ? Mathf.Clamp01(timeSinceDecelerating / decelerationDuration) : 1;
            speed = maxSpeed * (accelerationFactor * decelerationFactor);
            
            var (targetPosition, targetRotation) = targetPos.Value;

            Vector3 directionToTarget = targetPosition - transform.position;
            Quaternion targetRotationFinal = Quaternion.LookRotation(directionToTarget); //face towards the target position
            
            Vector3 targetVelocity = transform.forward * speed; //car always moves forward
            rigidbody.velocity = targetVelocity;

            float speedFactor = Mathf.Clamp01(speed / maxSpeed);
            float finalTurnSpeed = turnSpeed * speedFactor;
            rigidbody.MoveRotation(Quaternion.Slerp(rigidbody.rotation, targetRotationFinal, finalTurnSpeed * Time.deltaTime));
        }
        
        private void RotateWheels()
        {
            foreach (Transform wheel in wheels)
            {
                wheel.Rotate(Vector3.up, wheelRotateSpeed * speed * Time.deltaTime, Space.Self);
            }
        }
        
        private void TurnFrontWheels()
        {
            const int maxTurningDistance = 6; //the number of spline samples
            
            float speedFactor = Mathf.Clamp01(speed / maxSpeed);

            int distanceAhead = Mathf.RoundToInt(Mathf.Lerp(movementTargetDistance, maxTurningDistance, speedFactor));
            
            SplineSample? desiredSplineSample = GetSplineSampleAhead(distanceAhead);
            if (desiredSplineSample == null)
                return; //no more chunks
            
            var (position, rotation) = currentChunk.TrafficManager.GetLanePosition(desiredSplineSample.Value, currentLaneDistance);
            Vector3 directionAhead = (position - transform.position).normalized;
            
            foreach (Transform wheel in frontWheels)
            {
                const float wheelTurnSpeed = 1f;
                Quaternion targetRotation = Quaternion.LookRotation(directionAhead, wheel.transform.up);
                wheel.rotation = Quaternion.Slerp(wheel.rotation, targetRotation, Time.deltaTime * wheelTurnSpeed);
            }
        }

        /// <summary>
        /// Get the next desired position and rotation relative to the sample on the next chunk's spline.
        /// </summary>
        /// <returns>The spline sample's position and rotation, or null if no more loaded chunks in the desired direction.</returns>
        private (Vector3, Quaternion)? GetTargetPosition()
        {
            if (currentChunk == null)
                return null;
            
            if (currentChunk.TrafficManager == null)
            {
                Debug.LogWarning($"A traffic car is on the chunk {currentChunk.gameObject.name}, but it doesn't have a traffic manager.");
                return null;
            }

            SplineSample? desiredSplineSample = GetSplineSampleAhead(movementTargetDistance);
            if (desiredSplineSample == null)
                return null; //no more chunks loaded
            
            var (position, rotation) = currentChunk.TrafficManager.GetLanePosition(desiredSplineSample.Value, currentLaneDistance);

            return (position, rotation);
        }
        
        /// <summary>
        /// Gets the spline sample that is sampleDistance away from the closest sample.
        /// </summary>
        /// <param name="sampleDistance">How many samples away should be retrieved.</param>
        private SplineSample? GetSplineSampleAhead(int sampleDistance)
        {
            if (currentChunk.TrafficManager == null)
                return null; //no traffic manager
            
            int closestSplineIndex = currentChunk.GetClosestSampleIndexOnSpline(transform.position, true).Item1;
            int desiredSplineSampleIndex = faceForward ? closestSplineIndex + sampleDistance : closestSplineIndex - sampleDistance;

            int chunkIndex = ChunkManager.Instance.GetMapIndexOfLoadedChunk(currentChunk);
            while (desiredSplineSampleIndex >= currentChunk.SplineSamples.Length || desiredSplineSampleIndex < 0) //has reached end of chunk
            {
                //get the next chunk
                chunkIndex = faceForward ? chunkIndex + 1 : chunkIndex - 1;

                if (!ChunkManager.Instance.IsChunkWithinLoadRadius(chunkIndex))
                {
                    //no more loaded chunks
                    return null;
                }

                Chunk newChunk = ChunkManager.Instance.GetLoadedChunkByMapIndex(chunkIndex);
                currentChunk = newChunk;
                if (currentChunk.TrafficManager == null)
                    return null; //no traffic manager
                
                closestSplineIndex = newChunk.GetClosestSampleIndexOnSpline(transform.position, true).Item1;
                desiredSplineSampleIndex = faceForward ? closestSplineIndex + 1 : closestSplineIndex - 1;
            }

            return currentChunk.SplineSamples[desiredSplineSampleIndex];
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
            OnStopMoving();
            gameObject.Pool();
        }

    }
}
