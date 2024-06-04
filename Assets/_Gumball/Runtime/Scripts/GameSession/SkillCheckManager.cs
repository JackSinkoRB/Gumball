using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using TMPro;
#if UNITY_EDITOR
using Gumball.Editor;
#endif
using UnityEngine;

namespace Gumball
{
    public class SkillCheckManager : MonoBehaviour
    {
        
        [Header("Near miss")]
        [SerializeField] private TextMeshProUGUI nearMissLabel;
        [SerializeField] private float nearMissLabelDuration = 1;
        [Tooltip("The minimum speed the player must be going for a near miss.")]
        [SerializeField] private float minSpeedForNearMissKmh = 25;
        [Tooltip("How long (in seconds) after a collision should it wait to have no collisions before checking for a near miss?")]
        [SerializeField] private float minTimeWithNoCollisionForNearMiss = 1;
        [Tooltip("The max distance around a traffic car to consider a near miss.")]
        [SerializeField] private float nearMissMaxDistance = 1;
        [SerializeField, Range(0,1)] private float nearMissNosBonus = 0.1f;
        [SerializeField] private float nearMissPointBonus = 5;
        [Space(5)]
        [SerializeField, ReadOnly] private float timeSinceLastCollision;

        private readonly Dictionary<AICar, float> timeTrafficCarEnteredNearMissRadius = new();
        private readonly Collider[] tempNearMissHolder = new Collider[50];
        private Coroutine nearMissLabelCoroutine;
        
        [Header("Slip stream")]
        [SerializeField] private TextMeshProUGUI slipStreamLabel;
        [Tooltip("The minimum speed the player must be going to enable slip stream.")]
        [SerializeField] private float minSpeedForSlipStreamKmh = 25;
        [Tooltip("The width around the cars front position for a racer to be in to trigger slipstreaming.")]
        [SerializeField] private float slipStreamWidth = 1; 
        [Tooltip("The max distance in front of the player that can be considered it slip streaming.")]
        [SerializeField] private float slipStreamMaxDistance = 3;
        [Tooltip("A nos bonus (measured in percent) to give each second while the player is within another racer cars slip stream bounds.")]
        [SerializeField, Range(0,1)] private float slipStreamNosBonus = 0.1f;
        [Tooltip("The amount of points to give each second the player is slip streaming.")]
        [SerializeField] private float slipStreamPointBonus = 1;
        [Space(5)]
        [SerializeField, ReadOnly] private bool isSlipStreaming;
        [SerializeField, ReadOnly] private float pointsGainedSinceSlipStreamStarted;

        private readonly RaycastHit[] tempSlipStreamHolder = new RaycastHit[1];
        
        [Header("Air time")]
        [SerializeField] private TextMeshProUGUI airTimeLabel;
        [SerializeField] private float minimumSpeedForAirTimeKmh = 25;
        [Tooltip("A nos bonus (measured in percent) to give each second while the player has 0 wheels touching the ground and moving at the minimum speed.")]
        [SerializeField, Range(0,1)] private float airTimeNosBonus = 0.1f;
        [Tooltip("The amount of points to give each second the player has air time.")]
        [SerializeField] private float airTimePointBonus = 1;
        [Space(5)]
        [SerializeField, ReadOnly] private bool isInAir;
        [SerializeField, ReadOnly] private float timeInAir;

        [Header("Landing")]
        [SerializeField] private TextMeshProUGUI landingLabel;
        [SerializeField] private float landingLabelDuration = 1;
        [SerializeField] private float minTimeInAirForLandingPoints = 0.3f;
        [Tooltip("A nos bonus (measured in percent) to give when the player performs a landing.")]
        [SerializeField, Range(0, 1)] private float landingNosBonus = 0.1f;
        [SerializeField] private float landingPointBonus = 5;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private float currentPoints;
        
        public float CurrentPoints => currentPoints;

        private void OnEnable()
        {
            //start disabled
            slipStreamLabel.gameObject.SetActive(false);
            nearMissLabel.gameObject.SetActive(false);
            airTimeLabel.gameObject.SetActive(false);
            landingLabel.gameObject.SetActive(false);

            currentPoints = 0;
        }

