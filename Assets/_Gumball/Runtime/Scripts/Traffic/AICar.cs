using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Dreamteck.Splines;
#if UNITY_EDITOR
using Gumball.Editor;
#endif
using MyBox;
using UnityEngine;

namespace Gumball
{
    [RequireComponent(typeof(Rigidbody))]
    public class AICar : MonoBehaviour
    {
        
        private enum ObstacleAvoidanceOffset
        {
            NONE,
            LEFT,
            RIGHT,
            LEFT_SMALLER,
            RIGHT_SMALLER
        }

        [SerializeField] private Transform[] frontWheelMeshes;
        [SerializeField] private Transform[] rearWheelMeshes;
        [SerializeField] private WheelCollider[] frontWheelColliders;
        [SerializeField] private WheelCollider[] rearWheelColliders;

        [Header("Max speed")]
        [Tooltip("Does the car obey the current chunks speed limit?")]
        [SerializeField] private bool obeySpeedLimit = true;
        [SerializeField, ConditionalField(nameof(obeySpeedLimit), true)] private float maxSpeed = 200;
        
        [Header("Lanes")]
        [Tooltip("Does the car try to take the optimal race line, or does it stay in a single (random) lane?")]
        [SerializeField] private bool useRacingLine;
        [ConditionalField(nameof(useRacingLine)), SerializeField, ReadOnly] private float currentLaneDistance;

        [Header("Acceleration")]
        [SerializeField] private float motorTorque = 500;
        [Tooltip("The duration to go from 0 motor torque to the max motor torque.")]
        [SerializeField] private float accelerationDurationToMaxTorque = 1f;
        [SerializeField] private AnimationCurve accelerationEase;
        [SerializeField, ReadOnly] private bool isAccelerating;
        private Tween[] currentMotorTorqueTweens;
        
        [Header("Steering")]
        [SerializeField] private MinMaxFloat movementTargetDistance = new(5, 10);
        [Tooltip("At less than or equal to 'min' km/h, the movementTargetDistance is min.\n" +
                 "At greater than or equal to 'max' km/h, the movementTargetDistance is max.")]
        [SerializeField] private MinMaxFloat movementTargetDistanceSpeedFactors = new(20, 90);
        [Tooltip("The speed that the wheel mesh is interpolated to the desired steer angle. This makes it not snappy.")]
        [SerializeField] private float visualSteerSpeed = 5;
        [SerializeField] private float maxSteerAngle = 65;
        [Space(5)]
        [SerializeField, ReadOnly] private float desiredSteerAngle;
        [SerializeField, ReadOnly] private float visualSteerAngle;
        
        [Header("Collisions")]
        [SerializeField] private float collisionRecoverDuration = 1;
        [Tooltip("Does the car disable its braking, acceleration and steering while in a collision? Or can it keep driving?")]
        [SerializeField] private bool disableMovementInCollision = true;
        private float timeOfLastCollision = -Mathf.Infinity;
        [Space(5)]
        [SerializeField, ReadOnly] private bool inCollision;
        
        [Header("Obstacle detection")]
        [SerializeField] private LayerMask obstacleLayers;
        [SerializeField] private bool brakeForObstacles = true;
        [ConditionalField(nameof(brakeForObstacles)), SerializeField, ReadOnly] private float obstacleSpeed = Mathf.Infinity;
        
        [Header("Obstacle avoidance")]
        [SerializeField] private bool useObstacleAvoidance;
        [ConditionalField(nameof(useObstacleAvoidance)), SerializeField] private Vector3 obstacleAvoidanceDetectorSize = new(0.5f,1,0.5f);
        [ConditionalField(nameof(useObstacleAvoidance)), SerializeField] private float blockedPathDetectorDistance = 15;
        [ConditionalField(nameof(useObstacleAvoidance)), SerializeField] private float blockedPathDetectorDistanceSide = 10;
        [ConditionalField(nameof(useObstacleAvoidance)), SerializeField] private float blockedPathDetectorDistanceSideSmall = 5;
        [Space(5)]
        [ConditionalField(nameof(useObstacleAvoidance)), SerializeField, ReadOnly] private ObstacleAvoidanceOffset currentOffset;
        [ConditionalField(nameof(useObstacleAvoidance)), SerializeField, ReadOnly] private bool hasPickedRandomSide;
        [ConditionalField(nameof(useObstacleAvoidance)), SerializeField, ReadOnly] private bool hasPickedRandomSideSmaller;
        
