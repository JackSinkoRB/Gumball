using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class AvatarStandingIdleState : AvatarDynamicState
    {
        
        public override void OnSetCurrent()
        {
            base.OnSetCurrent();
            
            //remove parented
            avatar.transform.SetParent(null);
            
            avatar.CurrentBody.TransformBone.enabled = true;
        }
        
    }
}
