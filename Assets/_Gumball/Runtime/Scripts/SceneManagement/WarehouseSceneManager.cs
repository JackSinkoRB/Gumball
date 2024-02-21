using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using MyBox;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace Gumball
{
    public class WarehouseSceneManager : Singleton<WarehouseSceneManager>
    {
        
        #region STATIC
        public static void LoadWarehouse()
        {
            CoroutineHelper.Instance.StartCoroutine(LoadWarehouseIE());
        }
        
        private static IEnumerator LoadWarehouseIE()
        {
            PanelManager.GetPanel<LoadingPanel>().Show();

            Stopwatch sceneLoadingStopwatch = Stopwatch.StartNew();
            yield return Addressables.LoadSceneAsync(SceneManager.WarehouseSceneName, LoadSceneMode.Single, true);
            sceneLoadingStopwatch.Stop();
            GlobalLoggers.LoadingLogger.Log($"{SceneManager.WarehouseSceneName} loading complete in {sceneLoadingStopwatch.Elapsed.ToPrettyString(true)}");
            
            AvatarManager.Instance.HideAvatars(true);

            yield return Instance.PopulateSlots();
            Instance.SelectSlot(0); //select the first slot
            
            PanelManager.GetPanel<LoadingPanel>().Hide();
        }
        #endregion

        public event Action<WarehouseCarSlot> onSelectSlot;
        
        [SerializeField] private WarehouseCarSlot[] carSlots;
        [SerializeField, ReadOnly] private WarehouseCarSlot currentSelectedSlot;

        public WarehouseCarSlot[] CarSlots => carSlots;

        public void SelectSlot(WarehouseCarSlot slot)
        {
            currentSelectedSlot = slot;

            onSelectSlot?.Invoke(slot);
        }
        
        public void SelectSlot(int slotIndex) => SelectSlot(carSlots[slotIndex]);

        public IEnumerator PopulateSlots()
        {
            int index = 0;
            HashSet<TrackedCoroutine> slotHandles = new HashSet<TrackedCoroutine>();
            foreach (WarehouseCarSlot slot in carSlots)
            {
                if (index >= WarehouseManager.Instance.AllCars.Count)
                    break; //not enough cars

                if (index == WarehouseManager.Instance.CurrentCar.CarIndex)
                    slot.PopulateWithCar(WarehouseManager.Instance.CurrentCar); //can reuse the car
                else
                    slotHandles.Add(new TrackedCoroutine(slot.PopulateWithCar(index, 0))); //spawn new car
                
                index++;
            }

            yield return new WaitUntil(slotHandles.AreAllComplete);
        }
        
    }
}
