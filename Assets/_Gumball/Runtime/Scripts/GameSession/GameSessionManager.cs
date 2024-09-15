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

        [Header("Mode icons")]
        [SerializeField] private Sprite raceIcon;
        [SerializeField] private Sprite timedIcon;
        [SerializeField] private Sprite speedCameraSprintIcon;
        [SerializeField] private Sprite knockoutIcon;
        [SerializeField] private Sprite cleanRunIcon;
        
        [Header("Challenge data for objectives")]
        [SerializeField] private ObjectiveUI.FakeChallengeData racePositionChallengeData;
        [SerializeField] private ObjectiveUI.FakeChallengeData timeChallengeData;

        public ObjectiveUI.FakeChallengeData RacePositionChallengeData => racePositionChallengeData;
        public ObjectiveUI.FakeChallengeData TimeChallengeData => timeChallengeData;
        
        public GameSession CurrentSession => currentSession;
        
        public Sprite RaceIcon => raceIcon;
        public Sprite TimedIcon => timedIcon;
        public Sprite SpeedCameraSprintIcon => speedCameraSprintIcon;
        public Sprite KnockoutIcon => knockoutIcon;
        public Sprite CleanRunIcon => cleanRunIcon;

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

        public void RestartCurrentSession()
        {
            if (currentSession == null)
                throw new NullReferenceException("Cannot restart session because there is none current.");

            GameSession session = CurrentSession;
            if (session.InProgress)
                session.EndSession(GameSession.ProgressStatus.NOT_ATTEMPTED);
            session.UnloadSession();
            session.StartSession();
        }
    }
}
