using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/GameSession/Clean run")]
    public class CleanRunGameSession : TimedGameSession
    {
        
        [SerializeField] private float maxCollisionImpulseAllowed = 500;
        
        public override string GetName()
        {
            return "Clean run";
        }

        protected override GameSessionPanel GetSessionPanel()
        {
            return PanelManager.GetPanel<CleanRunSessionPanel>();
        }
        
        protected override SessionEndPanel GetSessionEndPanel()
        {
            return PanelManager.GetPanel<CleanRunSessionEndPanel>();
        }

        protected override void OnSessionStart()
        {
            base.OnSessionStart();

            WarehouseManager.Instance.CurrentCar.onCollisionEnter += OnCollision;
        }

        protected override void OnSessionEnd()
        {
            base.OnSessionEnd();
            
            WarehouseManager.Instance.CurrentCar.onCollisionEnter -= OnCollision;
        }
        
        private void OnCollision(Collision collision)
        {
            float magnitudeSqr = collision.impulse.sqrMagnitude;
            float maxMagnitudeSqrRequired = maxCollisionImpulseAllowed * maxCollisionImpulseAllowed;

            if (magnitudeSqr >= maxMagnitudeSqrRequired)
            {
                EndSession(ProgressStatus.ATTEMPTED);
            }
        }
        
    }
}
