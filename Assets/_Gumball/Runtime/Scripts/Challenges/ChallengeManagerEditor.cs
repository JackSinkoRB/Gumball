using System.Collections;
using System.Collections.Generic;
using MyBox.Internal;
using UnityEditor;
using UnityEngine;

namespace Gumball
{
    [CustomEditor(typeof(ChallengeManager))]
    public class ChallengeManagerEditor : UnityObjectEditor
    {

        private ChallengeManager challengeManager => (ChallengeManager)target;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EnsureChallengesAreUnique(challengeManager.Daily);
            EnsureChallengesAreUnique(challengeManager.Weekly);
        }

        private void EnsureChallengesAreUnique(Challenges challenges)
        {
            HashSet<string> uniqueIDs = new HashSet<string>();

            foreach (Challenge challenge in challenges.ChallengePool)
            {
                while (uniqueIDs.Contains(challenge.UniqueID))
                    challenge.AssignNewID();
            
                uniqueIDs.Add(challenge.UniqueID);
            }
        
            EditorUtility.SetDirty(challengeManager);
        }
        
    }
}
