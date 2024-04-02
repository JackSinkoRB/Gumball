using UnityEngine;

namespace Gumball
{
    public class UniqueColorSetter : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            // Generate a random seed between 1 and 50
            float seed = UnityEngine.Random.Range(1f, 50f);

            // Apply the seed to all child renderers
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            foreach (Renderer rend in renderers)
            {
                MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
                rend.GetPropertyBlock(propBlock);
                propBlock.SetFloat("_ColorSeed", seed);
                rend.SetPropertyBlock(propBlock);
            }
        }
    }
}