using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class AvatarDynamicState : DynamicState
    {

        protected new AvatarStateManager manager => base.manager as AvatarStateManager;
        protected Avatar avatar => manager.Avatar;

    }
}
