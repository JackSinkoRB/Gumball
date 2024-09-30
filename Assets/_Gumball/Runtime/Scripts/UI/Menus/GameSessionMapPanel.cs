using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class GameSessionMapPanel : AnimatedPanel
    {
        
        [SerializeField] private Button leftArrow;
        [SerializeField] private Button rightArrow;

        public Button LeftArrow => leftArrow;
        public Button RightArrow => rightArrow;
        
        public void OnClickBackButton()
        {
            MainSceneManager.LoadMainScene();
        }

        public void OnClickLeftArrow()
        {
            MapSceneManager.Instance.SelectPreviousMap();
        }
        
        public void OnClickRightArrow()
        {
            MapSceneManager.Instance.SelectNextMap();
        }
        
    }
}
