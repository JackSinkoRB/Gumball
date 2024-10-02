using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/Singletons/Decal Manager")]
    public class DecalManager : SingletonScriptable<DecalManager>
    {
        
        [SerializeField] private LiveDecal liveDecalPrefab;
        [SerializeField] private DecalUICategory[] decalUICategories;

        public DecalUICategory[] DecalUICategories => decalUICategories;

        public static LiveDecal CreateLiveDecal(DecalUICategory category, DecalTexture decalTexture, int priority = -1)
        {
            LiveDecal liveDecal = Instance.liveDecalPrefab.gameObject.GetSpareOrCreate<LiveDecal>();
            DontDestroyOnLoad(liveDecal);

            liveDecal.transform.SetParent(DecalEditor.Instance.CurrentCar.transform);
            
            liveDecal.SetPriority(priority);
            liveDecal.Initialise(decalTexture,
                Array.IndexOf(Instance.decalUICategories, category), 
                Array.IndexOf(category.DecalTextures, decalTexture));
            
            return liveDecal;
        }

        /// <summary>
        /// Saves the specified decals to the car's save data.
        /// </summary>
        public static void SaveLiveDecalData(AICar car, List<LiveDecal> liveDecals)
        {
            LiveDecalData[] liveDecalData = CreateLiveDecalData(liveDecals);
            DataManager.Cars.Set(GetDecalsSaveKey(car), liveDecalData);
            GlobalLoggers.DecalsLogger.Log($"Saving {liveDecals.Count} live decals for {car.gameObject.name}.");
        }

        /// <summary>
        /// Loads and applies the decals from the car's save data.
        /// </summary>
        public static IEnumerator ApplyDecalDataToCar(AICar car)
        {
            if (!DecalEditor.ExistsRuntime)
            {
                Debug.LogError("Could not apply decals as the decal editor doesn't exist.");
                yield break;
            }

            DecalEditor.Instance.StartSession(car);
            yield return DecalEditor.Instance.EndSession();
        }

        public static LiveDecalData[] GetSavedDecalData(AICar car)
        {
            LiveDecalData[] liveDecalData = DataManager.Cars.Get(GetDecalsSaveKey(car), Array.Empty<LiveDecalData>());
            return liveDecalData;
        }
        
        /// <summary>
        /// Creates a list of live decals from the specified car's save data.
        /// </summary>
        public static List<LiveDecal> CreateLiveDecalsFromData(LiveDecalData[] liveDecalData)
        {
            List<LiveDecal> liveDecals = new();
            foreach (LiveDecalData data in liveDecalData)
            {
                LiveDecal liveDecal = CreateLiveDecalFromData(data);
                liveDecals.Add(liveDecal);
            }

            List<LiveDecal> decalsSorted = liveDecals.OrderBy(liveDecal => liveDecal.Priority).ToList();

            return decalsSorted;
        }
        
        public static LiveDecal CreateLiveDecalFromData(LiveDecalData data)
        {
            DecalUICategory category = Instance.decalUICategories[data.CategoryIndex];
            DecalTexture decalTexture = category.DecalTextures[data.TextureIndex];
            LiveDecal liveDecal = CreateLiveDecal(category, decalTexture, data.Priority);
            liveDecal.PopulateWithData(data);
            return liveDecal;
        }

        private static LiveDecalData[] CreateLiveDecalData(List<LiveDecal> liveDecals)
        {
            LiveDecalData[] finalData = new LiveDecalData[liveDecals.Count];
            for (int index = 0; index < liveDecals.Count; index++)
            {
                LiveDecal liveDecal = liveDecals[index];
                finalData[index] = new LiveDecalData(liveDecal);
            }

            return finalData;
        }
        
        /// <summary>
        /// Gets the save key for the specific car in the player's car.
        /// </summary>
        public static string GetDecalsSaveKey(AICar car)
        {
            return $"{car.SaveKey}.Decals";
        }

        public static void ApplyBaseDecals()
        {
            //TODO
        }

        public static void RemoveBaseDecals()
        {
            //TODO
        }

    }
}
