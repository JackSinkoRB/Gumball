using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/Singletons/Challenge Manager")]
    public class ChallengeManager : SingletonScriptable<ChallengeManager>
    {
        
        [SerializeField] private Challenges daily;
        [SerializeField] private Challenges weekly;

        public Challenges Daily => daily;
        public Challenges Weekly => weekly;

        protected override void OnInstanceLoaded()
        {
            base.OnInstanceLoaded();

            daily.Initialise();
        }
        
    }
}
