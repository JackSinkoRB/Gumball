#if UNITY_EDITOR
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

            Challenge.EnsureChallengesAreUnique(challengeManager.Daily.ChallengePool, challengeManager);
            Challenge.EnsureChallengesAreUnique(challengeManager.Weekly.ChallengePool, challengeManager);
        }

    }
}
#endif