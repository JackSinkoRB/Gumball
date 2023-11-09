using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class DecalEditorPanel : AnimatedPanel
    {

        [Header("Decal editor panel")]
        [SerializeField] private DecalLayerSelector layerSelector;

        public DecalLayerSelector LayerSelector => layerSelector;

    }
}
