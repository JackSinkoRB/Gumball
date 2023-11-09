using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Gumball
{
    [Serializable]
    public class ScrollItem
    {

        /// <summary>
        /// Called when the item is loaded into an icon.
        /// </summary>
        public Action onLoad;

        /// <summary>
        /// Called when the item is the closest icon to the magnet.
        /// </summary>
        public Action onSelect;

        /// <summary>
        /// Called when the item is no longer the closest icon to the magnet.
        /// </summary>
        public Action onUnselect;

        /// <summary>
        /// Called when the item's icon button component is clicked.
        /// </summary>
        public Action onClick;

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

            AddClickListeners();

            onLoad?.Invoke();
            HasLoaded = true;
        }

        public void OnSelect()
        {
            onSelect?.Invoke();
        }

        public void OnUnselect()
        {
            onUnselect?.Invoke();
        }

        public void OnClick()
        {
            onClick?.Invoke();
        }

        private void AddClickListeners()
        {
            if (CurrentIcon.Button == null)
            {
                if (CurrentIcon.ScrollBelongsTo.SelectIconIfButtonClicked)
                {
                    //add the button component
                    CurrentIcon.AddButtonComponent();
                }
                else
                {
                    //no button component on icon
                    return;
                }
            }

            //reset the listener
            CurrentIcon.Button.onClick.RemoveAllListeners();

            //add the click listener for this item
            CurrentIcon.Button.onClick.AddListener(OnClick);

            //also add the click listener on the icon
            CurrentIcon.Button.onClick.AddListener(CurrentIcon.OnClick);
        }

    }
}