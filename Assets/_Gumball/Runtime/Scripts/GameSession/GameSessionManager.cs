using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class GameSessionManager : Singleton<GameSessionManager>
    {

        [SerializeField, ReadOnly] private GameSession currentSession;

    }
}
