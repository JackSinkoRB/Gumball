using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/Singletons/Gumball Event Manager")]
    public class GumballEventManager : SingletonScriptable<GumballEventManager>
    {

        [SerializeField, DisplayInspector] private GumballEvent currentEvent;

        public GumballEvent CurrentEvent => currentEvent;

    }
}
