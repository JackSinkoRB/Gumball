using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MagneticScrollUtils
{
    [Serializable]
    public class ScrollItem
    {

        /// <summary>
        /// Called when the item is loaded into an icon.
        /// </summary>
        public event Action onLoad;

        /// <summary>
        /// Called when the item is the closest icon to the magnet.
        /// </summary>
        public event Action onSelect;

        /// <summary>
        /// Called after the pointer is released after selecting an item.
        /// </summary>
        public event Action onSelectComplete;
        
        /// <summary>
        /// Called when the item is no longer the closest icon to the magnet.
        /// </summary>
        public event Action onUnselect;

        /// <summary>
        /// Called when the item's icon button component is clicked.
        /// </summary>
        public event Action onClick;

        public ScrollIcon CurrentIcon { get; private set; }
        public bool HasLoaded { get; private set; }

        public ScrollItem(Action onSelect = null)
        {
            onClick = null;
            this.onSelect = onSelect;
        }

        public void OnLoad(ScrollIcon iconToLoadInto)
        {
            CurrentIcon = iconToLoadInto;
            CurrentIcon.SetCurrentItem(this); //keep reference in the icon

            onLoad?.Invoke();
            HasLoaded = true;
        }

        public void OnSelect()
        {
            onSelect?.Invoke();
        }
        
        public void OnSelectComplete()
        {
            onSelectComplete?.Invoke();
        }

        public void OnUnselect()
        {
            onUnselect?.Invoke();
        }

        public void OnClick()
        {
            OnSelectComplete();
            onClick?.Invoke();
        }

    }
}