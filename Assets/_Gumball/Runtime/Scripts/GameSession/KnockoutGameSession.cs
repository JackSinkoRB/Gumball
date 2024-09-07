using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/GameSession/Knockout")]
    public class KnockoutGameSession : RaceGameSession
    {

        [Header("Knockout")]
        [Tooltip("The distance (in metres) along the map for the knockout positions.")]
        [HelpBox("There is 1 knockout position per racer, with the Race Distance being the last knockout position.", MessageType.Info, HelpBoxAttribute.Position.ABOVE)]
        [SerializeField] private float[] knockoutPositions;
        
        [SerializeField] private CheckpointMarkers knockoutPositionMarkers;

        private List<AICar> remainingRacersInOrder
        {
            get
            {
                List<AICar> racers = new List<AICar>(RacersInPositionOrder);
                foreach (AICar eliminatedRacer in EliminatedRacers)
                    racers.Remove(eliminatedRacer);
                return racers;
            }
        }
        
        public readonly HashSet<AICar> EliminatedRacers = new();

        public override string GetName()
        {
            return "Knockout";
        }
        
        protected override GameSessionPanel GetSessionPanel()
        {
            return PanelManager.GetPanel<KnockoutSessionPanel>();
        }

        protected override SessionEndPanel GetSessionEndPanel()
        {
            return PanelManager.GetPanel<KnockoutSessionEndPanel>();
        }

        protected override IEnumerator LoadSession()
        {
            yield return base.LoadSession();

            EliminatedRacers.Clear();

            SpawnKnockoutPositionMarkers();
        }

        public override void UpdateWhenCurrent()
        {
            base.UpdateWhenCurrent();

            CheckToEliminateRacers();
        }

        private void CheckToEliminateRacers()
        {
            AICar secondLastRacer = RacersInPositionOrder[^(EliminatedRacers.Count + 2)];
            float secondLastRacerPosition = secondLastRacer.GetComponent<SplineTravelDistanceCalculator>().DistanceInMap;
            
            int knockoutPositionsPassed = 0;
            foreach (float knockoutPosition in knockoutPositions)
            {
                if (secondLastRacerPosition >= knockoutPosition)
                    knockoutPositionsPassed++;
            }

            int desiredRacers = knockoutPositions.Length - knockoutPositionsPassed + 1;
            int desiredEliminatedRacers = CurrentRacers.Count - desiredRacers;

            while (EliminatedRacers.Count < desiredEliminatedRacers)
            {
                EliminateLastRacer();

                bool isPlayerLastRacer = InProgress && desiredRacers == 1;
                if (isPlayerLastRacer)
                    EndSession(ProgressStatus.COMPLETE);
            }
        }

        private void EliminateLastRacer()
        {
            AICar lastRacer = remainingRacersInOrder[^1];
            
            GlobalLoggers.GameSessionLogger.Log($"Eliminating {lastRacer.name}");

            EliminatedRacers.Add(lastRacer);

            PanelManager.GetPanel<KnockoutSessionPanel>().ProgressBar.RemoveRacerIcon(lastRacer);
            
            lastRacer.SetObeySpeedLimit(true);

            if (lastRacer.IsPlayer)
                OnEliminatePlayer();
        }

        private void OnEliminatePlayer()
        {
            EndSession(ProgressStatus.ATTEMPTED);
        }

        private void SpawnKnockoutPositionMarkers()
        {
            foreach (float knockoutPosition in knockoutPositions)
            {
                knockoutPositionMarkers.Spawn(knockoutPosition);
            }
        }
        
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            SetKnockoutDistancesRelativeToRacers();
            ForceSetLastKnockoutPosition();
        }

        private void ForceSetLastKnockoutPosition()
        {
            if (knockoutPositions.Length == 0)
                return;
            
            knockoutPositions[^1] = raceDistanceMetres;
        }

        private void SetKnockoutDistancesRelativeToRacers()
        {
            int desiredKnockoutPositions = racerData.Length; //number of AI racers, + 1 for player, minus 1

            if (knockoutPositions.Length == desiredKnockoutPositions)
                return;
            
            if (desiredKnockoutPositions <= 0)
            {
                knockoutPositions = Array.Empty<float>();
                return;
            }

            knockoutPositions = new float[desiredKnockoutPositions];
            for (int index = 0; index < knockoutPositions.Length; index++)
            {
                float knockoutPosition = knockoutPositions[index];
                if (knockoutPosition == 0)
                    knockoutPosition = Mathf.RoundToInt((index + 1) * (raceDistanceMetres / knockoutPositions.Length));
                
                knockoutPositions[index] = knockoutPosition;
            }
        }
#endif
        
    }
}
