using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class MainMenuPanel : AnimatedPanel
    {

        [SerializeField] private Transform challengesNotification;
        
        protected override void OnShow()
        {
            base.OnShow();

            RefreshChallengesNotification();
        }

        public void OnClickChallengesButton()
        {
            if (PlayFabManager.ServerTimeInitialisationStatus != PlayFabManager.ConnectionStatusType.SUCCESS)
            {
                //attempt server time sync
                PlayFabManager.AttemptReconnection(() => PanelManager.GetPanel<ChallengesPanel>().Show(),
                    () =>
                    {
                        PanelManager.GetPanel<GenericMessagePanel>().Show();
                        PanelManager.GetPanel<GenericMessagePanel>().Initialise("Challenges require an internet connection.");
                    });
                
                return;
            }
            
            PanelManager.GetPanel<ChallengesPanel>().Show();
        }

        public void OnClickWardrobeButton()
        {
            AvatarEditor.LoadEditor();
        }

        public void OnClickGarageButton()
        {
            WarehouseSceneManager.LoadWarehouse();
        }

        public void OnClickStoreButton()
        {
            PanelManager.GetPanel<StorePanel>().Show();
        }

        private void RefreshChallengesNotification()
        {
            if (PlayFabManager.ServerTimeInitialisationStatus != PlayFabManager.ConnectionStatusType.SUCCESS)
            {
                //requires internet
                challengesNotification.gameObject.SetActive(false);
                return;
            }
            
            challengesNotification.gameObject.SetActive(CanShowChallengesNotification());
        }

        private bool CanShowChallengesNotification()
        {
            //if daily login is ready
            int currentDayNumber = DailyLoginManager.Instance.GetCurrentDayNumber();
            bool isDayWaiting = DailyLoginManager.Instance.IsDayReady(currentDayNumber) && !DailyLoginManager.Instance.IsDayClaimed(currentDayNumber);
            if (isDayWaiting)
                return true;

            if (ChallengeManager.Instance.Daily.AreRewardsReadyToBeClaimed())
                return true;
            
            if (ChallengeManager.Instance.Weekly.AreRewardsReadyToBeClaimed())
                return true;
            
            return false;
        }

    }
}
