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
    public class TrafficCar : MonoBehaviour
    {
        
        private const float timeBetweenActivationChecks = 1;
        private const float carActivationRange = 100;
        private const float collisionRecoverDuration = 5;
        private const float blockedPathDetectorDistance = 20;
        private const float desiredDistanceToBlockage = 5;
        
        [SerializeField] private Transform[] frontWheels;
        [SerializeField] private Transform[] wheels;
        [Space(5)]
        [SerializeField] private MinMaxFloat movementTargetDistance = new(3, 10);
        [Tooltip("At less than or equal to 'min' km/h, the movementTargetDistance is min.\n" +
                 "At greater than or equal to 'max' km/h, the movementTargetDistance is max.")]
        [SerializeField] private MinMaxFloat movementTargetDistanceSpeedFactors = new(40, 90);
        [Space(5)]
        [SerializeField] private float wheelRotateSpeed = 30;
        [Space(5)]
        [SerializeField] private MinMaxFloat turnSpeed = new(0, 4.5f);
        [Tooltip("At less than or equal to 'min' km/h, the turnSpeed is min.\n" +
                 "At greater than or equal to 'max' km/h, the turnSpeed is max.")]
        [SerializeField] private MinMaxFloat turnSpeedFactors = new(0, 90);
        [Space(5)]
        [Tooltip("The time (in seconds) it takes to go from 0 to 60.")]
        [SerializeField] private float timeToAccelerateTo60 = 10;
        [Tooltip("The time (in seconds) it takes to go from 60 to 0.")]
        [SerializeField] private float timeToDecelerateFrom60 = 5;
        [Tooltip("The time (in seconds) it takes to go from 60 to 0 when there's an obstacle ahead.")]
        [SerializeField] private float emergencyBrakeTimeFrom60 = 2.5f;

        [Header("Debugging")]
        [SerializeField] private bool debug;
        [SerializeField, ReadOnly] private bool isInitialised;
        [SerializeField, ReadOnly] private bool isFrozen;
        [SerializeField, ReadOnly] private bool isActivated = true;
        [SerializeField, ReadOnly] private Chunk currentChunk;
        [Space(5)]
        [SerializeField, ReadOnly] private bool isMoving;
        [SerializeField, ReadOnly] private float speed;
        [SerializeField, ReadOnly] private float desiredSpeed;
        [SerializeField, ReadOnly] private bool isAccelerating;
        [SerializeField, ReadOnly] private float timeSinceAccelerating;
        [SerializeField, ReadOnly] private bool isDecelerating;
        [SerializeField, ReadOnly]  private float timeSinceDecelerating;
        [SerializeField, ReadOnly] private bool inCollision;
        [SerializeField, ReadOnly] private bool isPathBlocked;
        [SerializeField, ReadOnly] private float distanceToBlockage;

        private readonly Cooldown activationCheckCooldown = new(timeBetweenActivationChecks);
        private readonly List<Collision> collisions = new();
        private float timeOfLastCollision = -Mathf.Infinity;
        private float timeSinceLastDelayedUpdate;
        private readonly RaycastHit[] blockingObjects = new RaycastHit[10];

        private float timeSinceCollision => Time.realtimeSinceStartup - timeOfLastCollision;
        private bool recoveringFromCollision => timeSinceCollision < collisionRecoverDuration;
        private bool faceForward => currentChunk.TrafficManager.GetLaneDirection(CurrentLaneDistance) == ChunkTrafficManager.LaneDirection.FORWARD;
        private Rigidbody rigidbody => GetComponent<Rigidbody>();
        
        public float CurrentLaneDistance { get; private set; }

        public void Initialise(Chunk currentChunk)
        {
            isInitialised = true;
            this.currentChunk = currentChunk;
            
            currentChunk.onBecomeAccessible += OnChunkBecomeAccessible;
            currentChunk.onBecomeInaccessible += OnChunkBecomeInaccessible;
            
            TrafficCarSpawner.TrackCar(this);
            
            //spawn at max speed
            SetMaxSpeed();
            
            gameObject.layer = (int)LayersAndTags.Layer.TrafficCar;
            
            ActivationRangeCheck();
        }

        private void OnDisable()
        {
            if (!isInitialised)
                return; //didn't get a chance to initialise
            
            TrafficCarSpawner.UntrackCar(this);
            
            currentChunk.onBecomeAccessible -= OnChunkBecomeAccessible;
            currentChunk.onBecomeInaccessible -= OnChunkBecomeInaccessible;
        }

        private void FixedUpdate()
        {
            if (!isInitialised)
                return;
            
            if (currentChunk == null)
            {
                //current chunk may have despawned
                Despawn();
                return;
            }
            
            if (!isFrozen && !recoveringFromCollision)
            {
                Move();
            }

            if (activationCheckCooldown.IsReady)
                ActivationRangeCheck();
            
            if (isActivated)
            {
                RotateWheels();
                if (!recoveringFromCollision && speed > 0.01f)
                    TurnFrontWheels();
            }
        }

        public void SetLaneDistance(float laneDistance)
        {
            CurrentLaneDistance = laneDistance;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!LayersAndTags.TrafficCarCollisionLayers.ContainsLayer(collision.gameObject.layer))
                return;

            GlobalLoggers.TrafficLogger.Log($"{gameObject.name} collided with {collision.gameObject.name}");

            if (collisions.Count == 0)
            {
                OnCollisionStart();
            }

            timeOfLastCollision = Time.realtimeSinceStartup; //reset collision time
            
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

        private void OnChunkBecomeInaccessible()
        {
            Freeze();
            ActivationRangeCheck();
        }

        private void OnChunkBecomeAccessible()
        {
            Unfreeze();
            ActivationRangeCheck();
        }
        
        private void Freeze()
        {
            isFrozen = true;
            
            rigidbody.velocity = Vector3.zero;
        }

        private void Unfreeze()
        {
            isFrozen = false;
        }
        
        private void ActivationRangeCheck()
        {
            activationCheckCooldown.Reset();
            
#if UNITY_EDITOR
            if (debug)
            {
                if (!isActivated)
                    Activate();
                return;
            }
#endif
            
            if (isFrozen)
            {
                if (isActivated)
                    Deactivate();
                return;
            }
            
            float carActivationRangeSqr = carActivationRange * carActivationRange;
            float distanceToPlayerSqr = Vector3.SqrMagnitude(PlayerCarManager.Instance.CurrentCar.transform.position - transform.position);
            if (!isActivated && distanceToPlayerSqr < carActivationRangeSqr)
                Activate();
            else if (isActivated && distanceToPlayerSqr > carActivationRangeSqr)
                Deactivate();
        }

        private void Activate()
        {
            isActivated = true;

            rigidbody.useGravity = true;
            
            foreach (Collider carCollider in GetComponents<Collider>())
            {
                carCollider.enabled = true;
            }
        }

        private void Deactivate()
        {
            isActivated = false;
            
            rigidbody.useGravity = false;

            foreach (Collider carCollider in GetComponents<Collider>())
            {
                carCollider.enabled = false;
            }
        }

        /// <summary>
        /// Sets the car's speed to max speed, and stops accelerating or decelerating.
        /// </summary>
        private void SetMaxSpeed()
        {
            float newDesiredSpeed = currentChunk.TrafficManager.SpeedLimitKmh;
            desiredSpeed = newDesiredSpeed;
            speed = newDesiredSpeed;
        }

        private void OnStartMoving()
        {
            isMoving = true;
            rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            if (debug) GlobalLoggers.TrafficLogger.Log("Started moving");
        }

        private void OnStopMoving()
        {
            isMoving = false;
            speed = 0;
            desiredSpeed = 0;
            OnStopAccelerating();
            OnStopDecelerating();
            rigidbody.constraints = RigidbodyConstraints.None;

            if (debug) GlobalLoggers.TrafficLogger.Log("Stopped moving");
        }
        
        private void OnStartAccelerating()
        {
            isAccelerating = true;
            timeSinceAccelerating = 0;
            speedBeforeAccelerating = speed;
            
            if (debug) GlobalLoggers.TrafficLogger.Log("Started accelerating");
        }

        private float speedBeforeAccelerating;
        private float speedBeforeDecelerating;

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
            
            if (debug) GlobalLoggers.TrafficLogger.Log("Stopped accelerating");
        }
        
        private void OnStartDecelerating()
        {
            isDecelerating = true;
            timeSinceDecelerating = 0;
            speedBeforeDecelerating = speed;
            
            if (debug) GlobalLoggers.TrafficLogger.Log("Started decelerating");
        }

        private void OnDecelerate()
        {
            timeSinceDecelerating += Time.deltaTime;
        }
        
        private void OnStopDecelerating()
        {
            isDecelerating = false;
            timeSinceDecelerating = 0;
            
            if (debug) GlobalLoggers.TrafficLogger.Log("Stopped decelerating");
        }

        private void OnChangeDesiredSpeed(float newSpeed)
        {
            float previousSpeed = desiredSpeed;
            
            desiredSpeed = newSpeed;
            
            if (debug) GlobalLoggers.TrafficLogger.Log($"Changed desired speed to {desiredSpeed}");

            if (newSpeed > previousSpeed)
            {
                OnStartAccelerating();
                if (isDecelerating)
                    OnStopDecelerating();
            }
            else if (newSpeed < previousSpeed)
            {
                OnStartDecelerating();
                if (isAccelerating)
                    OnStopAccelerating();
            }
        }

        private bool CheckIfPathIsBlocked()
        {
            if (!isActivated)
                return false;

            BoxCollider carCollider = GetComponent<BoxCollider>();
            Vector3 boxCastHalfExtents = carCollider.size / 2f;
            int hits = Physics.BoxCastNonAlloc(transform.TransformPoint(carCollider.center.OffsetZ(carCollider.size.z)), boxCastHalfExtents, transform.forward, blockingObjects, transform.rotation, blockedPathDetectorDistance, LayersAndTags.TrafficCarCollisionLayers);

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
        
        private void Move()
        {
            var targetPos = GetTargetPosition();
            if (targetPos == null)
            {
                Despawn();
                return;
            }

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
                float newDesiredSpeed = currentChunk.TrafficManager.SpeedLimitKmh;
                if (!newDesiredSpeed.Approximately(desiredSpeed))
                {
                    OnChangeDesiredSpeed(newDesiredSpeed);
                }
            }

            float accelerationTime = ((desiredSpeed - speedBeforeAccelerating)/60f) * timeToAccelerateTo60;
            float decelerationTime = ((speedBeforeDecelerating - desiredSpeed)/60f) * (isPathBlocked ? emergencyBrakeTimeFrom60 : timeToDecelerateFrom60);
            
            if (isAccelerating)
            {
                if (timeSinceAccelerating >= accelerationTime)
                    OnStopAccelerating();
                else
                    OnAccelerate();
            }

            if (isDecelerating)
            {
                if (timeSinceDecelerating >= decelerationTime)
                    OnStopDecelerating();
                else
                    OnDecelerate();
            }

            if (isAccelerating)
            {
                float difference = desiredSpeed - speedBeforeAccelerating;
                speed = speedBeforeAccelerating + (difference * Mathf.Clamp01(timeSinceAccelerating / accelerationTime));
            } else if (isDecelerating)
            {
                float difference = speedBeforeDecelerating - desiredSpeed;
                speed = speedBeforeDecelerating - (difference * Mathf.Clamp01(timeSinceDecelerating / decelerationTime));
            }
            else
            {
                speed = desiredSpeed;
            }

            if (speed.Approximately(0))
            {
                if (isMoving)
                    OnStopMoving();
                return;
            }

            if (!isMoving)
                OnStartMoving();

            var (newChunk, targetPosition, targetRotation) = targetPos.Value;

            currentChunk = newChunk;
            
            Vector3 directionToTarget = targetPosition - transform.position;
            Quaternion targetRotationFinal = Quaternion.LookRotation(directionToTarget); //face towards the target position
            
            rigidbody.MoveRotation(Quaternion.Slerp(rigidbody.rotation, targetRotationFinal, GetTurnSpeed(speed) * Time.deltaTime));

            Vector3 targetVelocity = transform.forward * SpeedUtils.FromKmh(speed); //car always moves forward
            rigidbody.velocity = targetVelocity;
            Debug.DrawLine(transform.position + rigidbody.velocity * 5, targetPosition, Color.green);
            Debug.DrawLine(transform.position, targetPosition, Color.yellow);
        }
        
        private void RotateWheels()
        {
            foreach (Transform wheel in wheels)
            {
                wheel.Rotate(Vector3.up, wheelRotateSpeed * speed * Time.deltaTime, Space.Self);
            }
        }

        private float GetMovementTargetDistance(float speedToCheck)
        {
            speedToCheck = Mathf.Clamp(speedToCheck, movementTargetDistanceSpeedFactors.Min, movementTargetDistanceSpeedFactors.Max);
            float percentage = (speedToCheck - movementTargetDistanceSpeedFactors.Min) / (movementTargetDistanceSpeedFactors.Max - movementTargetDistanceSpeedFactors.Min);
            float resultDistance = Mathf.Lerp(movementTargetDistance.Min, movementTargetDistance.Max, percentage);
            return resultDistance;
        }
        
        private float GetTurnSpeed(float speedToCheck)
        {
            speedToCheck = Mathf.Clamp(speedToCheck, turnSpeedFactors.Min, turnSpeedFactors.Max);
            float percentage = (speedToCheck - turnSpeedFactors.Min) / (turnSpeedFactors.Max - turnSpeedFactors.Min);
            float resultSpeed = Mathf.Lerp(turnSpeed.Min, turnSpeed.Max, percentage);
            return resultSpeed;
        }
        
        private void TurnFrontWheels()
        {
            (SplineSample, Chunk)? splineSampleAhead = GetSplineSampleAhead(GetMovementTargetDistance(speed));
            if (splineSampleAhead == null)
                return; //no more chunks
            
            var (position, rotation) = currentChunk.TrafficManager.GetLanePosition(splineSampleAhead.Value.Item1, CurrentLaneDistance);
            Vector3 directionAhead = (position - transform.position).normalized;
            
            Debug.DrawLine(transform.position, position, Color.blue);

            foreach (Transform wheel in frontWheels)
            {
                float speedFactor = Mathf.Clamp01(speed / desiredSpeed);
                const float minWheelTurnSpeed = 0f;
                const float maxWheelTurnSpeed = 1f;
                float wheelTurnSpeed = Mathf.Lerp(minWheelTurnSpeed, maxWheelTurnSpeed, speedFactor);
                
                Quaternion targetRotation = Quaternion.LookRotation(directionAhead, wheel.transform.up);
                wheel.rotation = Quaternion.Slerp(wheel.rotation, targetRotation, Time.deltaTime * wheelTurnSpeed);
            }
        }

        /// <summary>
        /// Get the next desired position and rotation relative to the sample on the next chunk's spline.
        /// </summary>
        /// <returns>The spline sample's position and rotation, or null if no more loaded chunks in the desired direction.</returns>
        private (Chunk, Vector3, Quaternion)? GetTargetPosition()
        {
            if (currentChunk == null)
                return null;
            
            if (currentChunk.TrafficManager == null)
            {
                Debug.LogWarning($"A traffic car is on the chunk {currentChunk.gameObject.name}, but it doesn't have a traffic manager.");
                return null;
            }

            (SplineSample, Chunk)? splineSampleAhead = GetSplineSampleAhead(GetMovementTargetDistance(speed));
            if (splineSampleAhead == null)
                return null; //no more chunks loaded
            
            var (position, rotation) = currentChunk.TrafficManager.GetLanePosition(splineSampleAhead.Value.Item1, CurrentLaneDistance);

            return (splineSampleAhead.Value.Item2, position, rotation);
        }

        /// <summary>
        /// Gets the spline sample that is 'distance' metres away from the closest sample.
        /// </summary>
        private (SplineSample, Chunk)? GetSplineSampleAhead(float desiredDistance)
        {
            if (currentChunk.TrafficManager == null)
                return null; //no traffic manager

            float desiredDistanceSqr = desiredDistance * desiredDistance;

            Chunk chunkToUse = currentChunk;
            int chunkIndex = ChunkManager.Instance.GetMapIndexOfLoadedChunk(chunkToUse);
            
            bool isChunkLoaded = chunkIndex >= 0;
            if (!isChunkLoaded)
                return null; //current chunk isn't loaded
            
            //get the closest sample, then get the next, and next, until it is X distance away from the closest
            int closestSplineIndex = currentChunk.GetClosestSampleIndexOnSpline(transform.position).Item1;
            SplineSample closestSample = currentChunk.SplineSamples[closestSplineIndex];

            SplineSample? previousSample = null;
            float previousDistanceOffset = 0;
            int offset = faceForward ? 1 : -1;
            while (true)
            {
                int sampleIndex = closestSplineIndex + offset;
                
                //check if it goes past the current chunk
                if (sampleIndex >= chunkToUse.SplineSamples.Length || sampleIndex < 0)
                {
                    //get the next chunk
                    chunkIndex = faceForward ? chunkIndex + 1 : chunkIndex - 1;
                    
                    LoadedChunkData? loadedChunkData = ChunkManager.Instance.GetLoadedChunkDataByMapIndex(chunkIndex);
                    if (loadedChunkData == null)
                    {
                        //no more loaded chunks
                        return null;
                    }
                    
                    Chunk newChunk = loadedChunkData.Value.Chunk;
                    chunkToUse = newChunk;
                    if (newChunk.TrafficManager == null)
                        return null; //no traffic manager

                    //reset the values
                    previousSample = null;
                    closestSplineIndex = newChunk.GetClosestSampleIndexOnSpline(transform.position).Item1;
                    closestSample = newChunk.SplineSamples[closestSplineIndex];
                    offset = faceForward ? 1 : -1;
                    continue;
                }
                
                SplineSample sample = chunkToUse.SplineSamples[closestSplineIndex + offset];
                float distanceToSampleSqr = Vector3.SqrMagnitude(sample.position - closestSample.position);
                float distanceOffset = Mathf.Abs(desiredDistanceSqr - distanceToSampleSqr);
                
                bool isFurtherAway = previousSample != null && distanceOffset > previousDistanceOffset;
                if (isFurtherAway)
                    return (previousSample.Value, chunkToUse);
                
                previousDistanceOffset = distanceOffset;
                previousSample = sample;
                
                offset = faceForward ? offset + 1 : offset - 1;
            }
        }
        
        private void Despawn()
        {
            OnStopMoving();
            gameObject.Pool();
            GlobalLoggers.TrafficLogger.Log("Despawned");
        }

    }
}
