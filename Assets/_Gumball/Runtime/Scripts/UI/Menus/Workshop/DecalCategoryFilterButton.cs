using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class DecalCategoryFilterButton : MonoBehaviour
    {

        [SerializeField] private AutosizeTextMeshPro label;

        public void Initialise(DecalUICategory category)
        {
            label.text = category.CategoryName;
            this.PerformAtEndOfFrame(label.Resize);
        }

    }
}
