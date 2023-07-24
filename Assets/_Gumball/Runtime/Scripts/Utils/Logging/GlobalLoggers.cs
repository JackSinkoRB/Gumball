using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [CreateAssetMenu(menuName = "GlobalLoggers")]
    public class GlobalLoggers : SingletonScriptable<GlobalLoggers>
    {
        
        public static Logger LoadingLogger => Instance.loadingLogger;

        [SerializeField] private Logger loadingLogger;

    }
}