        private void Update()
        {
            if (GameSessionManager.Instance.CurrentSession == null
                || !GameSessionManager.Instance.CurrentSession.HasStarted)
            {
                return;
            }

            CheckForNearMiss();
            CheckForSlipStream();
            CheckForAirTime();
        }

        private void CheckForNearMiss()
        {
            if (WarehouseManager.Instance.CurrentCar.InCollision)
            {
                timeSinceLastCollision = 0;
                return;
            }

            timeSinceLastCollision += Time.deltaTime;

            if (timeSinceLastCollision < minTimeWithNoCollisionForNearMiss)
                return;
            
            if (WarehouseManager.Instance.CurrentCar.Speed < minSpeedForNearMissKmh)
                return;
            
            Vector3 halfExtents = new Vector3(WarehouseManager.Instance.CurrentCar.CarWidth / 2f + nearMissMaxDistance, 1, WarehouseManager.Instance.CurrentCar.FrontOfCarPosition.z + nearMissMaxDistance);
            
            int hits = Physics.OverlapBoxNonAlloc(WarehouseManager.Instance.CurrentCar.transform.position, halfExtents, tempNearMissHolder, WarehouseManager.Instance.CurrentCar.transform.rotation, LayersAndTags.GetLayerMaskFromLayer(LayersAndTags.Layer.TrafficCar));

            HashSet<AICar> previousCarsInRadius = new HashSet<AICar>(timeTrafficCarEnteredNearMissRadius.Keys);
            HashSet<AICar> carsInRadius = new();
            
            //track the cars that entered
            for (int count = 0; count < hits; count++)
            {
                AICar trafficCar = tempNearMissHolder[count].transform.GetComponentInAllParents<AICar>();
                carsInRadius.Add(trafficCar);
                
                if (!timeTrafficCarEnteredNearMissRadius.ContainsKey(trafficCar))
                {
                    //entered near miss radius
                    OnTrafficCarEnterNearMissRadius(trafficCar);
                }
            }

            //track the cars that exited
            foreach (AICar trafficCar in previousCarsInRadius)
            {
                if (!carsInRadius.Contains(trafficCar))
                {
                    //no longer in near miss radius
                    OnTrafficCarExitNearMissRadius(trafficCar);
                }
            }
            
#if UNITY_EDITOR
            BoxCastUtils.DrawBox(WarehouseManager.Instance.CurrentCar.transform.position, halfExtents, WarehouseManager.Instance.CurrentCar.transform.rotation, hits > 0 ? Color.yellow : Color.grey);
#endif
        }
        
        private void CheckForSlipStream()
        {
            if (WarehouseManager.Instance.CurrentCar.InCollision)
            {
                OnStopSlipStream();
                return;
            }

            if (WarehouseManager.Instance.CurrentCar.Speed < minSpeedForSlipStreamKmh)
            {
                OnStopSlipStream();
                return;
            }

            Vector3 halfExtents = new Vector3(slipStreamWidth / 2f, 1, 0);
            Vector3 frontOfCarPosition = WarehouseManager.Instance.CurrentCar.transform.TransformPoint(WarehouseManager.Instance.CurrentCar.FrontOfCarPosition);
            
            int hits = Physics.BoxCastNonAlloc(frontOfCarPosition, halfExtents, WarehouseManager.Instance.CurrentCar.transform.forward, tempSlipStreamHolder, WarehouseManager.Instance.CurrentCar.transform.rotation, slipStreamMaxDistance / 2f, LayersAndTags.GetLayerMaskFromLayer(LayersAndTags.Layer.RacerCar));

            if (hits > 0)
                OnPerformSlipStream();
            else if (isSlipStreaming)
                OnStopSlipStream();

#if UNITY_EDITOR
            BoxCastUtils.DrawBoxCastBox(frontOfCarPosition, halfExtents, WarehouseManager.Instance.CurrentCar.transform.rotation, WarehouseManager.Instance.CurrentCar.transform.forward, slipStreamMaxDistance / 2f, hits > 0 ? Color.green : Color.cyan);
#endif
        }

