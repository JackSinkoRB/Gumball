using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class GameSessionAudioManager : Singleton<GameSessionAudioManager>
    {

        [Header("Speed camera sprint")]
        [SerializeField] private AudioSource playerFailSpeedCameraSprintZone;
        
        public AudioSource PlayerFailSpeedCameraSprintZone => playerFailSpeedCameraSprintZone;
        
    }
}
