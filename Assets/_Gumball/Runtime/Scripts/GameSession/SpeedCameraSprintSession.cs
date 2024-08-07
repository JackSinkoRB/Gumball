using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dreamteck.Splines;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/GameSession/Speed camera sprint")]
    public class SpeedCameraSprintSession : RaceGameSession
    {
    
        public event Action<AICar, SpeedCameraZone> onPassZone;
        public event Action<AICar, SpeedCameraZone> onFailZone;

        [Header("Speed camera sprint")]
        [SerializeField] private SpeedCameraZone[] speedCameraZones;
        [Tooltip("How many kmh past the speed limit can the racer be travelling without failing?")]
        [SerializeField] private float speedLimitLeniencyKmh = 5;
        [SerializeField] private SpeedCameraZoneMarker zoneStartMarkerPrefab; 
        [SerializeField] private SpeedCameraZoneMarker zoneEndMarkerPrefab;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private GenericDictionary<AICar, HashSet<SpeedCameraZone>> zonesPassed = new();
        [SerializeField, ReadOnly] private GenericDictionary<AICar, HashSet<SpeedCameraZone>> zonesFailed = new();
        
        public SpeedCameraZone[] SpeedCameraZones => speedCameraZones;

        public override string GetName()
        {
            return "Speed camera sprint";
        }

        protected override GameSessionPanel GetSessionPanel()
        {
            return PanelManager.GetPanel<SpeedCameraSprintSessionPanel>();
        }
        
        protected override GameSessionEndPanel GetSessionEndPanel()
        {
            return PanelManager.GetPanel<SpeedCameraSprintSessionEndPanel>();
        }

        protected override IEnumerator LoadSession()
        {
            yield return base.LoadSession();
            
            zonesPassed.Clear();
            zonesFailed.Clear();

            SpawnZoneMarkers();
        }

        public override void UpdateWhenCurrent()
        {
            base.UpdateWhenCurrent();

            CheckIfRacerHasFailedZones();
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
                .ToArray();
        }

        private void SpawnZoneMarkers()
        {
            foreach (SpeedCameraZone zone in speedCameraZones)
            {
                SpawnZoneMarker(zoneStartMarkerPrefab, zone.Position - zone.Length, zone.SpeedLimitKmh);
                SpawnZoneMarker(zoneEndMarkerPrefab, zone.Position, zone.SpeedLimitKmh);
            }
        }

        private void SpawnZoneMarker(SpeedCameraZoneMarker prefab, float distanceAlongSpline, float speedLimitKmh)
        {
            if (prefab == null)
                return;

            SplineSample start = ChunkManager.Instance.GetSampleAlongSplines(distanceAlongSpline);
            SpeedCameraZoneMarker instance = prefab.gameObject.GetSpareOrCreate<SpeedCameraZoneMarker>(position: start.position.OffsetY(prefab.HeightAboveRoad), rotation: start.rotation);
            instance.Initialise(distanceAlongSpline, speedLimitKmh);
        }

        private void CheckIfRacerHasFailedZones()
        {
            foreach (SpeedCameraZone zone in speedCameraZones)
            {
                foreach (AICar racer in CurrentRacers.Keys)
                {
                    bool hasPassedZone = zonesPassed.ContainsKey(racer) && zonesPassed[racer].Contains(zone);
                    bool hasFailedZone = zonesFailed.ContainsKey(racer) && zonesFailed[racer].Contains(zone);

                    if (hasPassedZone || hasFailedZone)
                        continue;
                    
                    if (zone.IsRacerInZone(racer) && racer.Speed >= zone.SpeedLimitKmh + speedLimitLeniencyKmh)
                    {
                        OnFailZone(racer, zone);
                    }
                    else if (zone.HasRacerPassedZone(racer))
                    {
                        OnPassZone(racer, zone);
                    }
                }
            }
        }

        private void OnPassZone(AICar racer, SpeedCameraZone zone)
        {
            HashSet<SpeedCameraZone> existingZones = zonesPassed.ContainsKey(racer) ? zonesPassed[racer] : new HashSet<SpeedCameraZone>();
            existingZones.Add(zone);
            
            onPassZone?.Invoke(racer, zone);
            
            GlobalLoggers.GameSessionLogger.Log($"{racer.name} passed zone at {zone.Position}m.");
        }

        private void OnFailZone(AICar racer, SpeedCameraZone zone)
        {
            HashSet<SpeedCameraZone> existingZones = zonesFailed.ContainsKey(racer) ? zonesFailed[racer] : new HashSet<SpeedCameraZone>();
            existingZones.Add(zone);
            
            onFailZone?.Invoke(racer, zone);
            
            GlobalLoggers.GameSessionLogger.Log($"{racer.name} failed zone at {zone.Position}m.");
        }
        
    }
}
