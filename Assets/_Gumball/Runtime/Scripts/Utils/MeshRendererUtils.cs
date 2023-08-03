using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public static class MeshRendererUtils
    {

        public static bool HasValidMaterials(this Material[] materials)
        {
            if (materials == null)
                return false;

            if (materials.Length == 0)
                return false;

            if (!HasValidMaterialInArray(materials))
                return false;

            return true;
        }

        private static bool HasValidMaterialInArray(Material[] materials)
        {
            foreach (Material material in materials)
            {
                if (material != null)
                    return true;
            }

            return false;
        }
        
    }
}
