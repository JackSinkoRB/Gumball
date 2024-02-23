using System;
using System.Collections;
using System.Collections.Generic;
using Dreamteck.Splines;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [RequireComponent(typeof(Rigidbody))]
    public abstract class AICar : MonoBehaviour
    {

        private const float timeBetweenActivationChecks = 1;
        private const float carActivationRange = 100;
        
        [SerializeField] private Transform[] frontWheels;
        [SerializeField] private Transform[] wheels;
        
        [Header("Modify")]
        [SerializeField] private float collisionRecoverDuration = 5;
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
        [SerializeField, ReadOnly] protected bool isInitialised;
        [SerializeField, ReadOnly] protected Chunk currentChunk;
        [SerializeField, ReadOnly] protected bool isActivated = true;
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

        private readonly Cooldown activationCheckCooldown = new(timeBetweenActivationChecks);
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
            
            ActivationRangeCheck();
        }
        
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

            if (activationCheckCooldown.IsReady)
                ActivationRangeCheck();
            
            if (isActivated)
            {
                RotateWheels();
                if (!recoveringFromCollision && speed > 0.01f)
                    TurnFrontWheels();
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
            
            GetComponent<Rigidbody>().velocity = Vector3.zero;
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
            float distanceToPlayerSqr = Vector3.SqrMagnitude(WarehouseManager.Instance.CurrentCar.transform.position - transform.position);
            if (!isActivated && distanceToPlayerSqr < carActivationRangeSqr)
                Activate();
            else if (isActivated && distanceToPlayerSqr > carActivationRangeSqr)
                Deactivate();
        }

        private void Activate()
        {
            isActivated = true;

            GetComponent<Rigidbody>().useGravity = true;
            
            foreach (Collider carCollider in GetComponents<Collider>())
            {
                carCollider.enabled = true;
            }
        }

        private void Deactivate()
        {
            isActivated = false;
            
            GetComponent<Rigidbody>().useGravity = false;

            foreach (Collider carCollider in GetComponents<Collider>())
            {
                carCollider.enabled = false;
            }
        }

        private void OnStartMoving()
        {
            isMoving = true;
            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            if (debug) GlobalLoggers.AICarLogger.Log("Started moving");
        }

        private void OnStopMoving()
        {
            isMoving = false;
            speed = 0;
            desiredSpeed = 0;
            OnStopAccelerating();
            OnStopDecelerating();
            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;

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
        
        protected abstract (Chunk, Vector3, Quaternion)? GetTargetPosition();

        protected virtual void PreMoveChecks()
        {
            
        }
        
        private void Move()
        {
            PreMoveChecks();
            
            var targetPos = GetTargetPosition();
            if (targetPos == null)
            {
                Despawn();
                return;
            }
            
            float accelerationTime = ((desiredSpeed - speedBeforeAccelerating)/60f) * timeToAccelerateTo60;
            float decelerationTime = ((speedBeforeDecelerating - desiredSpeed)/60f) * timeToDecelerateFrom60; //TODO: there should be a min/max time to decelerate - depending on a 'safe distance' (normally 3 second gap, otherwise racers might be less with more braking power)
            //TODO cont. Eg. If safe distance is 3, if distance to car in front is 2
            
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
            
            rigidBody.MoveRotation(Quaternion.Slerp(rigidBody.rotation, targetRotationFinal, GetTurnSpeed(speed) * Time.deltaTime));

            Vector3 targetVelocity = transform.forward * SpeedUtils.FromKmh(speed); //car always moves forward
            rigidBody.velocity = targetVelocity;
            Debug.DrawLine(transform.position + GetComponent<Rigidbody>().velocity * 5, targetPosition, Color.green);
            Debug.DrawLine(transform.position, targetPosition, Color.yellow);
        }
        
        private void RotateWheels()
        {
            foreach (Transform wheel in wheels)
            {
                wheel.Rotate(Vector3.up, wheelRotateSpeed * speed * Time.deltaTime, Space.Self);
            }
        }

        protected float GetMovementTargetDistance(float speedToCheck)
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

        protected abstract Vector3? GetFrontWheelTurnDirection();
        
        private void TurnFrontWheels()
        {
            Vector3? frontWheelTurnDirection = GetFrontWheelTurnDirection();
            if (frontWheelTurnDirection == null)
                return;

            Vector3 directionAhead = frontWheelTurnDirection.Value;
            
            Debug.DrawLine(transform.position, transform.position + (directionAhead * GetMovementTargetDistance(speed)), Color.blue);

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

        private void Despawn()
        {
            OnStopMoving();
            gameObject.Pool();
            GlobalLoggers.AICarLogger.Log($"Despawned at {transform.position}");
        }
        
    }
}
