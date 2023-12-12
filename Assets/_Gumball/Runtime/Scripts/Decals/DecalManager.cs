using System;
using System.Collections;
using System.Collections.Generic;
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

            liveDecal.Initialise(decalTexture,
                Array.IndexOf(Instance.decalUICategories, category), 
                Array.IndexOf(category.DecalTextures, decalTexture));

            liveDecal.SetPriority(priority);
            
            return liveDecal;
        }

        /// <summary>
        /// Saves the specified decals to the car's save data.
        /// </summary>
        public static void SaveLiveDecalData(CarManager car, List<LiveDecal> liveDecals)
        {
            LiveDecal.LiveDecalData[] liveDecalData = CreateLiveDecalData(liveDecals);
            DataManager.Cars.Set(GetDecalsSaveKey(car), liveDecalData);
            GlobalLoggers.DecalsLogger.Log($"Saving {liveDecals.Count} live decals for {car.gameObject.name}.");
        }

        /// <summary>
        /// Loads and applies the decals from the car's save data.
        /// </summary>
        public static void ApplyDecalDataToCar(CarManager car)
        {
            DecalEditor.Instance.StartSession(car);
            DecalEditor.Instance.EndSession();
        }

        /// <summary>
        /// Creates a list of live decals from the specified car's save data.
        /// </summary>
        public static List<LiveDecal> CreateLiveDecalsFromData(CarManager car)
        {
            List<LiveDecal> liveDecals = new();
            LiveDecal.LiveDecalData[] liveDecalData = DataManager.Cars.Get(GetDecalsSaveKey(car), Array.Empty<LiveDecal.LiveDecalData>());
            
            foreach (LiveDecal.LiveDecalData data in liveDecalData)
            {
                LiveDecal liveDecal = CreateLiveDecalFromData(data);
                liveDecals.Add(liveDecal);
            }

            return liveDecals;
        }
        
        public static LiveDecal CreateLiveDecalFromData(LiveDecal.LiveDecalData data)
        {
            DecalUICategory category = Instance.decalUICategories[data.CategoryIndex];
            DecalTexture decalTexture = category.DecalTextures[data.TextureIndex];
            LiveDecal liveDecal = CreateLiveDecal(category, decalTexture, data.Priority);
            liveDecal.PopulateWithData(data);
            return liveDecal;
        }

        private static LiveDecal.LiveDecalData[] CreateLiveDecalData(List<LiveDecal> liveDecals)
        {
            LiveDecal.LiveDecalData[] finalData = new LiveDecal.LiveDecalData[liveDecals.Count];
            for (int index = 0; index < liveDecals.Count; index++)
            {
                LiveDecal liveDecal = liveDecals[index];
                finalData[index] = new LiveDecal.LiveDecalData(liveDecal);
            }

            return finalData;
        }
        
        /// <summary>
        /// Gets the save key for the specific car in the player's car.
        /// </summary>
        /// <param name="car"></param>
        /// <returns></returns>
        private static string GetDecalsSaveKey(CarManager car)
        {
            //TODO - use actual car ID
            const string carID = "0";
            return $"Cars.{carID}.Decals";
        }

    }
}
