using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class CarLevelUI : MonoBehaviour
    {
        
        [SerializeField] private AutosizeTextMeshPro label;
        
        private void OnEnable()
        {
            RefreshLabel();

            WarehouseManager.Instance.onCurrentCarChanged += OnCarChange;
        }

        private void OnDisable()
        {
            WarehouseManager.Instance.onCurrentCarChanged -= OnCarChange;
        }
        
        private void OnCarChange(AICar newcar)
        {
            RefreshLabel();
        }
        
        private void RefreshLabel()
        {
            if (WarehouseManager.Instance.CurrentCar == null)
            {
                label.text = "";
                return;
            }

            //TODO - display level once blueprints are in
            label.text = $"NA";
            this.PerformAtEndOfFrame(label.Resize);
        }

    }
}
