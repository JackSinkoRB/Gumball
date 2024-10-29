using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Gumball
{
    public class SpeedCameraSprintSessionPanel : RaceSessionPanel
    {

        [SerializeField] private Transform zoneHolder;
        [SerializeField] private SpeedCameraZoneMarkerUI zoneUIPrefab;

        [Header("Screen flash on fail")]
        [SerializeField] private CanvasGroup flashObject;
        [SerializeField] private float flashDuration;
        [SerializeField] private Ease flashEaseIn = Ease.InOutSine;
        [SerializeField] private Ease flashEaseOut = Ease.InOutSine;
        
        private Sequence flashTween;
        
        private SpeedCameraSprintSession session => (SpeedCameraSprintSession)GameSessionManager.Instance.CurrentSession;
        
        protected override void OnShow()
        {
            base.OnShow();

            SetupZoneUI();

            session.onFailZone += OnFailZone;
        }

        protected override void OnHide()
        {
            base.OnHide();

            if (session != null)
                session.onFailZone -= OnFailZone;
        }

        private void OnFailZone(AICar car, SpeedCameraZone zone)
        {
            if (car.IsPlayer)
                DoScreenFlash();
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

        private void DoScreenFlash()
        {
            flashObject.gameObject.SetActive(true);
            flashTween?.Kill();
            flashTween = DOTween.Sequence()
                .Join(flashObject.DOFade(1, flashDuration).SetEase(flashEaseIn))
                .Append(flashObject.DOFade(0, flashDuration).SetEase(flashEaseOut))
                .OnComplete(() => flashObject.gameObject.SetActive(false));
        }
        
        
    }
}