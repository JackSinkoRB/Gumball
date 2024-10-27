using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class LevelUpWorkshopSubMenu : WorkshopSubMenu
    {

        [SerializeField] private OpenBlueprintOption openBlueprintOptionPrefab;
        [SerializeField] private Transform openBlueprintOptionHolder;

        protected override void OnShow()
        {
            base.OnShow();

            PopulateOpenBlueprints();
        }

        private void PopulateOpenBlueprints()
        {
            foreach (Transform child in openBlueprintOptionHolder)
                child.gameObject.Pool();
            
            //get the sessions that give the CarIndex blueprint as a reward
            foreach (GameSession session in BlueprintManager.Instance.GetSessionsThatGiveBlueprint(WarehouseManager.Instance.CurrentCar.CarIndex))
            {
                OpenBlueprintOption instance = openBlueprintOptionPrefab.gameObject.GetSpareOrCreate<OpenBlueprintOption>(openBlueprintOptionHolder);
                instance.transform.SetAsLastSibling();
                instance.Initialise(session.GetModeIcon(), session.DisplayName);
            }
        }
        
    }
}
