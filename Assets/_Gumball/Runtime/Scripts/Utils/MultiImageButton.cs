using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class MultiImageButton : Button
    {

        [SerializeField, ReadOnly] private List<Graphic> targetGraphics;

        protected override void Start()
        {
            targetGraphics = transform.GetComponentsInAllChildren<Graphic>();
            
            base.Start();
        }

        protected override void OnValidate()
        {
            targetGraphics = transform.GetComponentsInAllChildren<Graphic>();
            
            base.OnValidate();
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