        [Header("Braking")]
        [Tooltip("When the angle is supplied (x axis), the y axis represents the desired speed.")]
        [SerializeField] private AnimationCurve cornerBrakingCurve;
        [Tooltip("The y axis represents the amount of brake torque when x (the amount to brake) is a certain value. When the amount to brake is more, there should be more brake torque.")]
        [SerializeField] private AnimationCurve brakeTorqueCurve;
        [Space(5)]
        [SerializeField, ReadOnly] private bool isBraking;
        [SerializeField, ReadOnly] private float corneringSpeed = Mathf.Infinity;
        [SerializeField, ReadOnly] private float speedToBrakeTo;
        [SerializeField, ReadOnly] private float currentBrakeForce;
        private bool wasBrakingLastFrame;

        [Header("Debugging")]
        [SerializeField] private bool debug;
        [Space(5)]
        [SerializeField, ReadOnly] protected bool isInitialised;
        [SerializeField, ReadOnly] private Transform[] allWheelMeshes;
        [SerializeField, ReadOnly] private WheelCollider[] allWheelColliders;
        [Space(5)]
        [SerializeField, ReadOnly] protected Chunk currentChunkCached;
        [SerializeField, ReadOnly] protected bool isFrozen;
        [Space(5)]
        [SerializeField, ReadOnly] private float speed;
        [SerializeField, ReadOnly] private float desiredSpeed;
        
        private readonly RaycastHit[] blockagesTemp = new RaycastHit[5]; //is used for all blockage checks, not to be used for debugging
        private int lastFrameChunkWasCached = -1;
        private (Chunk, Vector3, Quaternion)? targetPos;
        
        protected Rigidbody rigidBody => GetComponent<Rigidbody>();
        private float timeSinceCollision => Time.time - timeOfLastCollision;
        private bool recoveringFromCollision => inCollision || timeSinceCollision < collisionRecoverDuration;
        private bool faceForward => useRacingLine || currentChunkCached.TrafficManager.GetLaneDirection(CurrentLaneDistance) == ChunkTrafficManager.LaneDirection.FORWARD;
        
        public float DesiredSpeed => desiredSpeed;
        public float Speed => speed;
        public float CurrentLaneDistance => currentLaneDistance;
        
        /// <returns>The chunk the player is on, else null if it can't be found.</returns>
        public Chunk CurrentChunk
        {
            get
            {
                if (lastFrameChunkWasCached != Time.frameCount)
                {
                    lastFrameChunkWasCached = Time.frameCount;

                    Chunk previousChunk = currentChunkCached;
                    
                    //raycast down to terrain
                    currentChunkCached = Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitDown, Mathf.Infinity, LayersAndTags.GetLayerMaskFromLayer(LayersAndTags.Layer.ChunkDetector))
                        ? hitDown.transform.parent.GetComponent<Chunk>()
                        : null;
                    
                    if (currentChunkCached != previousChunk)
                        OnChangeChunk(previousChunk, currentChunkCached);
                }

                return currentChunkCached;
            }
        }
        
        private void OnEnable()
        {
            if (!isInitialised)
                Initialise();
        }

        public virtual void Initialise()
        {
            isInitialised = true;

            gameObject.layer = (int)LayersAndTags.Layer.TrafficCar;
            
            OnChangeChunk(null, CurrentChunk);

            CacheAllWheelMeshes();
            CacheAllWheelColliders();
        }

        public void SetLaneDistance(float laneDistance)
        {
            currentLaneDistance = laneDistance;
        }
        
