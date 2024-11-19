using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class SwapCorePartInstallButton : MonoBehaviour
    {

        [SerializeField] private AutosizeTextMeshPro label;
        [SerializeField] private GameObject costHolder;
        [SerializeField] private AutosizeTextMeshPro costLabel;
        
        private Button button => GetComponent<Button>();

        public void Initialise(CorePart.PartType type, CorePart part)
        {
            bool isSelected = CorePartManager.GetCorePart(WarehouseManager.Instance.CurrentCar.CarGUID, type) == part;
            if (isSelected || part == null)
            {
                label.alignment = TextAlignmentOptions.Center;
                label.text = "Installed";
                this.PerformAtEndOfFrame(label.Resize);
                button.interactable = false;

                costHolder.SetActive(false);
            }
            else
            {
                label.text = "Install";
                this.PerformAtEndOfFrame(label.Resize);
                button.interactable = WarehouseManager.Instance.CurrentCar.CarType == part.CarType;

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