        private void CheckForAirTime()
        {
            if (WarehouseManager.Instance.CurrentCar.Speed < minimumSpeedForAirTimeKmh)
            {
                OnStopAirTime();
                return;
            }
            
            if (WarehouseManager.Instance.CurrentCar.IsInAir && !isInAir)
            {
                OnStartAirTime();
                OnPerformAirTime();
            } else if (!WarehouseManager.Instance.CurrentCar.IsInAir && isInAir)
            {
                OnStopAirTime();
            } else if (isInAir)
            {
                OnPerformAirTime();
            }
        }

        private void OnPerformNearMiss()
        {
            if (nearMissLabelCoroutine != null)
                StopCoroutine(nearMissLabelCoroutine);

            nearMissLabelCoroutine = StartCoroutine(ShowNearMissLabelIE());

            WarehouseManager.Instance.CurrentCar.NosManager.AddNos(nearMissNosBonus);

            currentPoints += nearMissPointBonus;
        }

        private void OnStartSlipStream()
        {
            if (isSlipStreaming)
                return; //already started
            
            isSlipStreaming = true;
            
            slipStreamLabel.gameObject.SetActive(true);

            pointsGainedSinceSlipStreamStarted = 0;
        }
        
        private void OnPerformSlipStream()
        {
            if (!isSlipStreaming)
                OnStartSlipStream();
            
            WarehouseManager.Instance.CurrentCar.NosManager.AddNos(slipStreamNosBonus * Time.deltaTime);
            
            pointsGainedSinceSlipStreamStarted += slipStreamPointBonus * Time.deltaTime;
            slipStreamLabel.text = $"Slipstream +{Mathf.CeilToInt(pointsGainedSinceSlipStreamStarted)}";
            
            currentPoints += slipStreamPointBonus * Time.deltaTime;
        }

        private void OnStopSlipStream()
        {
            if (!isSlipStreaming)
                return; //already stopped
            
            isSlipStreaming = false;
            slipStreamLabel.gameObject.SetActive(false);
        }

        private void OnStartAirTime()
        {
            if (isInAir)
                return; //already in air
            
            isInAir = true;
            timeInAir = 0;
            
            airTimeLabel.gameObject.SetActive(true);
        }
        
        private void OnPerformAirTime()
        {
            timeInAir += Time.deltaTime;
            float pointsGainedSinceAirTimeStarted = airTimePointBonus * timeInAir;
            
            WarehouseManager.Instance.CurrentCar.NosManager.AddNos(airTimeNosBonus * Time.deltaTime);

            currentPoints += airTimePointBonus * Time.deltaTime;
            
            airTimeLabel.text = $"Air time +{Mathf.CeilToInt(pointsGainedSinceAirTimeStarted)}";
        }

        private void OnStopAirTime()
        {
            if (!isInAir)
                return; //already landed
            
            isInAir = false;
            
            airTimeLabel.gameObject.SetActive(false);

            if (timeInAir < minTimeInAirForLandingPoints)
                return;

            OnPerformLanding();
        }

        private void OnPerformLanding()
        {
            StartCoroutine(ShowLandingLabelIE());
            
            WarehouseManager.Instance.CurrentCar.NosManager.AddNos(landingNosBonus);

            currentPoints += landingPointBonus;
        }
        
        private void OnTrafficCarEnterNearMissRadius(AICar car)
        {
            timeTrafficCarEnteredNearMissRadius[car] = Time.time;
        }

        private void OnTrafficCarExitNearMissRadius(AICar car)
        {
            float timeSinceEntering = Time.time - timeTrafficCarEnteredNearMissRadius[car];
            bool hasBeenInCollisionSinceEntering = timeSinceEntering > timeSinceLastCollision;
            
            if (!hasBeenInCollisionSinceEntering)
                OnPerformNearMiss();
            
            timeTrafficCarEnteredNearMissRadius.Remove(car);
        }
        
        private IEnumerator ShowNearMissLabelIE()
        {
            nearMissLabel.gameObject.SetActive(true);
            nearMissLabel.text = $"Near miss +{nearMissPointBonus}";
            yield return new WaitForSeconds(nearMissLabelDuration);
            nearMissLabel.gameObject.SetActive(false);
        }
        
        private IEnumerator ShowLandingLabelIE()
        {
            landingLabel.gameObject.SetActive(true);
            landingLabel.text = $"Landing +{landingPointBonus}";
            yield return new WaitForSeconds(landingLabelDuration);
            landingLabel.gameObject.SetActive(false);
        }
        
    }
}
