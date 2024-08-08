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
using Debug = UnityEngine.Debug;

namespace Gumball
{
    public class MapSceneManager : Singleton<MapSceneManager>
    {

        #region STATIC
        public static void LoadMapScene()
        {
            CoroutineHelper.Instance.StartCoroutine(LoadMapSceneIE());
        }
        
        private static IEnumerator LoadMapSceneIE()
        {
            PanelManager.GetPanel<LoadingPanel>().Show();

            Stopwatch sceneLoadingStopwatch = Stopwatch.StartNew();
            yield return Addressables.LoadSceneAsync(SceneManager.MapSceneAddress, LoadSceneMode.Single, true);
            sceneLoadingStopwatch.Stop();
            GlobalLoggers.LoadingLogger.Log($"{SceneManager.MapSceneAddress} loading complete in {sceneLoadingStopwatch.Elapsed.ToPrettyString(true)}");

            yield return Instance.LoadMaps();
            
            Instance.SelectMap(Instance.GetCurrentMap());
            
            PanelManager.GetPanel<LoadingPanel>().Hide();
        }
        #endregion

        [SerializeField, DisplayInspector] private GumballEvent currentEvent;
        
        [Header("Nodes")]
        [SerializeField] private float nodeFadeAmountWhenNotFocused = 0.2f;
        [SerializeField] private float nodeFadeDuration = 0.2f;

        [Header("Arrows")]
        [SerializeField] private Button leftArrow;
        [SerializeField] private Button rightArrow;
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private GameSessionMap[] mapInstances;
        [SerializeField, ReadOnly] private GameSessionMap selectedMap;

        private Sequence currentNodesTween;

        public GameSessionMap SelectedMap => selectedMap;
        
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

        private IEnumerator LoadMaps()
        {
            mapInstances = new GameSessionMap[currentEvent.Maps.Length];
            AsyncOperationHandle<GameObject>[] handles = new AsyncOperationHandle<GameObject>[currentEvent.Maps.Length];
            
            for (int index = 0; index < currentEvent.Maps.Length; index++)
            {
                int finalIndex = index;
                AssetReferenceGameObject mapReference = currentEvent.Maps[index];
                
                handles[index] = Addressables.LoadAssetAsync<GameObject>(mapReference);

                handles[index].Completed += h =>
                {
                    GameSessionMap map = Instantiate(h.Result).GetComponent<GameSessionMap>();
                    map.GetComponent<AddressableReleaseOnDestroy>(true).Init(handles[finalIndex]);
                    mapInstances[finalIndex] = map;
                    map.gameObject.SetActive(false); //start inactive until selected
                };
            }

            yield return new WaitUntil(() => handles.AreAllComplete());
        }

        public void SelectPreviousMap()
        {
            int currentIndex = mapInstances.IndexOfItem(selectedMap);
            int newIndex = currentIndex - 1;
            if (newIndex < 0)
            {
                Debug.LogWarning("No previous maps.");
                return;
            }

            GameSessionMap newMap = mapInstances[newIndex];
            SelectMap(newMap);
        }

        public void SelectNextMap()
        {
            int currentIndex = mapInstances.IndexOfItem(selectedMap);
            int newIndex = currentIndex + 1;
            if (newIndex >= mapInstances.Length)
            {
                Debug.LogWarning("No more maps.");
                return;
            }

            GameSessionMap newMap = mapInstances[newIndex];
            SelectMap(newMap);
        }
        
        public void SelectMap(GameSessionMap newMap)
        {
            //disable old map
            if (selectedMap != null)
                selectedMap.gameObject.SetActive(false);
            
            //enable new map
            selectedMap = newMap;
            selectedMap.gameObject.SetActive(true);
            
            int currentIndex = mapInstances.IndexOfItem(selectedMap);
            rightArrow.interactable = mapInstances.Length > 1 && currentIndex < mapInstances.Length - 1;
            leftArrow.interactable = mapInstances.Length > 1 && currentIndex > 0;
        }

        /// <summary>
        /// The current map is the furthest map in the map list with at least 1 node unlocked.
        /// </summary>
        private GameSessionMap GetCurrentMap()
        {
            foreach (GameSessionMap map in mapInstances)
            {
                if (!map.AllSessionsComplete())
                    return map;
            }

            return mapInstances[^1];
        }
        
        public void RemoveFocusOnNode()
        {
            if (selectedMap == null)
                return;
            
            currentNodesTween?.Kill();
            currentNodesTween = DOTween.Sequence();
            
            foreach (GameSessionNode node in selectedMap.Nodes)
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
        
        private void OnClickNode(GameSessionNode clickedNode)
        {
            clickedNode.OnClicked();

            currentNodesTween?.Kill();
            currentNodesTween = DOTween.Sequence();
            
            //fade all but this node
            foreach (GameSessionNode node in selectedMap.Nodes)
            {
                if (node == clickedNode)
                    continue;

                currentNodesTween.Join(node.GetComponent<CanvasGroup>(true).DOFade(nodeFadeAmountWhenNotFocused, nodeFadeDuration));
            }
        }

    }
}
