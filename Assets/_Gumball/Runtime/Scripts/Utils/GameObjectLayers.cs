using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public static class GameObjectLayers
    {

        public enum Layer
        {
            Terrain = 7
        }

        public static LayerMask GetLayerMaskFromLayer(Layer layer)
        {
            return 1 << (int)layer;
        }
        
        public static LayerMask GetLayerMaskFromLayers(IEnumerable<Layer> layers)
        {
            int layerMask = 0;

            // Iterate through the layerIDs and set the corresponding bits in the layerMask
            foreach (Layer layer in layers)
            {
                int layerID = (int)layer;
                layerMask |= 1 << layerID;
            }
            
            return layerMask;
        }
        
    }
}
