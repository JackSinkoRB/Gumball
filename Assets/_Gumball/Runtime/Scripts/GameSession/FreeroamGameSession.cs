using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/GameSession/Freeroam")]
    public class FreeroamGameSession : GameSession
    {
        
        public override string GetModeDisplayName()
        {
            return "Freeroam";
        }

        public override Sprite GetModeIcon()
        {
            return null; //no icon exists yet
        }

        protected override GameSessionPanel GetSessionPanel()
        {
            return null; //has no UI
        }
        
        protected override SessionEndPanel GetSessionEndPanel()
        {
            return null; //has no end
        }

        public override ObjectiveUI.FakeChallengeData GetChallengeData()
        {
            return GameSessionManager.Instance.RacePositionChallengeData;
        }

        public override string GetMainObjectiveGoalValue()
        {
            return "";
        }
        
    }
}
