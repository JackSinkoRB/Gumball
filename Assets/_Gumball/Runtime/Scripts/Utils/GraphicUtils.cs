using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gumball
{
    public static class GraphicUtils
    {
        
        private static PointerEventData pointerEventData;
        
        public static Graphic GetClickableGraphic(GraphicRaycaster raycaster, Vector2 screenPosition)
        {
            //initialise
            pointerEventData ??= new PointerEventData(EventSystem.current);
            pointerEventData.position = screenPosition;

            List<RaycastResult> results = new List<RaycastResult>();

            raycaster.Raycast(pointerEventData, results);

            return results.Count == 0 ? null : results[0].gameObject.GetComponent<Graphic>();
        }

        /// <summary>
        /// Gets the raycastable graphics under the specified screen position, and add them to the supplied output list.
        /// </summary>
        public static void GetClickableGraphics(Vector2 screenPosition, List<Graphic> output)
        {
            PointerEventData pointer = new PointerEventData(EventSystem.current) { position = screenPosition };
        
            List<RaycastResult> raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, raycastResults);

            foreach (RaycastResult result in raycastResults)
            {
                Graphic graphic = result.gameObject.GetComponent<Graphic>();
                if (graphic == null || !graphic.raycastTarget)
                    continue;
                    
                output.Add(graphic);
            }
        }
        
        /// <summary>
        /// Gets a list of raycastable graphics under the specified screen position.
        /// </summary>
        public static List<Graphic> GetClickableGraphics(Vector2 screenPosition)
        {
            List<Graphic> clickablesAtPosition = new();
            
            GetClickableGraphics(screenPosition, clickablesAtPosition);

            return clickablesAtPosition;
        }
        
    }
}
