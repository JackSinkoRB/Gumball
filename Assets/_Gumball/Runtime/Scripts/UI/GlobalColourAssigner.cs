using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using TMPro;
using UIGradientUtility;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class GlobalColourAssigner : BaseMeshEffect
    {

        [SerializeField] private GlobalColourPalette.ColourCode colourCode;

        [Header("Gradient")]
        [SerializeField] private bool useGradient;
        [SerializeField, ConditionalField(nameof(useGradient))] private GlobalColourPalette.ColourCode gradientColor;
        [SerializeField, ConditionalField(nameof(useGradient))] private GlobalColourPalette.ColourCode gradientBaseTint = GlobalColourPalette.ColourCode.C5;
        [SerializeField, ConditionalField(nameof(useGradient)), Range(-180f, 180f)] private float gradientAngle;
        [SerializeField, ConditionalField(nameof(useGradient))] private bool gradientIgnoreRatio = true;

        private Image image => GetComponent<Image>();
        private TextMeshProUGUI label => GetComponent<TextMeshProUGUI>();

        protected override void OnEnable()
        {
            base.OnEnable();
            
            this.PerformAfterTrue(() => GlobalColourPalette.HasLoaded, UpdateColors);
        }
        
        public override void ModifyMesh(VertexHelper vertexHelper)
        {
            if (enabled && useGradient)
                RenderGradient(vertexHelper);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            
            if (!GlobalColourPalette.HasLoaded)
                GlobalColourPalette.LoadInstanceSync();
            UpdateColors();
        }
#endif

        public void SetColour(GlobalColourPalette.ColourCode colourCode)
        {
            this.colourCode = colourCode;
            UpdateColors();
        }

        private void UpdateColors()
        {
#if UNITY_EDITOR
            if (DataManager.IsUsingTestProviders) //disable if running tests as it causes issues
                return;
#endif
            
            Color globalColour = GlobalColourPalette.Instance.GetGlobalColor(colourCode);
        
            if (image != null)
                image.color = globalColour.WithAlphaSetTo(image.color.a);;

            if (label != null)
                label.color = globalColour.WithAlphaSetTo(label.color.a);;
        }

        private void RenderGradient(VertexHelper vertexHelper)
        {
            Rect rect = graphic.rectTransform.rect;
            Vector2 dir = UIGradientUtils.RotationDir(gradientAngle);

            if (!gradientIgnoreRatio)
                dir = UIGradientUtils.CompensateAspectRatio(rect, dir);

            UIGradientUtils.Matrix2x3 localPositionMatrix = UIGradientUtils.LocalPositionMatrix(rect, dir);

            Color color = GlobalColourPalette.Instance.GetGlobalColor(gradientColor);
            Color baseTint = GlobalColourPalette.Instance.GetGlobalColor(gradientBaseTint);
            
            UIVertex vertex = default(UIVertex);
            for (int i = 0; i < vertexHelper.currentVertCount; i++)
            {
                vertexHelper.PopulateUIVertex (ref vertex, i);
                Vector2 localPosition = localPositionMatrix * vertex.position;
                vertex.color *= Color.Lerp(color, baseTint, localPosition.y);
                vertexHelper.SetUIVertex (vertex, i);
            }
        }
        
    }
}
