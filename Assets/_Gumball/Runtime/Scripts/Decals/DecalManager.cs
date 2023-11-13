using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public static class DecalManager
    {

        /// <summary>
        /// Saves the specified decals to the car's save data.
        /// </summary>
        public static void SaveLiveDecalData(CarManager car, List<LiveDecal> liveDecals)
        {
            LiveDecal.LiveDecalData[] liveDecalData = CreateLiveDecalData(liveDecals);
            DataManager.Cars.Set(GetDecalsSaveKey(car), liveDecalData);
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
                LiveDecal liveDecal = DecalEditor.Instance.CreateLiveDecalFromData(data);
                liveDecals.Add(liveDecal);
            }

            return liveDecals;
        }
        
        /// <summary>
        /// Gets the save key for the specific car in the player's car
        /// </summary>
        /// <param name="car"></param>
        /// <returns></returns>
        private static string GetDecalsSaveKey(CarManager car)
        {
            //TODO - use actual car ID
            const string carID = "0";
            return $"Cars.{carID}.Decals";
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
        
    }
}
