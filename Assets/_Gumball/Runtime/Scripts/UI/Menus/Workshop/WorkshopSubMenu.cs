using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class WorkshopSubMenu : MonoBehaviour
    {

        public bool IsShowing => gameObject.activeSelf;

        public virtual void Show()
        {
            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }
        
    }
}
