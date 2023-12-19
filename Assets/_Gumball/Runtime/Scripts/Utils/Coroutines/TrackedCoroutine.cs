using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    /// <summary>
    /// Starts a coroutine when this object is created, and holds information about the coroutine including if it is currently playing.
    /// <remarks>Coroutine is run on the CoroutineHelper instance. Use Stop() to cancel.</remarks>
    /// </summary>
    public class TrackedCoroutine
    {

        public event Action onComplete;
        
        public Coroutine Coroutine { get; private set; }

        public bool IsPlaying => Coroutine != null;

        public TrackedCoroutine(IEnumerator action, Action onComplete = null)
        {
            SetCoroutine(Start(action));
            this.onComplete = onComplete;
        }

        public TrackedCoroutine()
        {
            
        }

        private IEnumerator Start(IEnumerator action)
        {
            yield return action;
            Stop(true);
        }

        public void Stop(bool completed)
        {
            if (Coroutine == null)
                return;
            
            CoroutineHelper.Instance.StopCoroutine(Coroutine);
            Coroutine = null;
            
            if (completed)
                onComplete?.Invoke();
        }

        public void SetCoroutine(IEnumerator action)
        {
            if (Coroutine != null)
                Stop(false);
            
            Coroutine = CoroutineHelper.Instance.StartCoroutine(Start(action));
        }
        
    }
}
