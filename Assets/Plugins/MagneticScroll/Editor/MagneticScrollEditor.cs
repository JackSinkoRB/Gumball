#if UNITY_EDITOR
using System;
using System.Linq;
using MyBox;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

namespace MagneticScrollUtils
{
    [CustomEditor(typeof(MagneticScroll))]
    public class MagneticScrollEditor : Editor
    {

        private MagneticScroll magneticScroll => target as MagneticScroll;
        private RectTransform rectTransform => magneticScroll.GetComponent<RectTransform>();
        private Image image => magneticScroll.GetComponent<Image>();

        //since we're accessing this outside runtime, the RectTransform property on the ScrollIcon will not have been initialised, hence use GetComponent.
        private RectTransform scrollIconPrefabRectTransform => magneticScroll.ScrollIconPrefab.GetComponent<RectTransform>();

        private void OnSceneGUI()
        {
            //disable editing the rect transform directly. Can only change position with move tool.
            rectTransform.hideFlags = HideFlags.NotEditable;
            //can't edit the image component
            image.hideFlags = HideFlags.NotEditable;

            //fix up the components if they've been edited outside the script
            FixRectTransformComponent();
            FixImageComponent();

            SetRectSize();
            SetRaycastPadding();
        }

        public override void OnInspectorGUI()
        {
            DisplayMessages();

            base.OnInspectorGUI();
        }

        private void DisplayMessages()
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox("Move the position of the magnetic scroll using the editor move tool.\n" +
                                    "The center of this object will be the position that the selected icon will snap to.", MessageType.Info);

            if (magneticScroll.PopulatesAtRuntime && magneticScroll.ScrollIconPrefab == null)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.HelpBox("You are missing a ScrollIconPrefab.", MessageType.Error);
            }

