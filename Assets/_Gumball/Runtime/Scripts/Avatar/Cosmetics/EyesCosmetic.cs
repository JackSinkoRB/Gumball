using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class EyesCosmetic : BlendShapeCosmetic
    {

        [ButtonMethod]
        public void Test()
        {
            List<BlendShapeOption> newOptions = new();
            for (int blendShapeIndex = 0; blendShapeIndex < 9; blendShapeIndex++)
            {
                BlendShapeOption newOption = new BlendShapeOption(Options[0]);
                newOption.PropertyModifiers[blendShapeIndex].value = 1;
                newOptions.Add(newOption);
            }

            options = newOptions.ToArray();
        }
        
    }
}
