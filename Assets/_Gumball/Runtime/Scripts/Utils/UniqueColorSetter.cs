using System;
using MyBox;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gumball
{
    [ExecuteAlways]
    public class UniqueColorSetter : MonoBehaviour
    {

        private static readonly int ColorSeed = Shader.PropertyToID("_ColorSeed");

        [SerializeField, ReadOnly] private Renderer[] renderersCached;

        private bool isInitialised;
        
        private void Start()
        {
            if (!isInitialised)
                Initialise();
            
            PickRandomColor();
        }

        private void Initialise()
        {
            isInitialised = true;
            renderersCached = GetComponentsInChildren<Renderer>(true);
        }

        private void PickRandomColor()
        {
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