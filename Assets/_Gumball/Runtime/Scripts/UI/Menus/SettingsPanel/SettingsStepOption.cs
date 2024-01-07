using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Gumball
{
    public class SettingsStepOption : MonoBehaviour
    {

        [SerializeField] private string label;

        [SerializeField] private GameObject deselected;
        [SerializeField] private GameObject selected;

        [SerializeField] private UnityEvent onSelected;
        [SerializeField] private UnityEvent onDeselected;

        public string Label => label;
        
        public void OnSelected()
        {
            onSelected?.Invoke();
            
            selected.gameObject.SetActive(true);
            deselected.gameObject.SetActive(false);
        }

        public void OnDeselected()
        {
            onDeselected?.Invoke();
            
            selected.gameObject.SetActive(false);
            deselected.gameObject.SetActive(true);
        }
        
    }
}
