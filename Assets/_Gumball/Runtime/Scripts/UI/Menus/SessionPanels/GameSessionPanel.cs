using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public abstract class GameSessionPanel : AnimatedPanel
    {

        [SerializeField] private SessionProgressBar progressBar;

        public SessionProgressBar ProgressBar => progressBar;

    }
}