        private void FixedUpdate()
        {
            if (!isInitialised)
                return;
            
            if (CurrentChunk == null)
            {
                //current chunk may have despawned
                Despawn();
                return;
            }
            
            if (!isFrozen)
            {
                Move();
            }
            
            for (int index = 0; index < rearWheelColliders.Length; index++)
            {
                WheelCollider wheelCollider = rearWheelColliders[index];

                if (debug)
                    Debug.Log($"{wheelCollider.name}: {wheelCollider.motorTorque}");
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            bool hitACar = LayersAndTags.AllCarLayers.ContainsLayer(collision.gameObject.layer);
            if (!hitACar)
                return;

            GlobalLoggers.AICarLogger.Log($"{gameObject.name} collided with {collision.gameObject.name}");

            timeOfLastCollision = Time.time; //reset collision time

            inCollision = true;
        }

        private void OnCollisionExit(Collision collision)
        {
            bool hitACar = LayersAndTags.AllCarLayers.ContainsLayer(collision.gameObject.layer);
            if (!hitACar)
                return;
            
            timeOfLastCollision = Time.time; //reset collision time
            inCollision = false;
        }

        private void OnChunkCachedBecomeInaccessible()
        {
            Freeze();
        }

        private void OnChunkCachedBecomeAccessible()
        {
            Unfreeze();
        }
        
        private void Freeze()
        {
            isFrozen = true;
            
            GetComponent<Rigidbody>().velocity = Vector3.zero;
        }

        private void Unfreeze()
        {
            isFrozen = false;
        }
        
        protected void OnChangeDesiredSpeed(float newSpeed)
        {
            desiredSpeed = newSpeed;
            
            if (debug) GlobalLoggers.AICarLogger.Log($"Changed desired speed to {desiredSpeed}");
        }
        
        private void Move()
        {
            targetPos = GetPositionAhead(GetMovementTargetDistance(Speed));
            if (targetPos == null)
            {
                Despawn();
                return;
            }

            var (_, targetPosition, _) = targetPos.Value;
            
            speed = SpeedUtils.ToKmh(rigidBody.velocity.magnitude);

            if (useObstacleAvoidance)
                TryAvoidObstacles();
            
            UpdateDesiredSpeed();
            CalculateSteerAngle();
            CheckForCorner();
            
            UpdateBrakingValues();
            DoBrakeEvents();
            
            CheckToAccelerate();
            ApplySteering();
            UpdateWheelMeshes();
            
            //debug directions:
            Debug.DrawRay(transform.position, rigidBody.velocity, Color.green);
            
            Vector3 offsetDirection = GetObstacleOffset(currentOffset);
            Debug.DrawLine(transform.position, targetPosition + offsetDirection, Color.yellow);
        }

        private void UpdateDesiredSpeed()
        {
            float newDesiredSpeed = desiredSpeed;
            if (obeySpeedLimit)
            {
                if (CurrentChunk.TrafficManager != null)
                    newDesiredSpeed = CurrentChunk.TrafficManager.SpeedLimitKmh;
            }
            else
            {
                newDesiredSpeed = maxSpeed;
            }
            
            if (!newDesiredSpeed.Approximately(DesiredSpeed))
                OnChangeDesiredSpeed(newDesiredSpeed);
        }
        
        private void CheckToAccelerate()
        {
            bool wasAccelerating = isAccelerating;

            if (isBraking || (disableMovementInCollision && recoveringFromCollision))
                isAccelerating = false;
            else
                isAccelerating = speed < desiredSpeed;

            if (wasAccelerating && !isAccelerating)
                OnStopAccelerating();

            if (!wasAccelerating && isAccelerating)
                OnStartAccelerating();
            
            if (isAccelerating)
                OnAccelerate();
        }
        
        private void CalculateSteerAngle()
        {
            if (disableMovementInCollision && recoveringFromCollision)
                return; //don't update steering while in collision
            
            Vector3 offsetDirection = GetObstacleOffset(currentOffset);
            Vector3 targetPosition = targetPos.Value.Item2 + offsetDirection;
            Vector3 directionToTarget = targetPosition - rigidBody.position;
            desiredSteerAngle = Mathf.Clamp(-Vector2.SignedAngle(rigidBody.velocity.FlattenAsVector2(), directionToTarget.FlattenAsVector2()), -maxSteerAngle, maxSteerAngle);
            
            //set the visual steer angle (same for all front wheels)
            const float minSpeedVisualSteerModifier = 20;
            float speedModifier = Mathf.Clamp01(speed / minSpeedVisualSteerModifier); //adjust for low speed
            visualSteerAngle = Mathf.LerpAngle(visualSteerAngle, desiredSteerAngle, visualSteerSpeed * speedModifier * Time.deltaTime);
        }

        private void ApplyBrakeForce()
        {
            if (!isBraking)
                return;
            
            //the greater distance between speed and speedToBrakeTo, the more brake force should be applied
            float amountToBrake = speed - speedToBrakeTo;
            currentBrakeForce = brakeTorqueCurve.Evaluate(amountToBrake);
            
            //apply brake force to entire car rather than the wheels to prevent lock up
            rigidBody.AddForce(-rigidBody.velocity * currentBrakeForce, ForceMode.Force);
        }
        
        private void UpdateBrakingValues()
        {
            //reset for check
            isBraking = false;
            speedToBrakeTo = Mathf.Infinity;

            if (disableMovementInCollision && inCollision)
                return;
            
            //is speeding over the desired speed? 
            const float speedingLeeway = 10; //the amount the player can speed past the desired speed before needing to brake
            if (speed > desiredSpeed + speedingLeeway)
            {
                speedToBrakeTo = desiredSpeed;
                isBraking = true;
            }

            //brake around corners
            if (speed > corneringSpeed && corneringSpeed < speedToBrakeTo)
            {
                speedToBrakeTo = corneringSpeed;
                isBraking = true;
            }

            if (brakeForObstacles)
            {
                obstacleSpeed = GetSpeedOfObstaclesAhead();
                if (speed > obstacleSpeed && obstacleSpeed < speedToBrakeTo)
                {
                    speedToBrakeTo = obstacleSpeed;
                    isBraking = true;
                }
            }
        }
        
        private void DoBrakeEvents()
        {
            if (wasBrakingLastFrame && !isBraking)
                OnStopBraking();
            
            if (!wasBrakingLastFrame && isBraking)
                OnStartBraking();
            
            if (isBraking)
                OnBrake();

            wasBrakingLastFrame = isBraking;
        }

        private float GetSpeedOfObstaclesAhead()
        {
            var (_, targetPosition, _) = targetPos.Value;
            Vector3 directionToTarget = Vector3.Normalize(targetPosition - transform.position);

            RaycastHit? blockage = GetBlockage(directionToTarget, blockedPathDetectorDistance);
            
            bool isBlocked = blockage != null;
            if (!isBlocked)
                return Mathf.Infinity;
            
            //not moving
            return 0;
        }

        private void TryAvoidObstacles()
        {
            var (_, targetPosition, _) = targetPos.Value;
            Vector3 directionToTarget = Vector3.Normalize(targetPosition - transform.position);

            if (!IsDirectionBlocked(directionToTarget, blockedPathDetectorDistance))
            {
                SetObstacleOffset(ObstacleAvoidanceOffset.NONE);
                return;
            }
            
            //check left and right
            Vector3 directionToLeft = Vector3.Normalize(targetPosition + GetObstacleOffset(ObstacleAvoidanceOffset.LEFT) - transform.position);
            Vector3 directionToRight = Vector3.Normalize(targetPosition + GetObstacleOffset(ObstacleAvoidanceOffset.RIGHT) - transform.position);
                
            bool isLeftBlocked = IsDirectionBlocked(directionToLeft, blockedPathDetectorDistanceSide);
            bool isRightBlocked = IsDirectionBlocked(directionToRight, blockedPathDetectorDistanceSide);

            if (isLeftBlocked && isRightBlocked)
            {
                //both sides are blocked - try smaller directions
                Vector3 directionToLeftSmall = Vector3.Normalize(targetPosition + GetObstacleOffset(ObstacleAvoidanceOffset.LEFT_SMALLER) - transform.position);
                Vector3 directionToRightSmall = Vector3.Normalize(targetPosition + GetObstacleOffset(ObstacleAvoidanceOffset.RIGHT_SMALLER) - transform.position);
                
                bool isLeftBlockedSmall = IsDirectionBlocked(directionToLeftSmall, blockedPathDetectorDistanceSideSmall);
                bool isRightBlockedSmall = IsDirectionBlocked(directionToRightSmall, blockedPathDetectorDistanceSideSmall);

                if (isLeftBlockedSmall && isRightBlockedSmall)
                    return; //all directions blocked - don't change direction

                if (!isLeftBlockedSmall && !isRightBlockedSmall)
                {
                    if (hasPickedRandomSideSmaller)
                        return; //already picked a side
                
                    //pick random side to go to
                    int random = UnityEngine.Random.Range(0, 2);
                    SetObstacleOffset(random == 0 ? ObstacleAvoidanceOffset.LEFT_SMALLER : ObstacleAvoidanceOffset.RIGHT_SMALLER);
                
                    hasPickedRandomSideSmaller = true;
                }
                else if (isRightBlockedSmall && !isLeftBlockedSmall)
                {
                    //offset to the left
                    SetObstacleOffset(ObstacleAvoidanceOffset.LEFT_SMALLER);
                } else if (isLeftBlockedSmall && !isRightBlockedSmall)
                {
                    //offset to the right
                    SetObstacleOffset(ObstacleAvoidanceOffset.RIGHT_SMALLER);
                }
                
                return;
            }
                
            if (!isLeftBlocked && !isRightBlocked)
            {
                if (hasPickedRandomSide)
                    return; //already picked a side
                
                //pick random side to go to
                int random = UnityEngine.Random.Range(0, 2);
                SetObstacleOffset(random == 0 ? ObstacleAvoidanceOffset.LEFT : ObstacleAvoidanceOffset.RIGHT);
                
                hasPickedRandomSide = true;
            }
            else if (isRightBlocked && !isLeftBlocked)
            {
                //offset to the left
                SetObstacleOffset(ObstacleAvoidanceOffset.LEFT);
            } else if (isLeftBlocked && !isRightBlocked)
            {
                //offset to the right
                SetObstacleOffset(ObstacleAvoidanceOffset.RIGHT);
            }
        }

        private Vector3 GetObstacleOffset(ObstacleAvoidanceOffset offset)
        {
            switch (offset)
            {
                case ObstacleAvoidanceOffset.NONE:
                    return Vector3.zero;
                case ObstacleAvoidanceOffset.LEFT:
                case ObstacleAvoidanceOffset.RIGHT:
                {
                    const float offsetAmount = 3;
                    Vector3 offsetDirection = transform.right * offsetAmount;
                    if (offset == ObstacleAvoidanceOffset.LEFT)
                        offsetDirection = -offsetDirection;

                    return offsetDirection;
                }
                case ObstacleAvoidanceOffset.LEFT_SMALLER:
                case ObstacleAvoidanceOffset.RIGHT_SMALLER:
                {
                    const float offsetAmount = 6;
                    Vector3 offsetDirection = transform.right * offsetAmount;
                    if (offset == ObstacleAvoidanceOffset.LEFT_SMALLER)
                        offsetDirection = -offsetDirection;

                    return offsetDirection;
                }
                default:
                    throw new NotImplementedException();
            }
        }
        
        private void SetObstacleOffset(ObstacleAvoidanceOffset offset)
        {
            currentOffset = offset;
            hasPickedRandomSide = false;
            hasPickedRandomSideSmaller = false;
        }

        private bool IsDirectionBlocked(Vector3 direction, float raycastLength)
        {
            return GetBlockage(direction, raycastLength) != null;
        }
        
        /// <summary>
        /// Gets the closest blockage in the given direction.
        /// </summary>
        private RaycastHit? GetBlockage(Vector3 direction, float raycastLength)
        {
            int hits = Physics.BoxCastNonAlloc(transform.position, obstacleAvoidanceDetectorSize, direction, blockagesTemp, transform.rotation, raycastLength, obstacleLayers);
            RaycastHit? actualHit = null;
            
            RaycastHitSorter.SortRaycastHitsByDistance(blockagesTemp, hits);
            
            for (int index = 0; index < hits; index++)
            {
                RaycastHit hit = blockagesTemp[index];
                if (!ReferenceEquals(hit.transform.gameObject, gameObject.transform.gameObject))
                {
                    actualHit = hit;
                    break; //just get the first/closest hit
                }
            }
            
#if UNITY_EDITOR
            BoxCastUtils.DrawBoxCastBox(transform.position, obstacleAvoidanceDetectorSize, transform.rotation, direction, raycastLength, actualHit != null ? Color.magenta : Color.gray);
#endif

            return actualHit;
        }

        private void OnStartBraking()
        {
            rigidBody.drag = 1;
        }
        
        private void OnBrake()
        {
            ApplyBrakeForce();
        }

        private void OnStopBraking()
        {
            rigidBody.drag = 0;
        }
        
        private void CheckForCorner()
        {
            const float visionDistance = 25f;
            var targetPos = GetPositionAhead(visionDistance);
            if (targetPos == null)
                return;
            
            var (chunk, targetPosition, targetRotation) = targetPos.Value;
            Vector3 directionToTarget = targetPosition - transform.position;

            float angleAhead = Vector2.Angle(transform.forward.FlattenAsVector2(), directionToTarget.FlattenAsVector2());
            float angleAheadToBrakeTo = Mathf.Max(Mathf.Abs(desiredSteerAngle), angleAhead);
            
            corneringSpeed = cornerBrakingCurve.Evaluate(angleAheadToBrakeTo);
        }

        private void OnStartAccelerating()
        {
            //initialise the array
            if (currentMotorTorqueTweens == null || currentMotorTorqueTweens.Length == 0)
                currentMotorTorqueTweens = new Tween[rearWheelColliders.Length];

            for (int index = 0; index < rearWheelColliders.Length; index++)
            {
                WheelCollider rearWheel = rearWheelColliders[index];
                
                if (currentMotorTorqueTweens[index] != null)
                    currentMotorTorqueTweens[index]?.Kill();

                float currentTorque = rearWheel.motorTorque;
                float durationPercent = Mathf.Clamp01(1 - (currentTorque / motorTorque));
                float duration = durationPercent * accelerationDurationToMaxTorque;
                
                currentMotorTorqueTweens[index] = DOTween.To(() => rearWheel.motorTorque,
                        x => rearWheel.motorTorque = x, motorTorque, duration).SetEase(accelerationEase);
            }
        }

        private void OnStopAccelerating()
        {
            for (int index = 0; index < rearWheelColliders.Length; index++)
            {
                WheelCollider wheelCollider = rearWheelColliders[index];

                //stop acceleration tweens
                currentMotorTorqueTweens[index]?.Kill();
                
                wheelCollider.motorTorque = 0;
            }
        }
        
        private void OnAccelerate()
        {
            
        }
        
        private void ApplySteering()
        {
            foreach (WheelCollider frontWheel in frontWheelColliders)
            {
                frontWheel.steerAngle = desiredSteerAngle;
            }
        }

        /// <summary>
        /// Update all the wheel meshes to match the wheel colliders.
        /// </summary>
        private void UpdateWheelMeshes()
        {
            //do rear wheels first as the front wheels require their rotation
            for (int count = 0; count < rearWheelMeshes.Length; count++)
            {
                Transform rearWheelMesh = rearWheelMeshes[count];
                WheelCollider rearWheelCollider = rearWheelColliders[count];
                
                rearWheelCollider.GetWorldPose(out Vector3 wheelPosition, out Quaternion wheelRotation);
                rearWheelMesh.position = wheelPosition;
                rearWheelMesh.rotation = wheelRotation;
            }

            for (int count = 0; count < frontWheelMeshes.Length; count++)
            {
                Transform frontWheelMesh = frontWheelMeshes[count];
                WheelCollider frontWheelCollider = frontWheelColliders[count];
                
                frontWheelCollider.GetWorldPose(out Vector3 wheelPosition, out _);
                frontWheelMesh.position = wheelPosition;

                //rotation is the same as the rear wheel, but with interpolated steer speed
                Transform rearWheelRotation = rearWheelMeshes[count];
                frontWheelMesh.rotation = rearWheelRotation.rotation;
                
                //set the steer amount
                Transform steerPivot = frontWheelMesh.parent;
                steerPivot.Rotate(Vector3.up, visualSteerAngle);
            }
        }
        
        protected float GetMovementTargetDistance(float speedToCheck)
        {
            speedToCheck = Mathf.Clamp(speedToCheck, movementTargetDistanceSpeedFactors.Min, movementTargetDistanceSpeedFactors.Max);
            float percentage = (speedToCheck - movementTargetDistanceSpeedFactors.Min) / (movementTargetDistanceSpeedFactors.Max - movementTargetDistanceSpeedFactors.Min);
            float resultDistance = Mathf.Lerp(movementTargetDistance.Min, movementTargetDistance.Max, percentage);
            return resultDistance;
        }
        
        private void Despawn()
        {
            gameObject.Pool();
            GlobalLoggers.AICarLogger.Log($"Despawned at {transform.position}");
        }

        private void CacheAllWheelMeshes()
        {
            int indexCount = 0;
            allWheelMeshes = new Transform[frontWheelMeshes.Length + rearWheelMeshes.Length];
            foreach (Transform wheelMesh in frontWheelMeshes)
            {
                allWheelMeshes[indexCount] = wheelMesh;
                indexCount++;
            }
            foreach (Transform wheelMesh in rearWheelMeshes)
            {
                allWheelMeshes[indexCount] = wheelMesh;
                indexCount++;
            }
        }
        
        private void CacheAllWheelColliders()
        {
            int indexCount = 0;
            allWheelColliders = new WheelCollider[frontWheelColliders.Length + rearWheelColliders.Length];
            foreach (WheelCollider wheelCollider in frontWheelColliders)
            {
                allWheelColliders[indexCount] = wheelCollider;
                indexCount++;
            }
            foreach (WheelCollider wheelCollider in rearWheelColliders)
            {
                allWheelColliders[indexCount] = wheelCollider;
                indexCount++;
            }
        }
        
        
        /// <summary>
        /// Get the next desired position and rotation relative to the sample on the next chunk's spline.
        /// </summary>
        /// <returns>The spline sample's position and rotation, or null if no more loaded chunks in the desired direction.</returns>
        protected (Chunk, Vector3, Quaternion)? GetPositionAhead(float distance)
        {
            if (CurrentChunk == null)
                return null;
            
            if (CurrentChunk.TrafficManager == null)
            {
                Debug.LogWarning($"A traffic car is on the chunk {CurrentChunk.gameObject.name}, but it doesn't have a traffic manager.");
                return null;
            }

            (SplineSample, Chunk)? splineSampleAhead = GetSplineSampleAhead(distance);
            if (splineSampleAhead == null)
                return null; //no more chunks loaded

            if (useRacingLine)
            {
                return (splineSampleAhead.Value.Item2, splineSampleAhead.Value.Item1.position, splineSampleAhead.Value.Item1.rotation);
            }
            else
            {
                var (position, rotation) = CurrentChunk.TrafficManager.GetLanePosition(splineSampleAhead.Value.Item1, CurrentLaneDistance);
                return (splineSampleAhead.Value.Item2, position, rotation);
            }

            return null;
        }

        /// <summary>
        /// Gets the spline sample that is 'distance' metres away from the closest sample.
        /// </summary>
        private (SplineSample, Chunk)? GetSplineSampleAhead(float desiredDistance)
        {
            if (CurrentChunk.TrafficManager == null)
                return null; //no traffic manager

            float desiredDistanceSqr = desiredDistance * desiredDistance;

            Chunk chunkToUse = CurrentChunk;
            int chunkIndex = ChunkManager.Instance.GetMapIndexOfLoadedChunk(chunkToUse);
            
            bool isChunkLoaded = chunkIndex >= 0;
            if (!isChunkLoaded)
                return null; //current chunk isn't loaded

            SampleCollection sampleCollection = useRacingLine ? CurrentChunk.TrafficManager.RacingLine.SampleCollection : CurrentChunk.SplineSampleCollection;
            
            //get the closest sample, then get the next, and next, until it is X distance away from the closest
            int closestSplineIndex = sampleCollection.GetClosestSampleIndexOnSpline(rigidBody.position).Item1;
            SplineSample closestSample = sampleCollection.samples[closestSplineIndex];

            SplineSample? previousSample = null;
            float previousDistanceOffset = 0;
            int offset = faceForward ? 1 : -1;
            while (true)
            {
                int sampleIndex = closestSplineIndex + offset;
                
                //check if it goes past the current chunk
                if (sampleIndex >= sampleCollection.samples.Length || sampleIndex < 0)
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

                    sampleCollection = useRacingLine ? chunkToUse.TrafficManager.RacingLine.SampleCollection : chunkToUse.SplineSampleCollection;
                    
                    //reset the values
                    previousSample = null;
                    closestSplineIndex = sampleCollection.GetClosestSampleIndexOnSpline(rigidBody.position).Item1;
                    closestSample = sampleCollection.samples[closestSplineIndex];
                    offset = faceForward ? 1 : -1;
                    continue;
                }
                
                SplineSample sample = sampleCollection.samples[closestSplineIndex + offset];
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
        
        private void OnChangeChunk(Chunk previous, Chunk current)
        {
            if (previous != null)
            {
                previous.onBecomeAccessible -= OnChunkCachedBecomeAccessible;
                previous.onBecomeInaccessible -= OnChunkCachedBecomeInaccessible;
            }

            if (current != null)
            {
                current.onBecomeAccessible += OnChunkCachedBecomeAccessible;
                current.onBecomeInaccessible += OnChunkCachedBecomeInaccessible;
            }
        }
        
        private void OnDrawGizmos()
        {
            Vector3 carCentreOfMassWorld = rigidBody.transform.TransformPoint(rigidBody.centerOfMass);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(carCentreOfMassWorld, 0.5f);
        }
        
    }
}
