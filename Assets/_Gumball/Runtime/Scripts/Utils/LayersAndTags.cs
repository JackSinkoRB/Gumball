using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public static class LayersAndTags
    {

        public enum Layer
        {
            LiveDecal = 6,
            Terrain = 7,
            PlayerVehicle = 8,
            TrafficCar = 9,
            ChunkObject = 10
        }
        
        public static LayerMask TrafficCarCollisionLayers = 1 << (int)Layer.TrafficCar | 1 << (int)Layer.PlayerVehicle;

        public static bool ContainsLayer(this LayerMask layerMask, int layer)
        {
            return layerMask == (layerMask | (1 << layer));
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
