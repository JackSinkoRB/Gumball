using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Gumball
{
    public class SceneManager : PersistentSingleton<SceneManager>
    {
        
        public const string BootSceneName = "BootScene";
        public const string GameLoaderSceneName = "GameLoaderScene";
        public const string MainSceneName = "MainScene";
        public const string MapDrivingSceneName = "MapDrivingScene";
        public const string DecalEditorSceneName = "DecalEditor";
        public const string AvatarEditorSceneName = "AvatarEditor";

    }
}
