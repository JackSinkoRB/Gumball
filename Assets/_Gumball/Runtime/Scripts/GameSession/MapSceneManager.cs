using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using DG.Tweening;
using MyBox;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Gumball
{
    public class MapSceneManager : Singleton<MapSceneManager>
    {

        public static void LoadMapScene()
        {
            CoroutineHelper.Instance.StartCoroutine(LoadMapSceneIE());
        }
        
        private static IEnumerator LoadMapSceneIE()
        {
            PanelManager.GetPanel<LoadingPanel>().Show();

            Stopwatch sceneLoadingStopwatch = Stopwatch.StartNew();
            yield return Addressables.LoadSceneAsync(SceneManager.MapSceneName, LoadSceneMode.Single, true);
            sceneLoadingStopwatch.Stop();
            GlobalLoggers.LoadingLogger.Log($"{SceneManager.MapSceneName} loading complete in {sceneLoadingStopwatch.Elapsed.ToPrettyString(true)}");
            
            yield return Instance.LoadLastPlayedMap();
            
            PanelManager.GetPanel<LoadingPanel>().Hide();
        }
        
        private const string lastPlayedEventSaveKey = "CurrentEvent";
        private const string lastPlayedMapSaveKey = "CurrentMap";

        [Tooltip("The event that will load into the map if playing for the first time.")]
        [SerializeField] private int defaultEventIndex;
        [Tooltip("A collection of all the Gumball events that the player can see.")]
        [SerializeField, DisplayInspector] private GumballEvent[] allEvents;

        [SerializeField] private float nodeFadeAmountWhenNotFocused = 0.2f;
        [SerializeField] private float nodeFadeDuration = 0.2f;
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private GumballEvent currentSelectedEvent;
        //TODO: make readonly after testing
        [SerializeField] private GameSessionMap currentSelectedMap;

        public GameSessionMap CurrentSelectedMap => currentSelectedMap;
        
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

        protected override void Initialise()
        {
            base.Initialise();
            
            PrimaryContactInput.onRelease += OnPrimaryContactRelease;
            
            if (WarehouseManager.Instance.CurrentCar != null)
                WarehouseManager.Instance.CurrentCar.gameObject.SetActive(false);
            
            AvatarManager.Instance.HideAvatars(true);
        }

        protected override void OnInstanceDisabled()
        {
            base.OnInstanceDisabled();
            
            PrimaryContactInput.onRelease -= OnPrimaryContactRelease;
        }

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
        
        public void RemoveFocusOnNode()
        {
            if (currentSelectedMap == null)
                return;
            
            currentNodesTween?.Kill();
            currentNodesTween = DOTween.Sequence();
            
            foreach (GameSessionNode node in currentSelectedMap.Nodes)
            {
                currentNodesTween.Join(node.GetComponent<CanvasGroup>(true).DOFade(1, nodeFadeDuration));
            }
        }
        
        private void OnPrimaryContactRelease()
        {
            bool positionHasMoved = !PrimaryContactInput.OffsetSincePressedNormalised.Approximately(Vector2.zero, PrimaryContactInput.PressedThreshold);
            if (!positionHasMoved)
                CheckIfClickedSessionNode();
        }
        
        private void CheckIfClickedSessionNode()
        {
            foreach (Graphic graphic in PrimaryContactInput.GetClickableGraphicsUnderPointer())
            {
                GameSessionNode nodeOnObject = graphic.GetComponent<GameSessionNode>();
                if (nodeOnObject != null)
                {
                    OnClickNode(nodeOnObject);
                    return;
                }

                //check parents
                GameSessionNode nodeInParent = graphic.transform.GetComponentInAllParents<GameSessionNode>();
                if (nodeInParent != null)
                {
                    OnClickNode(nodeInParent);
                    return;
                }
            }
        }

        private Sequence currentNodesTween;
        
        private void OnClickNode(GameSessionNode clickedNode)
        {
            clickedNode.OnClicked();

            currentNodesTween?.Kill();
            currentNodesTween = DOTween.Sequence();
            
            //fade all but this node
            foreach (GameSessionNode node in currentSelectedMap.Nodes)
            {
                if (node == clickedNode)
                    continue;

                currentNodesTween.Join(node.GetComponent<CanvasGroup>(true).DOFade(nodeFadeAmountWhenNotFocused, nodeFadeDuration));
            }
        }

    }
}
