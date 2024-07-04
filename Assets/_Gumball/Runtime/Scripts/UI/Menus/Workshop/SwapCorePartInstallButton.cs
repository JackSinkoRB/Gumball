using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class SwapCorePartInstallButton : MonoBehaviour
    {

        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private GameObject costHolder;
        [SerializeField] private TextMeshProUGUI costLabel;
        
        private Button button => GetComponent<Button>();

        public void Initialise(CorePart.PartType type, CorePart part)
        {
            bool isSelected = PartModification.GetCorePart(WarehouseManager.Instance.CurrentCar.CarIndex, type) == part;
            if (isSelected)
            {
                label.alignment = TextAlignmentOptions.Center;
                label.text = "Installed";
                button.interactable = false;

                costHolder.SetActive(false);
            }
            else
            {
                label.text = "Install";
                button.interactable = true;

                bool isStockPart = part == null;
                if (isStockPart)
                {
                    //free
                    label.alignment = TextAlignmentOptions.Center;
                    costHolder.SetActive(false);
                }
                else
                {
                    label.alignment = TextAlignmentOptions.Right;
                    costHolder.SetActive(true);
                    costLabel.text = part.StandardCurrencyInstallCost.ToString();
                }
            }
        }

    }
}