            if (magneticScroll.PreDefinedItems.Any(scrollItem => scrollItem == null))
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.HelpBox("A Pre Defined Item is null or not set!", MessageType.Error);
            }

            if (!magneticScroll.PopulatesAtRuntime && magneticScroll.PreDefinedItems.Count == 0)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.HelpBox("This scroll doesn't have any items.", MessageType.Warning);
            }

            if (Application.isPlaying && magneticScroll.PopulatesAtRuntime && magneticScroll.Items.Count == 0)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.HelpBox("This scroll hasn't been populated with items.", MessageType.Warning);
            }

            EditorGUILayout.Space(10);
        }

        private void FixRectTransformComponent()
        {
            FixAnchoring();
            FixScale();
            FixRotation();
        }

        private void FixImageComponent()
        {
            //hide any graphics on this component
            if (image.color.a != 0)
            {
                image.SetAlpha(0);
            }

            //must be a raycast target
            if (!image.raycastTarget)
            {
                image.raycastTarget = true;
            }
        }

        private void FixAnchoring()
        {
            //check if anchoring is not centered
            Vector2 centered = new Vector2(0.5f, 0.5f);
            if (rectTransform.anchorMin != centered
                || rectTransform.anchorMax != centered
                || rectTransform.pivot != centered)
            {
                //log warning
                Debug.LogWarning("MagneticScroll rect anchoring should only be centered.");

                //fix the anchoring
                rectTransform.anchorMin = centered;
                rectTransform.anchorMax = centered;
                rectTransform.pivot = centered;
            }
        }

        private void FixScale()
        {
            //check if scale has been changed (possibly with scale tool)
            if (rectTransform.localScale != Vector3.one)
            {
                Debug.LogWarning("MagneticScroll rect scale should only be at 1,1,1.");
                //fix the scale
                rectTransform.localScale = Vector3.one;
            }
        }

        private void FixRotation()
        {
            //check if rotation has been changed (possibly with rotation tool)
            if (rectTransform.rotation.eulerAngles != Vector3.zero)
            {
                Debug.LogWarning("MagneticScroll rect rotation should only be at 0,0,0.");
                //fix the rotation
                rectTransform.rotation = Quaternion.Euler(Vector3.zero);
            }
        }

        private void SetRectSize()
        {
            //set the size of the rect depending on the number of icons and spacing
            //if it's a vertical scroll, the width can be the size of the largest icon's width (and vice versa for horizontal)

            //get the total size in the correct direction:
            float sizeOfAllIcons = GetSizeOfAllIcons();
            float spacing = GetTotalSpacing();

            float totalSize = sizeOfAllIcons + spacing;

            //get the largest icon size in the opposite direction
            MagneticScroll.Direction oppositeDirection =
                magneticScroll.IsHorizontal ? MagneticScroll.Direction.VERTICAL : MagneticScroll.Direction.HORIZONTAL;
            float largestIconInOppositeDirection = GetLargestIconSize(oppositeDirection);

            //set the rect size
            float width = magneticScroll.IsHorizontal ? totalSize : largestIconInOppositeDirection;
            float height = magneticScroll.IsVertical ? totalSize : largestIconInOppositeDirection;
            rectTransform.sizeDelta = new Vector2(width, height);
        }

        private void SetRaycastPadding()
        {
            image.raycastPadding = new Vector4(magneticScroll.ClickableAreaPadding.left,
                magneticScroll.ClickableAreaPadding.bottom,
                magneticScroll.ClickableAreaPadding.right,
                magneticScroll.ClickableAreaPadding.top);
        }

        private float GetSizeOfAllIcons()
        {
            float sizeOfAllIcons = 0;

            if (magneticScroll.PopulatesAtRuntime)
            {
                if (magneticScroll.ScrollIconPrefab == null)
                {
                    //hasn't added a scroll icon prefab
                    return 0;
                }

                float size = magneticScroll.IsHorizontal ? scrollIconPrefabRectTransform.rect.width : scrollIconPrefabRectTransform.rect.height;

                sizeOfAllIcons = magneticScroll.NumberOfIcons * size;
            }
            else
            {
                foreach (PreDefinedScrollItem scrollItem in magneticScroll.PreDefinedItems)
                {
                    if (scrollItem == null)
                    {
                        continue;
                    }

                    RectTransform scrollIconRectTransform = scrollItem.scrollIcon.GetComponent<RectTransform>();
                    float size = magneticScroll.IsHorizontal
                        ? scrollIconRectTransform.rect.width * scrollIconRectTransform.localScale.x
                        : scrollIconRectTransform.rect.height * scrollIconRectTransform.localScale.y;

                    sizeOfAllIcons += size;
                }
            }

            return sizeOfAllIcons;
        }

        private float GetTotalSpacing()
        {
            //spacing = number of icons - 1 * spacing
            return (magneticScroll.NumberOfIcons - 1) * magneticScroll.SpacingBetweenIcons;
        }

        private float GetLargestIconSize(MagneticScroll.Direction direction)
        {
            if (magneticScroll.PopulatesAtRuntime)
            {
                if (magneticScroll.ScrollIconPrefab == null)
                {
                    //hasn't added a scroll icon prefab
                    return 0;
                }

                //all icons are the same size if populated at runtime
                return direction == MagneticScroll.Direction.HORIZONTAL ? scrollIconPrefabRectTransform.rect.width * scrollIconPrefabRectTransform.localScale.x : scrollIconPrefabRectTransform.rect.height * scrollIconPrefabRectTransform.localScale.y;
            }

            float largest = 0;
            foreach (PreDefinedScrollItem scrollItem in magneticScroll.PreDefinedItems)
            {
                if (scrollItem == null)
                {
                    continue;
                }

                RectTransform scrollIconRectTransform = scrollItem.scrollIcon.GetComponent<RectTransform>();
                float size = direction == MagneticScroll.Direction.HORIZONTAL
                    ? scrollIconRectTransform.rect.width * scrollIconRectTransform.localScale.x
                    : scrollIconRectTransform.rect.height * scrollIconRectTransform.localScale.y;

                if (size > largest)
                {
                    largest = size;
                }
            }

            return largest;
        }

    }
}
#endif