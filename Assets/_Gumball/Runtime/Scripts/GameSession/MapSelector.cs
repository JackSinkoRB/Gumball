using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/Singletons/Map Selector")]
    public class MapSelector : SingletonScriptable<MapSelector>
    {
        
        public GumballEvent LastPlayedEvent
        {
            get => allEvents[DataManager.GameSessions.Get(lastPlayedEventSaveKey, defaultEventIndex)];
            set => DataManager.GameSessions.Set(lastPlayedEventSaveKey, allEvents.IndexOfItem(value));
        }
        
        public int LastPlayedMapIndex
        {
            get => DataManager.GameSessions.Get(lastPlayedMapSaveKey, 0);
            set => DataManager.GameSessions.Set(lastPlayedMapSaveKey, value);
        }
        
        private const string lastPlayedEventSaveKey = "CurrentEvent";
        private const string lastPlayedMapSaveKey = "CurrentMap";

        public IEnumerator LoadLastPlayedMap()
        {
            yield return LoadMap(LastPlayedEvent, LastPlayedMapIndex);
        }
        
        public IEnumerator LoadMap(GumballEvent gumballEvent, int mapIndex)
        {
            if (mapIndex >= gumballEvent.Maps.Length || mapIndex < 0)
                throw new IndexOutOfRangeException($"Event '{name}' doesn't have a map at index {mapIndex}.");

            if (currentSelectedMap != null)
                Destroy(currentSelectedMap);
            
            AssetReferenceGameObject assetReference = gumballEvent.Maps[mapIndex];
            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(assetReference);
            yield return handle;
            
            GameSessionMap map = Instantiate(handle.Result).GetComponent<GameSessionMap>();
            map.GetComponent<AddressableReleaseOnDestroy>(true).Init(handle);
            
            currentSelectedEvent = gumballEvent;
            currentSelectedMap = map;
        }
        
    }
}
