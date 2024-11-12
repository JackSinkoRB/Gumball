using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class SwapCorePartFilterButton : MonoBehaviour
    {

        [SerializeField] private AutosizeTextMeshPro label;

        public void Initialise(CarType type)
        {
            label.text = type.ToString();
            this.PerformAtEndOfFrame(label.Resize);
        }

    }
}
