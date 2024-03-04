using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Dreamteck.Splines;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [RequireComponent(typeof(Rigidbody))]
    public class AICar : MonoBehaviour
    {

        [SerializeField] private Transform[] frontWheelMeshes;
        [SerializeField] private Transform[] rearWheelMeshes;
        [SerializeField] private WheelCollider[] frontWheelColliders;
        [SerializeField] private WheelCollider[] rearWheelColliders;

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

        [Header("Braking")]
        [Tooltip("Does the car slow down and come to a stop when in a collision? Or can it keep accelerating?")]
        [SerializeField] private bool brakeInCollision = true;
        [Tooltip("When the angle is supplied (x axis), the y axis represents the desired speed.")]
        [SerializeField] private AnimationCurve cornerBrakingCurve;
        [Tooltip("The y axis represents the amount of brake torque when x (the amount to brake) is a certain value. When the amount to brake is more, there should be more brake torque.")]
        [SerializeField] private AnimationCurve brakeTorqueCurve;
        [Space(5)]
        [SerializeField, ReadOnly] private bool isBraking;
        [SerializeField, ReadOnly] private float cornerSpeed;
        [SerializeField, ReadOnly] private float speedToBrakeTo;
        [SerializeField, ReadOnly] private float currentBrakeForce;

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
        [SerializeField, ReadOnly] private bool inCollisionWithPlayer;

        private float timeOfLastCollision = -Mathf.Infinity;

        protected Rigidbody rigidBody => GetComponent<Rigidbody>();
        private float timeSinceCollision => Time.time - timeOfLastCollision;
        private bool recoveringFromCollision => inCollisionWithPlayer || timeSinceCollision < collisionRecoverDuration;
        public float DesiredSpeed => desiredSpeed;
        public float Speed => speed;
        public float CurrentLaneDistance => currentLaneDistance;
        private bool faceForward => useRacingLine || currentChunkCached.TrafficManager.GetLaneDirection(CurrentLaneDistance) == ChunkTrafficManager.LaneDirection.FORWARD;
        
        private int lastFrameChunkWasCached = -1;

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
            if (!LayersAndTags.AICarCollisionLayers.ContainsLayer(collision.gameObject.layer))
                return;

            GlobalLoggers.AICarLogger.Log($"{gameObject.name} collided with {collision.gameObject.name}");

            timeOfLastCollision = Time.time; //reset collision time

            bool collisionWithPlayer = collision.rigidbody != null && collision.rigidbody == WarehouseManager.Instance.CurrentCar.Rigidbody;
            if (collisionWithPlayer)
            {
                inCollisionWithPlayer = true;
                GlobalLoggers.AICarLogger.Log($"Player hit {gameObject.name} at {collision.impulse.magnitude}m/s");
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            bool collisionWithPlayer = collision.rigidbody != null && collision.rigidbody == WarehouseManager.Instance.CurrentCar.Rigidbody;
            if (collisionWithPlayer)
            {
                timeOfLastCollision = Time.time; //reset collision time
                inCollisionWithPlayer = false;
            }
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

        protected void ForceSetSpeed(float speed)
        {
            this.speed = speed;
        }
        

        protected virtual void PreMoveChecks()
        {
            
        }

        
        //TODO: move
        private (Chunk, Vector3, Quaternion)? targetPos;
        
        
        private void Move()
        {
            PreMoveChecks();
            
            targetPos = GetPositionAhead(GetMovementTargetDistance(Speed));
            if (targetPos == null)
            {
                Despawn();
                return;
            }

            var (newChunk, targetPosition, targetRotation) = targetPos.Value;
            
            speed = SpeedUtils.ToKmh(rigidBody.velocity.magnitude);
            
            CalculateSteerAngle();
            CheckForCorner();
            CheckToBrake();
            CheckToAccelerate();

            rigidBody.drag = recoveringFromCollision ? 1 : 0;

            ApplySteering();

            UpdateWheelMeshes();
            
            //debug directions:
            Debug.DrawLine(transform.position + rigidBody.velocity * 5, targetPosition, Color.green);
            Debug.DrawLine(transform.position, targetPosition, Color.yellow);
        }

        private void CheckToAccelerate()
        {
            bool wasAccelerating = isAccelerating;

            if (isBraking || (brakeInCollision && recoveringFromCollision))
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
            if (brakeInCollision && recoveringFromCollision)
                return; //don't update steering while in collision

            Vector3 targetPosition = targetPos.Value.Item2;
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

        private void CheckToBrake()
        {
            bool wasBraking = isBraking;
            isBraking = false; //reset for check

            const float speedingLeeway = 10; //the amount the player can speed past the desired speed before needing to brake
            float speedToObey = Mathf.Min(desiredSpeed + speedingLeeway, cornerSpeed);
            
            if (speed > speedToObey)
            {
                speedToBrakeTo = speedToObey;
                isBraking = true;
            }
            
            if (wasBraking && !isBraking)
                OnStopBraking();
            
            if (!wasBraking && isBraking)
                OnStartBraking();
            
            if (isBraking)
                OnBrake();
            
            //TODO: if obstacle blocking, check if it is a rigidbody and set the speedToBrakeTo to the speed
            // - else set speedToBrakeTo to 0 if it's stationary
        }

        private void OnStartBraking()
        {
            
        }
        
        private void OnBrake()
        {
            ApplyBrakeForce();
        }

        private void OnStopBraking()
        {
            
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
            
            cornerSpeed = cornerBrakingCurve.Evaluate(angleAheadToBrakeTo);
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
                steerPivot.Rotate(steerPivot.up, visualSteerAngle);
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
