using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class SpeedCameraZoneMarkerUI : MonoBehaviour
    {

        [SerializeField] private GameObject passed;
        [SerializeField] private GameObject failed;

        private SpeedCameraZone zone;
        
        private SpeedCameraSprintSession session => (SpeedCameraSprintSession)GameSessionManager.Instance.CurrentSession;

        public void Initialise(SpeedCameraZone zone)
        {
            this.zone = zone;
            
            //start disabled
            passed.gameObject.SetActive(false);
            failed.gameObject.SetActive(false);
            
            session.onPassZone += OnPassZone;
            session.onFailZone += OnFailZone;
        }

        private void OnDisable()
        {
            session.onPassZone -= OnPassZone;
            session.onFailZone -= OnFailZone;
        }

        private void OnPassZone(AICar racer, SpeedCameraZone zone)
        {
            if (this.zone == zone && racer == WarehouseManager.Instance.CurrentCar)
                SetPassed();
        }
        
        private void OnFailZone(AICar racer, SpeedCameraZone zone)
        {
            if (this.zone == zone && racer == WarehouseManager.Instance.CurrentCar)
                SetFailed();
        }
        
        private void SetPassed()
        {
            passed.gameObject.SetActive(true);
        }

        private void SetFailed()
        {
            failed.gameObject.SetActive(true);
        }
        
    }
}
