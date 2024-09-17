using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class SkillCheckManager : Singleton<SkillCheckManager>
    {

        [Header("Checks")]
        [SerializeField] private NearMissSkillCheck nearMiss;
        [SerializeField] private SlipStreamSkillCheck slipStream;
        [SerializeField] private AirTimeSkillCheck airTime;
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private float currentPoints;
        
        public SkillCheck[] AllSkillChecks { get; private set; }
        
        public float CurrentPoints => currentPoints;
        public SlipStreamSkillCheck SlipStream => slipStream;
        public NearMissSkillCheck NearMiss => nearMiss;
        public AirTimeSkillCheck AirTime => airTime;

        protected override void Initialise()
        {
            base.Initialise();

            FindAllChecks();
            DisableUI();
        }

        public void ResetForSession()
        {
            //start disabled
            DisableUI();

            currentPoints = 0;
            slipStream.ResetSessionPoints();
            nearMiss.ResetSessionPoints();
            airTime.ResetSessionPoints();
        }

        public void AddPoints(float points)
        {
            currentPoints += points;
        }
        
        private void Update()
        {
            if (GameSessionManager.Instance.CurrentSession == null
                || !GameSessionManager.Instance.CurrentSession.HasStarted)
            {
                DisableUI();
                return;
            }

            nearMiss.CheckIfPerformed();
            slipStream.CheckIfPerformed();
            airTime.CheckIfPerformed();
        }
        
        private void FindAllChecks()
        {
            List<SkillCheck> allChecks = new List<SkillCheck>
            {
                nearMiss,
                airTime,
                slipStream
            };
            AllSkillChecks = allChecks.ToArray();
        }

        private void DisableUI()
        {
            nearMiss.Label.gameObject.SetActive(false);
            slipStream.Label.gameObject.SetActive(false);
            airTime.Label.gameObject.SetActive(false);
            airTime.LandingLabel.gameObject.SetActive(false);
        }

    }
}
