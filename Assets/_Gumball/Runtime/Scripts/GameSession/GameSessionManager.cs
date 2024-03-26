using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class GameSessionManager : PersistentSingleton<GameSessionManager>
    {

        [SerializeField, ReadOnly] private GameSession currentSession;

        public GameSession CurrentSession => currentSession;

        public void SetCurrentSession(GameSession session)
        {
            currentSession = session;
        }
        
    }
}
