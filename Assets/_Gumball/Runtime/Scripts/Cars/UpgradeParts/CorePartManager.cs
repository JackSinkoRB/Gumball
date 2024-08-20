using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Gumball
{
    public static class CorePartManager
    {
        
        /// <summary>
        /// The addressables label for the core parts data group.
        /// </summary>
        public const string CorePartsAssetLabel = "CorePart";
        
        private static readonly List<CorePart> allParts = new();
        private static readonly Dictionary<CorePart.PartType, CorePart[]> allPartsGrouped = new();
        private static readonly Dictionary<string, CorePart> partsMappedByID = new();

        public static ReadOnlyCollection<CorePart> AllParts => allParts.AsReadOnly();

        private static bool isInitialised;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RuntimeInitialise()
        {
            isInitialised = false;
        }
        
        public static IEnumerator Initialise()
        {
            if (isInitialised)
                yield break;

            isInitialised = true;
            
            yield return FindParts();
        }
        
        public static void SetCorePart(int carIndex, CorePart.PartType type, CorePart corePart)
        {
            DataManager.Cars.Set($"{GetSaveKeyFromIndex(carIndex)}.Core.{type.ToString()}", corePart == null ? null : corePart.ID);
        }
        
        public static CorePart GetCorePart(int carIndex, CorePart.PartType type)
        {
            string saveKey = $"{GetSaveKeyFromIndex(carIndex)}.Core.{type.ToString()}";
            
            WarehouseCarData carData = WarehouseManager.Instance.AllCarData[carIndex];
            CorePart defaultPart = carData.GetDefaultPart(type);
            
            string partID = DataManager.Cars.Get(saveKey, defaultPart != null ? defaultPart.ID : null);
            return GetPartByID(partID);
        }

        public static CorePart[] GetCoreParts(int carIndex)
        {
            CorePart.PartType[] coreParts = (CorePart.PartType[]) Enum.GetValues(typeof(CorePart.PartType));
            CorePart[] parts = new CorePart[coreParts.Length];
            
            for (int index = 0; index < coreParts.Length; index++) {
                CorePart.PartType type = coreParts[index];
                parts[index] = GetCorePart(carIndex, type);
            }

            return parts;
        }

        public static void InstallParts(int carIndex)
        {
            foreach (CorePart part in GetCoreParts(carIndex))
            {
                if (part == null)
                    continue; //no part applied
                
                InstallPartOnCar(part.Type, part, carIndex);
            }
        }
        
        public static CorePart GetPartByID(string ID)
        {
            if (ID == null)
                return null;
            
            if (!partsMappedByID.ContainsKey(ID))
                throw new NullReferenceException($"There is no core part with ID {ID}");
            
            return partsMappedByID[ID];
        }
        
        /// <returns>A collection of unlocked parts of type 'partType'.</returns>
        public static HashSet<CorePart> GetSpareParts(CorePart.PartType partType, CarType? carType = null)
        {
            HashSet<CorePart> spareParts = new();

            if (!allPartsGrouped.ContainsKey(partType))
                return spareParts;
            
            foreach (CorePart part in allPartsGrouped[partType])
            {
                if (part.IsUnlocked && !part.IsAppliedToCar && (carType == null || carType.Value == part.CarType))
                    spareParts.Add(part);
            }
            
            return spareParts;
        }

        public static void InstallPartOnCar(CorePart.PartType type, CorePart part, int carIndex)
        {
            //if car has a part already installed, remove the reference to set it as a spare
            CorePart existingPart = GetCorePart(carIndex, type);
            if (existingPart != null)
                existingPart.RemoveFromCar();

            //apply to car
            SetCorePart(carIndex, type, part);
            
            //apply to part
            if (part != null)
                part.ApplyToCar(carIndex);
            
            //update the cars performance profile if it's the active car
            bool isAttachedToCurrentCar = WarehouseManager.Instance.CurrentCar != null && WarehouseManager.Instance.CurrentCar.CarIndex == carIndex;
            if (isAttachedToCurrentCar)
                WarehouseManager.Instance.CurrentCar.SetPerformanceProfile(new CarPerformanceProfile(carIndex));
        }
        
        
        //TODO: sub parts
        
        // public static void SetSubPart(int carIndex, SubPart.SubPartType type, CorePart corePart)
        // {
        //     DataManager.Cars.Set($"{GetSaveKeyFromIndex(carIndex)}.Sub.{type.ToString()}", corePart == null ? null : corePart.ID);
        // }
        //
        // public static SubPart GetSubPart(int carIndex, SubPart.SubPartType type)
        // {
        //     string partID = DataManager.Cars.Get<string>($"{GetSaveKeyFromIndex(carIndex)}.Sub.{type.ToString()}");
        //     return SubPartManager.GetPartByID(partID);
        // }

        private static IEnumerator FindParts()
        {
            allParts.Clear();
            
            yield return AddressableUtils.LoadAssetsAsync(CorePartsAssetLabel, allParts, typeof(CorePart));

            InitialiseSubPartSlots();
            GroupParts();
            CreateIDLookup();
        }

        private static void InitialiseSubPartSlots()
        {
            foreach (CorePart part in allParts)
            {
                part.InitialiseSubPartSlots();
            }
        }

        private static void GroupParts()
        {
            allPartsGrouped.Clear();
            
            Dictionary<CorePart.PartType, HashSet<CorePart>> grouped = new();
        
            //group
            foreach (CorePart part in allParts)
            {
                if (!grouped.ContainsKey(part.Type))
                    grouped[part.Type] = new HashSet<CorePart>();

                grouped[part.Type].Add(part);
            }

            //convert to arrays
            foreach (KeyValuePair<CorePart.PartType, HashSet<CorePart>> key in grouped)
            {
                allPartsGrouped[key.Key] = key.Value.ToArray();
            }
        }

        private static void CreateIDLookup()
        {
            partsMappedByID.Clear();
            
            foreach (CorePart corePart in allParts)
            {
                if (partsMappedByID.ContainsKey(corePart.ID))
                    Debug.LogWarning($"There are multiple core parts with ID {corePart.ID} ({corePart.name})");

                partsMappedByID[corePart.ID] = corePart;
            } 
        }
        
        private static string GetSaveKeyFromIndex(int carIndex)
        {
            return $"{AICar.GetSaveKeyFromIndex(carIndex)}.Parts";
        }
        
    }
}
