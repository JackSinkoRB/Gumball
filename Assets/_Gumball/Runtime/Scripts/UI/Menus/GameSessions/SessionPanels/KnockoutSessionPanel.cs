using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class KnockoutSessionPanel : RaceSessionPanel
    {
        
        [Header("Knockout")]
        [SerializeField] private KnockoutPositionIcon knockoutPositionIconPrefab;
        [SerializeField] private Transform knockoutPositionIconHolder;

        [Header("Debugging")]
        [SerializeField] private KnockoutPositionIcon[] knockoutPositionIconInstances;

        public KnockoutPositionIcon[] KnockoutPositionIconInstances => knockoutPositionIconInstances;
        
        private KnockoutGameSession session => (KnockoutGameSession)GameSessionManager.Instance.CurrentSession;

        protected override int numberOfRacers => base.numberOfRacers - session.EliminatedRacers.Count;
        
        protected override void OnShow()
        {
            base.OnShow();
            
            PopulateKnockoutPositionIcons();
        }
        
        private void PopulateKnockoutPositionIcons()
        {
            knockoutPositionIconInstances = new KnockoutPositionIcon[session.KnockoutPositions.Length];

            for (int index = 0; index < session.KnockoutPositions.Length; index++)
            {
                float knockoutPosition = session.KnockoutPositions[index];
                
                //instantiate
                KnockoutPositionIcon instance = Instantiate(knockoutPositionIconPrefab.gameObject, knockoutPositionIconHolder).GetComponent<KnockoutPositionIcon>();
                knockoutPositionIconInstances[index] = instance;
                
                //set position
                RectTransform rectTransform = instance.GetComponent<RectTransform>();
                float percentage = Mathf.Clamp01(knockoutPosition / session.RaceDistanceMetres);
                rectTransform.pivot = rectTransform.pivot.SetX(percentage);
                rectTransform.anchoredPosition = Vector2.zero;
            }
        }

    }
}
