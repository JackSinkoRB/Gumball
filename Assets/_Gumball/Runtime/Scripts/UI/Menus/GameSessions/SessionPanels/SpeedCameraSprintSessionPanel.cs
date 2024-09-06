using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class SpeedCameraSprintSessionPanel : RaceSessionPanel
    {

        [SerializeField] private Transform zoneHolder;
        [SerializeField] private SpeedCameraZoneMarkerUI zoneUIPrefab;

        private SpeedCameraSprintSession session => (SpeedCameraSprintSession)GameSessionManager.Instance.CurrentSession;
        
        protected override void OnShow()
        {
            base.OnShow();

            SetupZoneUI();
        }

        private void SetupZoneUI()
        {
            //pool previous markers
            foreach (Transform child in zoneHolder)
                child.gameObject.Pool();
            
            foreach (SpeedCameraZone zone in session.SpeedCameraZones)
            {
                SpeedCameraZoneMarkerUI markerUI = zoneUIPrefab.gameObject.GetSpareOrCreate<SpeedCameraZoneMarkerUI>(zoneHolder);
                markerUI.Initialise(zone);
            }
        }
        
    }
}