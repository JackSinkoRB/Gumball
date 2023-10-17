using System.Collections.Generic;
using UnityEngine;

namespace CC
{
    [CreateAssetMenu(fileName = "Apparel", menuName = "ScriptableObjects/Apparel")]
    public class scrObj_Apparel : ScriptableObject
    {
        [System.Serializable]
        public struct Apparel
        {
            public GameObject Mesh;
            public string Name;
            public bool AddCopyPoseScript;
            public Texture2D Mask;
            public FootOffset FootOffset;
            public bool AffectFootOffset;
            public Sprite Icon;
        }

        public List<Apparel> Items = new List<Apparel>();
        public string MaskProperty;
    }
}