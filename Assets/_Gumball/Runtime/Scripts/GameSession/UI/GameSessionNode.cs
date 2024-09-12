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
using UnityEngine.UI;

namespace Gumball
{
    public class GameSessionNode : MonoBehaviour
    {

        [Header("Unlock requirements")]
        [HelpBox("Assign either a session node (from this map), or game session (from another map).", MessageType.Info, HelpBoxAttribute.Position.ABOVE)]
        [SerializeField] private GameSessionNode requiredSessionNode;
        [SerializeField] private GameSession requiredSession;
        [Space(5)]
        [HelpBox("Sessions that require fans won't show on the map until the player has enough fans.", MessageType.Info, HelpBoxAttribute.Position.ABOVE, true, true)]
        [SerializeField] private bool requireFans;
        [SerializeField, ConditionalField(nameof(requireFans))] private int fansRequired;
        
        [Obsolete("Old reference, use gameSession. This will be removed.")]
        [SerializeField, HideInInspector] private AssetReferenceT<GameSession> sessionAssetReference;
        [Header("Game session")]
        [SerializeField, DisplayInspector] private GameSession gameSession;
        
        [Header("UI")]
        [SerializeField] private Image modeIcon;
        [SerializeField] private Transform lockObject;

        public bool IsUnlocked {
            get
            {
                if (requiredSessionNode != null && requiredSessionNode.gameSession.Progress != GameSession.ProgressStatus.COMPLETE)
                    return false;
                
                if (requiredSession != null && requiredSession.Progress != GameSession.ProgressStatus.COMPLETE)
                    return false;

                if (requireFans && FollowersManager.CurrentFollowers < fansRequired)
                    return false;

                return true;
            }
        }
        
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
                nodeName = $"{gameSession.GetModeDisplayName()} - {gameSession.name}";
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
            modeIcon.sprite = gameSession.GetModeIcon();
            
            lockObject.gameObject.SetActive(!IsUnlocked);
            
            //nodes that require followers don't show on the map until there's enough followers
            if (requireFans && FollowersManager.CurrentFollowers < fansRequired)
                gameObject.SetActive(false);
        }

        public void OnClicked()
        {
            PanelManager.GetPanel<GameSessionNodePanel>().Show();
            PanelManager.GetPanel<GameSessionNodePanel>().Initialise(this);
        }
        
    }
}
