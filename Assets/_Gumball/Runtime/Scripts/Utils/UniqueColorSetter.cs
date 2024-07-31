using System;
using MyBox;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gumball
{
    [ExecuteAlways]
    public class UniqueColorSetter : MonoBehaviour, ISerializationCallbackReceiver
    {

        private static readonly int ColorSeed = Shader.PropertyToID("_ColorSeed");

        [SerializeField, ReadOnly] private Renderer[] renderersCached;

        private void Start()
        {
            PickRandomColor();
        }

        public void OnBeforeSerialize()
        {
            //cache the renderers
            renderersCached = GetComponentsInChildren<Renderer>(true);
        }

        public void OnAfterDeserialize()
        {
            
        }

        private void PickRandomColor()
        {
            if (renderersCached == null || renderersCached.Length == 0)
            {
                //have to find them at runtime
                renderersCached = GetComponentsInChildren<Renderer>(true);
            }
            
            //apply the seed to all child renderers
            float seed = Random.Range(1, 50);
            foreach (Renderer rend in renderersCached)
            {
                MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
                rend.GetPropertyBlock(propBlock);
                propBlock.SetFloat(ColorSeed, seed);
                rend.SetPropertyBlock(propBlock);
            }
        }
        
    }
}