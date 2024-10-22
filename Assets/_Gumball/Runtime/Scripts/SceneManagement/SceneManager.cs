using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Gumball
{
    public class SceneManager : PersistentSingleton<SceneManager>
    {
        
        public const string BootSceneName = "BootScene";
        public const string MapSceneName = "MapScene";
        
        public const string BootSceneAddress = "Scenes/Loading/" + BootSceneName + ".unity";
        public const string GameLoaderSceneAddress = "Scenes/Loading/GameLoaderScene.unity";
        public const string MainSceneAddress = "Scenes/MainScene.unity";
        public const string AvatarEditorSceneAddress = "Scenes/AvatarEditor.unity";
        public const string MapSceneAddress = "Scenes/" + MapSceneName + ".unity";
        public const string WarehouseSceneAddress = "Scenes/Hanger.unity";

    }
}
