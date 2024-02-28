using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dreamteck.Splines;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [RequireComponent(typeof(Rigidbody))]
    public abstract class AICar : MonoBehaviour
    {

        [SerializeField] private Transform[] frontWheelMeshes;
        [SerializeField] private Transform[] rearWheelMeshes;
        [SerializeField] private WheelCollider[] frontWheelColliders;
        [SerializeField] private WheelCollider[] rearWheelColliders;

        [Header("Acceleration")]
        [SerializeField] private float motorTorque = 500;
        
        [Header("Steering")]
        [SerializeField] private MinMaxFloat movementTargetDistance = new(3, 10);
        [Tooltip("At less than or equal to 'min' km/h, the movementTargetDistance is min.\n" +
                 "At greater than or equal to 'max' km/h, the movementTargetDistance is max.")]
        [SerializeField] private MinMaxFloat movementTargetDistanceSpeedFactors = new(40, 90);
        [Space(5)]
        [SerializeField, ReadOnly] private float desiredSteerAngle;

        [Header("Collisions")]
        [SerializeField] private float collisionRecoverDuration = 5; //TODO - make this value only start when the rigidbody velocity magnitude is less than a certain amount (has come to a stop)
        
        [Header("Braking")]
        [Tooltip("When the angle is supplied (x axis), the y axis represents the desired speed.")]
        [SerializeField] private AnimationCurve cornerBrakingCurve;
        [Tooltip("The y axis represents the amount of brake torque when x (the amount to brake) is a certain value. When the amount to brake is more, there should be more brake torque.")]
        [SerializeField] private AnimationCurve brakeTorqueCurve;
        [Space(5)]
        [SerializeField, ReadOnly] private bool isBraking;
        [SerializeField, ReadOnly] private float cornerSpeed;
        [SerializeField, ReadOnly] private float speedToBrakeTo;
        [SerializeField, ReadOnly] private float currentBrakeForce;
        
        [Header("Old")]
        [Tooltip("The time (in seconds) it takes to go from 0 to 60.")]
        [SerializeField] private float timeToAccelerateTo60 = 10;
        [Tooltip("The time (in seconds) it takes to go from 60 to 0.")]
        [SerializeField] private float timeToDecelerateFrom60 = 5;
        [Tooltip("The time (in seconds) it takes to go from 60 to 0 when there's an obstacle ahead.")]
        [SerializeField] private float emergencyBrakeTimeFrom60 = 2.5f;
        
        [Header("Debugging")]
        [SerializeField] private bool debug;
        [Space(5)]
        [SerializeField, ReadOnly] protected bool isInitialised;
        [SerializeField, ReadOnly] private Transform[] allWheelMeshes;
        [SerializeField, ReadOnly] private WheelCollider[] allWheelColliders;
        [Space(5)]
        [SerializeField, ReadOnly] protected Chunk currentChunk;
        [SerializeField, ReadOnly] protected bool isFrozen;
        [Space(5)]
        [SerializeField, ReadOnly] private bool isMoving;
        [SerializeField, ReadOnly] private float speed;
        [SerializeField, ReadOnly] private float desiredSpeed;
        [SerializeField, ReadOnly] private bool isAccelerating;
        [SerializeField, ReadOnly] private float timeSinceAccelerating;
        [SerializeField, ReadOnly] private float speedBeforeAccelerating;
        [SerializeField, ReadOnly] private bool isDecelerating;
        [SerializeField, ReadOnly] private float timeSinceDecelerating;
        [SerializeField, ReadOnly] private float speedBeforeDecelerating;
        [SerializeField, ReadOnly] private bool inCollision;

        private readonly List<Collision> collisions = new();
        private float timeOfLastCollision = -Mathf.Infinity;

        private Rigidbody rigidBody => GetComponent<Rigidbody>();
        private float timeSinceCollision => Time.realtimeSinceStartup - timeOfLastCollision;
        private bool recoveringFromCollision => timeSinceCollision < collisionRecoverDuration;
        public float DesiredSpeed => desiredSpeed;
        public float Speed => speed;

        public virtual void Initialise(Chunk currentChunk)
        {
            isInitialised = true;
            this.currentChunk = currentChunk;
            
            currentChunk.onBecomeAccessible += OnChunkBecomeAccessible;
            currentChunk.onBecomeInaccessible += OnChunkBecomeInaccessible;

            CacheAllWheelMeshes();
            CacheAllWheelColliders();
        }

        //TODO: braking/acceleration
        // - if collided with, start braking and enable drag

        protected virtual void OnDisable()
        {
            if (!isInitialised)
                return; //didn't get a chance to initialise
            
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
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            if (!LayersAndTags.AICarCollisionLayers.ContainsLayer(collision.gameObject.layer))
                return;

            GlobalLoggers.AICarLogger.Log($"{gameObject.name} collided with {collision.gameObject.name}");

            if (collisions.Count == 0)
            {
                OnCollisionStart();
            }

            timeOfLastCollision = Time.time; //reset collision time
            
            collisions.Add(collision);

            if (collision.rigidbody.Equals(WarehouseManager.Instance.CurrentCar.Rigidbody))
            {
                GlobalLoggers.AICarLogger.Log($"Player hit {gameObject.name} at {collision.impulse.magnitude}m/s");
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
        }

        private void OnChunkBecomeAccessible()
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

        private void OnStartMoving()
        {
            isMoving = true;

            if (debug) GlobalLoggers.AICarLogger.Log("Started moving");
        }

        private void OnStopMoving()
        {
            isMoving = false;
            speed = 0;
            desiredSpeed = 0;
            OnStopAccelerating();
            OnStopDecelerating();

            if (debug) GlobalLoggers.AICarLogger.Log("Stopped moving");
        }
        
        private void OnStartAccelerating()
        {
            isAccelerating = true;
            timeSinceAccelerating = 0;
            speedBeforeAccelerating = speed;
            
            if (debug) GlobalLoggers.AICarLogger.Log("Started accelerating");
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
            
            if (debug) GlobalLoggers.AICarLogger.Log("Stopped accelerating");
        }
        
        private void OnStartDecelerating()
        {
            isDecelerating = true;
            timeSinceDecelerating = 0;
            speedBeforeDecelerating = speed;
            
            if (debug) GlobalLoggers.AICarLogger.Log("Started decelerating");
        }

        private void OnDecelerate()
        {
            timeSinceDecelerating += Time.deltaTime;
        }
        
        private void OnStopDecelerating()
        {
            isDecelerating = false;
            timeSinceDecelerating = 0;
            
            if (debug) GlobalLoggers.AICarLogger.Log("Stopped decelerating");
        }

        protected void OnChangeDesiredSpeed(float newSpeed)
        {
            float previousSpeed = desiredSpeed;
            
            desiredSpeed = newSpeed;
            
            if (debug) GlobalLoggers.AICarLogger.Log($"Changed desired speed to {desiredSpeed}");

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

        protected void ForceSetSpeed(float speed)
        {
            this.speed = speed;
        }

        protected abstract (Chunk, Vector3, Quaternion)? GetPositionAhead(float distance);

        protected virtual void PreMoveChecks()
        {
            
        }
        
        private void Move()
        {
            PreMoveChecks();
            
            var targetPos = GetPositionAhead(GetMovementTargetDistance(Speed));
            if (targetPos == null)
            {
                Despawn();
                return;
            }
            
            if (!isMoving)
                OnStartMoving();

            var (newChunk, targetPosition, targetRotation) = targetPos.Value;
            Vector3 directionToTarget = targetPosition - transform.position;
            
            currentChunk = newChunk;
            speed = SpeedUtils.ToKmh(rigidBody.velocity.magnitude);
            desiredSteerAngle = -Vector2.SignedAngle(transform.forward.FlattenAsVector2(), directionToTarget.FlattenAsVector2());

            //debug directions:
            Debug.DrawLine(transform.position + rigidBody.velocity * 5, targetPosition, Color.green);
            Debug.DrawLine(transform.position, targetPosition, Color.yellow);

            CheckForCorner();
            CheckToBrake();
            
            if (isBraking)
            {
                ApplyBrakeForce();
            }
            else
            {
                ApplyAccelerationForce();
            }
            
            ApplySteering();

            UpdateWheelMeshes();
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
            isBraking = false; //reset for check

            const float speedingLeeway = 10; //the amount the player can speed past the desired speed before needing to brake
            float speedToObey = Mathf.Min(desiredSpeed + speedingLeeway, cornerSpeed);

            if (speed > speedToObey)
            {
                speedToBrakeTo = speedToObey;
                isBraking = true;
            }
            
            //TODO: if obstacle blocking, check if it is a rigidbody and set the speedToBrakeTo to the speed
            // - else set speedToBrakeTo to 0 if it's stationary
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

        private void ApplyAccelerationForce()
        {
            foreach (WheelCollider rearWheel in rearWheelColliders)
            {
                rearWheel.motorTorque = speed < desiredSpeed ? motorTorque : 0; //TODO: use some kind of interpolation
            }
        }
        
        private void ApplySteering()
        {
            foreach (WheelCollider frontWheel in frontWheelColliders)
            {
                frontWheel.steerAngle = desiredSteerAngle; //TODO: use some kind of interpolation
            }
        }

        /// <summary>
        /// Update all the wheel meshes to match the wheel colliders.
        /// </summary>
        private void UpdateWheelMeshes()
        {
            for (int count = 0; count < allWheelMeshes.Length; count++)
            {
                Transform mesh = allWheelMeshes[count];
                WheelCollider wheelCollider = allWheelColliders[count];
                
                wheelCollider.GetWorldPose(out Vector3 wheelPosition, out Quaternion wheelRotation);
                mesh.position = wheelPosition;
                mesh.rotation = wheelRotation;
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
            OnStopMoving();
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
        
        private void OnDrawGizmos()
        {
            Vector3 carCentreOfMassWorld = rigidBody.transform.TransformPoint(rigidBody.centerOfMass);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(carCentreOfMassWorld, 0.5f);
        }
        
    }
}
