using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class KnockoutSessionPanel : RaceSessionPanel
    {
        
        private KnockoutGameSession session => (KnockoutGameSession)GameSessionManager.Instance.CurrentSession;

        protected override int numberOfRacers => base.numberOfRacers - session.EliminatedRacers.Count;
        
    }
}
