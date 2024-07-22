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
            
            Instance.SelectMap(Instance.GetCurrentMapIndex());
            
            PanelManager.GetPanel<LoadingPanel>().Hide();
        }
        #endregion

        [SerializeField, DisplayInspector] private GumballEvent currentEvent;
        
        [Header("Nodes")]
        [SerializeField] private float nodeFadeAmountWhenNotFocused = 0.2f;
        [SerializeField] private float nodeFadeDuration = 0.2f;
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private GameSessionMap[] mapInstances;
        [SerializeField, ReadOnly] private GameSessionMap selectedMap;

        private Sequence currentNodesTween;
        
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
                    map.GetComponent<AddressableReleaseOnDestroy>(true).Init(h);
                    mapInstances[finalIndex] = map;
                };
            }

            yield return new WaitUntil(handles.AreAllComplete());
        }
        
        public void SelectMap(int mapIndex)
        {
            if (mapIndex >= currentEvent.Maps.Length || mapIndex < 0)
                throw new IndexOutOfRangeException($"Event '{name}' doesn't have a map at index {mapIndex}.");

            //disable old map
            if (selectedMap != null)
                selectedMap.gameObject.SetActive(false);
            
            //enable new map
            selectedMap = mapInstances[mapIndex];
            selectedMap.gameObject.SetActive(true);
        }

        /// <summary>
        /// The current map is the furthest map in the map list with at least 1 node unlocked.
        /// </summary>
        private int GetCurrentMapIndex()
        {
            foreach (var map in currentEvent.Maps)
            {
                
            }

            return 0; //TODO
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
