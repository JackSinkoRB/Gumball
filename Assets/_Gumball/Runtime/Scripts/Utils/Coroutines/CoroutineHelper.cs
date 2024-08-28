using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Gumball
{
    public class CoroutineHelper : PersistentSingleton<CoroutineHelper>
    {

        public static event Action onUnityUpdate;
        public static event Action onUnityLateUpdate;
        public static event Action onUnityFixedUpdate;

        private static readonly Dictionary<string, CoroutineHelperInstance> sceneCoroutineInstances = new();
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RuntimeInitialise()
        {
            onUnityUpdate = null;
            onUnityLateUpdate = null;
            onUnityFixedUpdate = null;

            sceneCoroutineInstances.Clear();
        }
        
        /// <summary>
        /// Starts the coroutine on the current scene only. If the scene is unloaded, the coroutine will stop.
        /// </summary>
        public static void StartCoroutineOnCurrentScene(IEnumerator routine)
        {
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (!sceneCoroutineInstances.ContainsKey(sceneName))
                sceneCoroutineInstances[sceneName] = new GameObject($"CoroutineHelper-{sceneName}").AddComponent<CoroutineHelperInstance>();
            
            sceneCoroutineInstances[sceneName].StartCoroutine(routine);
        }
        
        private void Update()
        {
            onUnityUpdate?.Invoke();
        }

        private void LateUpdate()
        {
            onUnityLateUpdate?.Invoke();
        }

        private void FixedUpdate()
        {
            onUnityFixedUpdate?.Invoke();
        }

        public static Coroutine PerformNextFrame(Action action, MonoBehaviour monoToRunOn = null)
        {
            if (action == null)
                return null;

            if (monoToRunOn == null)
                monoToRunOn = Instance;
            return monoToRunOn.StartCoroutine(PerformNextFrameIE(action));
        }

        public static Coroutine PerformAfterFixedUpdate(Action action, MonoBehaviour monoToRunOn = null)
        {
            if (action == null)
                return null;

            if (monoToRunOn == null)
                monoToRunOn = Instance;
            return monoToRunOn.StartCoroutine(PerformAfterFixedUpdateIE(action));
        }

        public static Coroutine PerformAfterTrue(Func<bool> condition, Action action, MonoBehaviour monoToRunOn = null)
        {
            if (condition == null || condition.Invoke())
            {
                //can perform instantly
                action?.Invoke();
                return null;
            }

            if (monoToRunOn == null)
                monoToRunOn = Instance;
            return monoToRunOn.StartCoroutine(PerformAfterTrueIE(condition, action));
        }

        public static Coroutine PerformAfterDelay(float delay, Action action, MonoBehaviour monoToRunOn = null)
        {
            if (delay <= 0)
            {
                //can perform instantly
                action?.Invoke();
                return null;
            }

            if (monoToRunOn == null)
                monoToRunOn = Instance;
            return monoToRunOn.StartCoroutine(PerformAfterDelayIE(delay, action));
        }

        private static IEnumerator PerformNextFrameIE(Action action)
        {
            yield return null;
            action?.Invoke();
        }

        private static IEnumerator PerformAfterFixedUpdateIE(Action action)
        {
            yield return new WaitForFixedUpdate();
            action?.Invoke();
        }

        private static IEnumerator PerformAfterTrueIE(Func<bool> condition, Action action)
        {
            yield return new WaitUntil(condition);
            action?.Invoke();
        }

        private static IEnumerator PerformAfterDelayIE(float delay, Action action)
        {
            yield return new WaitForSeconds(delay);
            action.Invoke();
        }

    }
}