using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class MultiImageButton : Button
    {

        [SerializeField, ReadOnly] private List<Graphic> targetGraphics = new();

        public List<Graphic> TargetGraphics => targetGraphics;
        
        protected override void Start()
        {
            FindTargetGraphics();
            
            base.Start();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            FindTargetGraphics();

            base.OnValidate();
        }
#endif

        private void FindTargetGraphics()
        {
            targetGraphics = transform.GetComponentsInAllChildren<Graphic>();
            
            Graphic ownGraphic = transform.GetComponent<Graphic>();
            if (ownGraphic != null)
                targetGraphics.Add(ownGraphic);
        }
        
        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            var targetColor =
                state == SelectionState.Disabled ? colors.disabledColor :
                state == SelectionState.Highlighted ? colors.highlightedColor :
                state == SelectionState.Normal ? colors.normalColor :
                state == SelectionState.Pressed ? colors.pressedColor :
                state == SelectionState.Selected ? colors.selectedColor : Color.white;
            
            foreach (Graphic graphic in targetGraphics)
            {
                graphic.CrossFadeColor(targetColor, instant ? 0f : colors.fadeDuration, true, true);
            }
        }
        
    }
}
