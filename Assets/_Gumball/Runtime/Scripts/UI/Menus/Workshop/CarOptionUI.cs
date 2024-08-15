using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class CarOptionUI : MonoBehaviour
    {

        [SerializeField] private TextMeshProUGUI nameLabel;
        [SerializeField] private Image icon;
        
        public void Initialise(WarehouseCarData carData)
        {
            nameLabel.text = carData.DisplayName;
            
            icon.sprite = carData.Icon;
            icon.gameObject.SetActive(icon.sprite != null);
        }
        
    }
}
