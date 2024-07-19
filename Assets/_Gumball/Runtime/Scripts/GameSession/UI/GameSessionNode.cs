using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Gumball
{
    public class GameSessionNode : MonoBehaviour
    {

        [Obsolete("Old reference, use gameSession. This will be removed.")]
        [SerializeField, HideInInspector] private AssetReferenceT<GameSession> sessionAssetReference;
        [SerializeField, DisplayInspector] private GameSession gameSession;
        
        [Header("UI")]
        [SerializeField] private TextMeshProUGUI typeLabel;

        private AsyncOperationHandle<GameSession> handle;

        public GameSession GameSession => gameSession;

#if UNITY_EDITOR
        private void OnValidate()
        {
            //TODO: temp - remove when sessionAssetReferenceIsRemoved
            if (gameSession == null)
                gameSession = sessionAssetReference.editorAsset;
            
            UpdateInspectorName();
        }

        private void UpdateInspectorName()
        {
            string nodeName = "GameSessionNode";
            if (gameSession != null)
            {
                nodeName = $"{gameSession.GetName()} - {gameSession.name}";
                AssetReferenceT<ChunkMap> chunkMap = gameSession.ChunkMapAssetReference;
                if (chunkMap != null && chunkMap.editorAsset != null)
                    nodeName += $" - {chunkMap.editorAsset.name}";
            }
            gameObject.name = nodeName;
            EditorUtility.SetDirty(gameObject);
        }
#endif
        
        private void OnEnable()
        {
            typeLabel.text = GameSession.GetName();
        }

        public void OnClicked()
        {
            PanelManager.GetPanel<GameSessionNodePanel>().Show();
            PanelManager.GetPanel<GameSessionNodePanel>().Initialise(this);
        }
        
    }
}
