using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.Serialization;

namespace Gumball
{
    public class GameSessionManager : PersistentSingleton<GameSessionManager>
    {

        [SerializeField, ReadOnly] private GameSession currentSession;

        [Header("Challenge data for objectives")]
        [SerializeField] private ObjectiveUI.FakeChallengeData racePositionChallengeData;
        [SerializeField] private ObjectiveUI.FakeChallengeData timeChallengeData;

        public ObjectiveUI.FakeChallengeData RacePositionChallengeData => racePositionChallengeData;
        public ObjectiveUI.FakeChallengeData TimeChallengeData => timeChallengeData;
        
        public GameSession CurrentSession => currentSession;

        public void SetCurrentSession(GameSession session)
        {
            currentSession = session;
        }

        private void Update()
        {
            if (currentSession != null && currentSession.InProgress)
            {
                currentSession.UpdateWhenCurrent();
            }
        }
    }
}
