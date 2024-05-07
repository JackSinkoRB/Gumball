using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class PartModification : MonoBehaviour
    {
        
#region STATIC
        private static string GetSaveKeyFromIndex(int carIndex)
        {
            return $"{AICar.GetSaveKeyFromIndex(carIndex)}.CoreParts";
        }
        
        public static void SetCorePart(int carIndex, CorePart.PartType type, CorePart corePart)
        {
            DataManager.Cars.Set($"{GetSaveKeyFromIndex(carIndex)}.{type.ToString()}", corePart == null ? null : corePart.ID);
        }
        
        public static CorePart GetCorePart(int carIndex, CorePart.PartType type)
        {
            string partID = DataManager.Cars.Get<string>($"{GetSaveKeyFromIndex(carIndex)}.{type.ToString()}");
            return CorePartManager.GetPartByID(partID);
        }
#endregion
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private AICar carBelongsTo;

        public void Initialise(AICar carBelongsTo)
        {
            this.carBelongsTo = carBelongsTo;
        }

    }
}
