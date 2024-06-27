using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class AvatarDynamicState : DynamicState
    {

        protected AvatarStateManager avatarStateManager => Manager as AvatarStateManager;
        protected Avatar avatar => avatarStateManager.Avatar;

    }
}
