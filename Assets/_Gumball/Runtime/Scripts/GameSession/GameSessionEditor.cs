#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using MyBox.Internal;
using UnityEditor;
using UnityEngine;

namespace Gumball.Editor
{
    [CustomEditor(typeof(GameSession), editorForChildClasses: true)]
    public class GameSessionEditor : UnityObjectEditor
    {
        
        private GameSession gameSession => (GameSession)target;
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            CacheBlueprintRewardsIfModified();

            foreach (RacerSessionData data in gameSession.RacerData)
                data.CachePerformanceRatings();
        }
        
        private void CacheBlueprintRewardsIfModified()
        {
            string currentBlueprintRewards = string.Join(", ", gameSession.Rewards.Blueprints);
            bool blueprintsHaveChanged = !currentBlueprintRewards.Equals(gameSession.PreviousBlueprintRewards);
            if (blueprintsHaveChanged)
            {
                BlueprintManager.Instance.CacheSessionsThatGiveBlueprints();
                gameSession.PreviousBlueprintRewards = currentBlueprintRewards;
                EditorUtility.SetDirty(gameSession);
            }
        }
        
    }
}
#endif