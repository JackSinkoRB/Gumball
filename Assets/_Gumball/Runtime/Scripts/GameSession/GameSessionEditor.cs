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

#region STATIC
        public static void OnInspectorGUI(GameSession gameSession)
        {
            CacheBlueprintRewardsIfModified(gameSession);

            foreach (RacerSessionData data in gameSession.RacerData)
                data.CachePerformanceRatings();

            Challenge.EnsureChallengesAreUnique(gameSession.SubObjectives, gameSession);
        }
        
        private static void CacheBlueprintRewardsIfModified(GameSession gameSession)
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
#endregion
        
        private GameSession gameSession => (GameSession)target;
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            OnInspectorGUI(gameSession);
        }

    }
}
#endif