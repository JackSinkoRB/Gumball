using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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
        [Tooltip("The duration to go from 0 motor torque to the max motor torque.")]
        [SerializeField] private float accelerationDurationToMaxTorque = 1f;
        [SerializeField] private AnimationCurve accelerationEase;
        [SerializeField, ReadOnly] private bool isAccelerating;
        private Tween[] currentMotorTorqueTweens;
        
        [Header("Steering")]
        [SerializeField] private MinMaxFloat movementTargetDistance = new(3, 10);
        [Tooltip("At less than or equal to 'min' km/h, the movementTargetDistance is min.\n" +
                 "At greater than or equal to 'max' km/h, the movementTargetDistance is max.")]
        [SerializeField] private MinMaxFloat movementTargetDistanceSpeedFactors = new(40, 90);
        [Tooltip("The speed that the wheel mesh is interpolated to the desired steer angle. This makes it not snappy.")]
        [SerializeField] private float visualSteerSpeed = 1;
        [SerializeField] private float maxSteerAngle = 65;
        [Space(5)]
        [SerializeField, ReadOnly] private float desiredSteerAngle;
        [SerializeField, ReadOnly] private float visualSteerAngle;
        
        [Header("Collisions")]
        [SerializeField] private float collisionRecoverDuration = 1;
        
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
        [SerializeField, ReadOnly] private float speed;
        [SerializeField, ReadOnly] private float desiredSpeed;
        [SerializeField, ReadOnly] private bool inCollisionWithPlayer;

        private float timeOfLastCollision = -Mathf.Infinity;

        protected Rigidbody rigidBody => GetComponent<Rigidbody>();
        private float timeSinceCollision => Time.time - timeOfLastCollision;
        private bool recoveringFromCollision => inCollisionWithPlayer || timeSinceCollision < collisionRecoverDuration;
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
        
        protected void OnChangeDesiredSpeed(float newSpeed)
        {
            desiredSpeed = newSpeed;
            
            if (debug) GlobalLoggers.AICarLogger.Log($"Changed desired speed to {desiredSpeed}");
        }

        protected void ForceSetSpeed(float speed)
        {
            this.speed = speed;
        }

        protected abstract (Chunk, Vector3, Quaternion)? GetPositionAhead(float distance);

        protected virtual void PreMoveChecks()
        {
            
        }

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
            
            currentChunk = newChunk;
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

            if (isBraking || recoveringFromCollision)
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
            if (recoveringFromCollision)
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
        
        private void OnDrawGizmos()
        {
            Vector3 carCentreOfMassWorld = rigidBody.transform.TransformPoint(rigidBody.centerOfMass);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(carCentreOfMassWorld, 0.5f);
        }
        
    }
}
