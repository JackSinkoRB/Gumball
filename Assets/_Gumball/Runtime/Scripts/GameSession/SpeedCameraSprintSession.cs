using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/GameSession/Speed camera sprint")]
    public class SpeedCameraSprintSession : GameSession
    {

        [SerializeField] private SpeedCameraZone[] speedCameraZones;

        [SerializeField, ReadOnly] private GenericDictionary<AICar, HashSet<SpeedCameraZone>> zonesPassed = new();
        [SerializeField, ReadOnly] private GenericDictionary<AICar, HashSet<SpeedCameraZone>> zonesFailed = new();
        
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

        protected override void OnSessionStart()
        {
            base.OnSessionStart();

            zonesPassed.Clear();
        }

        public override void UpdateWhenCurrent()
        {
            base.UpdateWhenCurrent();

            CheckIfRacerHasFailedZones();
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
                    
                    if (zone.IsRacerInZone(racer) && racer.Speed >= zone.SpeedLimitKmh)
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
            
            GlobalLoggers.GameSessionLogger.Log($"{racer.name} passed zone at {zone.Position}m.");
        }

        private void OnFailZone(AICar racer, SpeedCameraZone zone)
        {
            HashSet<SpeedCameraZone> existingZones = zonesFailed.ContainsKey(racer) ? zonesFailed[racer] : new HashSet<SpeedCameraZone>();
            existingZones.Add(zone);
            
            GlobalLoggers.GameSessionLogger.Log($"{racer.name} failed zone at {zone.Position}m.");
        }
        
    }
}
