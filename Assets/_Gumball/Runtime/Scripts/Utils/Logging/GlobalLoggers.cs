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

        [SerializeField] private Logger loadingLogger;
        [SerializeField] private Logger chunkLogger;

    }
}
