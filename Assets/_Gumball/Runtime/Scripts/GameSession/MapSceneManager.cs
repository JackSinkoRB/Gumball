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
            yield return Addressables.LoadSceneAsync(SceneManager.MapSceneAddress, LoadSceneMode.Single, true);
            sceneLoadingStopwatch.Stop();
            GlobalLoggers.LoadingLogger.Log($"{SceneManager.MapSceneAddress} loading complete in {sceneLoadingStopwatch.Elapsed.ToPrettyString(true)}");
            
            yield return MapSelector.Instance.LoadLastPlayedMap();
            
            PanelManager.GetPanel<LoadingPanel>().Hide();
        }

        [SerializeField] private float nodeFadeAmountWhenNotFocused = 0.2f;
        [SerializeField] private float nodeFadeDuration = 0.2f;
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private GameSessionMap currentSelectedMap;

        private Sequence currentNodesTween;
        
        public GameSessionMap CurrentSelectedMap => currentSelectedMap;

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
