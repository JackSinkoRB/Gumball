using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Logging/GlobalLoggers")]
    public class GlobalLoggers : SingletonScriptable<GlobalLoggers>
    {
        
        public static Logger LoadingLogger => Instance.loadingLogger;
        public static Logger ChunkLogger => Instance.chunkLogger;
        public static Logger InputLogger => Instance.inputLogger;
        public static Logger SaveDataLogger => Instance.saveDataLogger;
        public static Logger PanelLogger => Instance.panelLogger;
        public static Logger ObjectPoolLogger => Instance.objectPoolLogger;
        public static Logger DecalsLogger => Instance.decalsLogger;
        public static Logger TrafficLogger => Instance.trafficLogger;
        public static Logger AvatarLogger => Instance.avatarLogger;

        [SerializeField] private Logger loadingLogger;
        [SerializeField] private Logger chunkLogger;
        [SerializeField] private Logger inputLogger;
        [SerializeField] private Logger saveDataLogger;
        [SerializeField] private Logger panelLogger;
        [SerializeField] private Logger objectPoolLogger;
        [SerializeField] private Logger decalsLogger;
        [SerializeField] private Logger trafficLogger;
        [SerializeField] private Logger avatarLogger;

    }
}
