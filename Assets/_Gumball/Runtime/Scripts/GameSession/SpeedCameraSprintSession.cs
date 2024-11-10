using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dreamteck.Splines;
using MyBox;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/GameSession/Speed camera sprint")]
    public class SpeedCameraSprintSession : RaceGameSession
    {
    
        public event Action<AICar, SpeedCameraZone> onPassZone;
        public event Action<AICar, SpeedCameraZone> onFailZone;

        [Header("Speed camera sprint")]
        [SerializeField] private SpeedCameraZoneMarker zoneMarkerPrefab;
        [Tooltip("How many kmh past the speed limit can the racer be travelling without failing?")]
        [SerializeField] private float speedLimitLeniencyKmh = 5;
        [SerializeField] private SpeedCameraZone[] speedCameraZones;
        [HelpBox("There is 1 value per racer.", MessageType.Info, HelpBoxAttribute.Position.ABOVE)]
        [Tooltip("Relative to the order of the session racer data.")]
        [SerializeField] private SpeedCameraSprintRacerData[] racerSpeedCameraSprintData;
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private GenericDictionary<AICar, GenericDictionary<SpeedCameraZone, MinMaxFloat>> preCalculatedSpeedLimitPositions = new();
        [SerializeField, ReadOnly] private GenericDictionary<AICar, HashSet<SpeedCameraZone>> zonesPassed = new();
        [SerializeField, ReadOnly] private GenericDictionary<AICar, HashSet<SpeedCameraZone>> zonesFailed = new();
        
        public SpeedCameraZone[] SpeedCameraZones => speedCameraZones;

        public override string GetModeDisplayName()
        {
            return "Speed camera sprint";
        }
        
        public override Sprite GetModeIcon()
        {
            return GameSessionManager.Instance.SpeedCameraSprintIcon;
        }

        protected override GameSessionPanel GetSessionPanel()
        {
            return PanelManager.GetPanel<SpeedCameraSprintSessionPanel>();
        }
        
        protected override SessionEndPanel GetSessionEndPanel()
        {
            return PanelManager.GetPanel<SpeedCameraSprintSessionEndPanel>();
        }

        protected override IEnumerator LoadSession()
        {
            yield return base.LoadSession();
            
            zonesPassed.Clear();
            zonesFailed.Clear();

            SpawnZoneMarkers();

            CalculateTemporarySpeedLimitDistances();
        }
        
        private void CalculateTemporarySpeedLimitDistances()
        {
            preCalculatedSpeedLimitPositions.Clear();

            foreach (SpeedCameraZone zone in speedCameraZones)
            {
                int racerIndex = 0;
                foreach (AICar racer in CurrentRacers.Keys)
                {
                    if (racer.IsPlayer)
                        continue;

                    float brakingPosition = zone.Position - racerSpeedCameraSprintData[racerIndex].BrakingDistanceRange.RandomInRange();
                    float accelerationPosition = zone.Position + racerSpeedCameraSprintData[racerIndex].AccelerationDistanceRange.RandomInRange();
                    
                    if (!preCalculatedSpeedLimitPositions.ContainsKey(racer))
                        preCalculatedSpeedLimitPositions[racer] = new GenericDictionary<SpeedCameraZone, MinMaxFloat>();
                    preCalculatedSpeedLimitPositions[racer][zone] = new MinMaxFloat(brakingPosition, accelerationPosition);
                    
                    racerIndex++;
                }
            }

        }

        public override void UpdateWhenCurrent()
        {
            base.UpdateWhenCurrent();

            CheckIfRacerShouldBrakeForZone();
            CheckIfRacerHasPassedOrFailedZones();
        }
        
        public bool HasCarFailedZone(AICar car, SpeedCameraZone zone)
        {
            return zonesFailed.ContainsKey(car) && zonesFailed[car].Contains(zone);
        }
        
        public bool HasCarPassedZone(AICar car, SpeedCameraZone zone)
        {
            return zonesPassed.ContainsKey(car) && zonesPassed[car].Contains(zone);
        }

        protected override void UpdateRacersPositions()
        {
            racersInPositionOrderCached = CurrentRacers.Keys.OrderBy(racer => zonesFailed.ContainsKey(racer) ? zonesFailed[racer].Count : 0)
                .ThenByDescending(racer => racer.GetComponent<SplineTravelDistanceCalculator>().DistanceInMap)
                .ToList();
        }
        
        private void SpawnZoneMarkers()
        {
            if (zoneMarkerPrefab == null)
                return;
            
            foreach (SpeedCameraZone zone in speedCameraZones)
            {
                SplineSample splineSample = ChunkManager.Instance.GetSampleAlongSplines(zone.Position);
                SpeedCameraZoneMarker instance = zoneMarkerPrefab.gameObject.GetSpareOrCreate<SpeedCameraZoneMarker>(position: splineSample.position.OffsetY(zoneMarkerPrefab.HeightAboveRoad), rotation: splineSample.rotation);
                instance.Initialise(zone);
            }
        }

        private void CheckIfRacerShouldBrakeForZone()
        {
            foreach (AICar racer in CurrentRacers.Keys)
            {
                if (racer == null)
                    continue;
                
                if (racer.IsPlayer)
                    continue;
                
                racer.RemoveTemporarySpeedLimit();
                
                foreach (SpeedCameraZone zone in speedCameraZones)
                {
                    //if racer is between the distance, set temporary limit
                    float position = racer.GetComponent<SplineTravelDistanceCalculator>().DistanceInMap;
                    MinMaxFloat brakingPositions = preCalculatedSpeedLimitPositions[racer][zone];
                    bool isWithinZone = brakingPositions.IsInRange(position);
                    if (isWithinZone)
                    {
                        racer.SetTemporarySpeedLimit(zone.SpeedLimitKmh);
                        break;
                    }
                }
            }
        }

        private void CheckIfRacerHasPassedOrFailedZones()
        {
            foreach (SpeedCameraZone zone in speedCameraZones)
            {
                foreach (AICar racer in CurrentRacers.Keys)
                {
                    if (racer == null)
                        continue;
                    
                    bool hasPassedZone = zonesPassed.ContainsKey(racer) && zonesPassed[racer].Contains(zone);
                    bool hasFailedZone = zonesFailed.ContainsKey(racer) && zonesFailed[racer].Contains(zone);

                    if (hasPassedZone || hasFailedZone)
                        continue;

                    if (zone.HasRacerPassedZone(racer))
                    {
                        if (racer.SpeedKmh >= zone.SpeedLimitKmh + speedLimitLeniencyKmh)
                            OnFailZone(racer, zone);
                        else
                            OnPassZone(racer, zone);
                    }
                }
            }
        }

        private void OnPassZone(AICar racer, SpeedCameraZone zone)
        {
            HashSet<SpeedCameraZone> existingZones = zonesPassed.ContainsKey(racer) ? zonesPassed[racer] : new HashSet<SpeedCameraZone>();
            existingZones.Add(zone);
            zonesPassed[racer] = existingZones;
            
            onPassZone?.Invoke(racer, zone);
            
            GlobalLoggers.GameSessionLogger.Log($"{racer.name} passed zone at {zone.Position}m.");
        }

        private void OnFailZone(AICar racer, SpeedCameraZone zone)
        {
            HashSet<SpeedCameraZone> existingZones = zonesFailed.ContainsKey(racer) ? zonesFailed[racer] : new HashSet<SpeedCameraZone>();
            existingZones.Add(zone);
            zonesFailed[racer] = existingZones;

            if (racer.IsPlayer)
                GameSessionAudioManager.Instance.PlayerFailSpeedCameraSprintZone.Play();
            
            onFailZone?.Invoke(racer, zone);
            
            GlobalLoggers.GameSessionLogger.Log($"{racer.name} failed zone at {zone.Position}m doing {racer.SpeedKmh}kmh.");
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            UpdateRacerDataArray();
        }

        private void UpdateRacerDataArray()
        {
            int desiredDistances = racerData.Length; //number of AI racers (not including player)
            
            if (racerSpeedCameraSprintData.Length == desiredDistances)
                return;
            
            if (desiredDistances <= 0)
            {
                racerSpeedCameraSprintData = Array.Empty<SpeedCameraSprintRacerData>();
                return;
            }

            //copy previous values
            SpeedCameraSprintRacerData[] newArray = new SpeedCameraSprintRacerData[desiredDistances];
            for (int index = 0; index < desiredDistances; index++)
            {
                if (index >= racerSpeedCameraSprintData.Length)
                    break;
                
                SpeedCameraSprintRacerData previousValue = racerSpeedCameraSprintData[index];
                newArray[index] = previousValue;
            }

            racerSpeedCameraSprintData = newArray;
        }
#endif
        
    }
}
