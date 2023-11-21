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

        private const float timeBetweenChunkChecks = 1;

        [SerializeField] private Transform[] frontWheels;
        [SerializeField] private Transform[] wheels;
        [SerializeField] private float wheelRotateSpeed = 10;
        
        [SerializeField] private float moveSpeed = 10; //todo: remove and use the chunk's speed limit
        [SerializeField] private float turnSpeed = 10;

        [Header("Debugging")]
        [SerializeField] private bool debug;
        [SerializeField, ReadOnly] private bool activated;
        [SerializeField, ReadOnly] private ChunkTrafficManager currentChunkTraffic;

        private float timeSinceLastChunkCheck;
        private float currentLaneDistance;
        private readonly RaycastHit[] terrainRaycastHits = new RaycastHit[1];

        private Rigidbody rigidbody => GetComponent<Rigidbody>();
        
        private void OnEnable()
        {
            DoChunkCheck();
        }
        
        private void Update()
        {
            TryChunkCheck();

            //todo: activation range - enable/disable 'activated' if within range of player
            
            if (activated)
            {
                Move();
                RotateWheels();
            }
        }
        
        public void SetLaneDistance(float laneDistance)
        {
            currentLaneDistance = laneDistance;
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
                
                int closestSplineIndex = currentChunkTraffic.Chunk.GetClosestSampleIndexOnSpline(transform.position, true).Item1;
                bool faceForward = currentChunkTraffic.DriveOnLeft && currentLaneDistance < 0;
                int desiredSplineSampleIndex = faceForward ? closestSplineIndex + turningDistance : closestSplineIndex - turningDistance;
                if (desiredSplineSampleIndex >= currentChunkTraffic.Chunk.SplineSamples.Length || desiredSplineSampleIndex < 0)
                {
                    //todo: get next chunk
                    break;
                }
                
                var (position, rotation) = currentChunkTraffic.GetLanePosition(currentChunkTraffic.Chunk.SplineSamples[desiredSplineSampleIndex], currentLaneDistance);
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
            var (targetPosition, targetRotation) = GetTargetPosition();
            
            Vector3 direction = (targetPosition - transform.position).normalized;
            Vector3 targetVelocity = direction * moveSpeed;
            rigidbody.velocity = Vector3.Lerp(rigidbody.velocity, targetVelocity, moveSpeed * Time.deltaTime);

            rigidbody.MoveRotation(Quaternion.Slerp(rigidbody.rotation, targetRotation, turnSpeed * Time.deltaTime));
        }

        private (Vector3, Quaternion) GetTargetPosition()
        {
            //get the closest spline sample, then get the spline sample ahead and spline sample 10 ahead and 
            int closestSplineIndex = currentChunkTraffic.Chunk.GetClosestSampleIndexOnSpline(transform.position, true).Item1;
            bool faceForward = currentChunkTraffic.DriveOnLeft && currentLaneDistance < 0;
            int desiredSplineSampleIndex = faceForward ? closestSplineIndex + 1 : closestSplineIndex - 1;

            if (desiredSplineSampleIndex >= currentChunkTraffic.Chunk.SplineSamples.Length || desiredSplineSampleIndex < 0)
                return (transform.position, transform.rotation); //todo: need to get the next chunks splines

            var (position, rotation) = currentChunkTraffic.GetLanePosition(currentChunkTraffic.Chunk.SplineSamples[desiredSplineSampleIndex], currentLaneDistance);

            return (position, rotation);
        }

        private void TryChunkCheck()
        {
            timeSinceLastChunkCheck += Time.deltaTime;

            if (timeSinceLastChunkCheck > timeBetweenChunkChecks)
            {
                timeSinceLastChunkCheck = 0;
                DoChunkCheck();
            }
        }

        private void DoChunkCheck()
        {
            //raycast down to get the chunk
            int hits = Physics.RaycastNonAlloc(transform.position,
                Vector3.down, terrainRaycastHits, Mathf.Infinity,
                GameObjectLayers.GetLayerMaskFromLayer(GameObjectLayers.Layer.Terrain));
            if (hits == 0)
            {
                gameObject.Pool();
            }
            else
            {
                currentChunkTraffic = terrainRaycastHits[0].transform.parent.GetComponentInChildren<ChunkTrafficManager>();
            }
        }
        
    }
}
