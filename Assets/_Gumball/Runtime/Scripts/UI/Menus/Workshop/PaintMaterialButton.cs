using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    [RequireComponent(typeof(MultiImageButton))]
    public class PaintMaterialButton : MonoBehaviour
    {

        [SerializeField] private Color selectedColor;
        [SerializeField] private Color deselectedColor;
        [SerializeField] private float selectionTweenDuration = 0.3f;

        private bool isSelected;
        private Sequence selectionTween;
        
        public MultiImageButton Button => GetComponent<MultiImageButton>();
        
        public void Select()
        {
            isSelected = true;
            
            selectionTween?.Kill();
            selectionTween = DOTween.Sequence();
            foreach (Graphic graphic in Button.TargetGraphics)
                selectionTween.Join(graphic.DOColor(selectedColor, selectionTweenDuration));
        }

        public void Deselect()
        {
            isSelected = false;
            
            selectionTween?.Kill();
            selectionTween = DOTween.Sequence();
            foreach (Graphic graphic in Button.TargetGraphics)
                selectionTween.Join(graphic.DOColor(deselectedColor, selectionTweenDuration));
        }
        
    }
}
