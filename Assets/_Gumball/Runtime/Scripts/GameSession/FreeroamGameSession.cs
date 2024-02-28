using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/GameSession/Freeroam")]
    public class FreeroamGameSession : GameSession
    {
        
        public override string GetName()
        {
            return "Freeroam";
        }
        
    }
}
