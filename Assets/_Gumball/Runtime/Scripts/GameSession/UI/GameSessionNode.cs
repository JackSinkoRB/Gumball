using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Gumball
{
    public class GameSessionNode : MonoBehaviour
    {

        [SerializeField] private AssetReferenceT<GameSession> sessionAssetReference;
        [SerializeField] private TextMeshProUGUI typeLabel;

        private AsyncOperationHandle<GameSession> handle;

        public AssetReferenceT<GameSession> SessionAssetReference => sessionAssetReference;
        public GameSession GameSession { get; private set; }

        private void OnEnable()
        {
            TryLoadGameSessionAsset();
            
            typeLabel.text = GameSession.GetName();
        }

        public void OnClicked()
        {
            PanelManager.GetPanel<GameSessionNodePanel>().Show();
            PanelManager.GetPanel<GameSessionNodePanel>().Initialise(this);
        }

        private void TryLoadGameSessionAsset()
        {
            //just load the game sessions once and keep it loaded as these will have a neglible impact on memory
            if (GameSession != null)
                return; //already loaded

            handle = Addressables.LoadAssetAsync<GameSession>(sessionAssetReference);
            handle.WaitForCompletion();

            GameSession = handle.Result;
        }

    }
}
