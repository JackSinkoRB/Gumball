using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Gumball
{
    public abstract class DataProvider
    {
        
        #region STATIC

        /// <summary>
        /// Called before all the data is saved to file when the app is exited.
        /// <remarks>This can be useful for saving data before the app is closed.</remarks>
        /// </summary>
        public static event Action onBeforeSaveAllDataOnAppExit;
        
        private const float timeBetweenAutoSaveInSeconds = 30;

        public static bool IsAutoSaveActive => autoSaveCoroutine != null;

        private static readonly List<DataProvider> dirtyProviders = new();
        private static Coroutine autoSaveCoroutine;

        /// <summary>
        /// Force save any dirty providers to their source.
        /// <remarks>This shouldn't need to be called as the autosaver handles saving to source.</remarks>
        /// </summary>
        public static void SaveAllAsync()
        {
            GlobalLoggers.SaveDataLogger.Log("Checking to save all dirty data providers (asynchronously).");

            foreach (DataProvider provider in dirtyProviders)
                provider.SaveToSourceAsync();

            dirtyProviders.Clear();
        }

        /// <summary>
        /// Force save any dirty providers to their source.
        /// <remarks>This shouldn't need to be called as the autosaver handles saving to source.</remarks>
        /// </summary>
        public static void SaveAllSync()
        {
#if UNITY_EDITOR
            if (!DataEditorOptions.DataProvidersEnabled)
                return; //don't save to source
#endif

            GlobalLoggers.SaveDataLogger.Log("Saving all dirty data providers (synchronously).");
            foreach (DataProvider provider in dirtyProviders)
            {
                provider.SaveOrRemoveFromSource();
            }

            dirtyProviders.Clear();
        }

        public static void StartAutoSave()
        {
            if (!Application.isPlaying)
                return;
            
            if (IsAutoSaveActive)
            {
                Debug.LogWarning("Tried running auto save, but it is already running.");
                return;
            }

            autoSaveCoroutine = CoroutineHelper.Instance.StartCoroutine(AutoSave());

            //listen for quit to force auto save
            Application.quitting += OnQuit;

            GlobalLoggers.SaveDataLogger.Log($"Started auto saver.");
        }

        public static void StopAutoSave()
        {
            if (!IsAutoSaveActive)
            {
                Debug.LogWarning("Tried pausing auto save, but it is not running.");
                return;
            }

            CoroutineHelper.Instance.StopCoroutine(autoSaveCoroutine);
            autoSaveCoroutine = null;

            Application.quitting -= OnQuit;

            GlobalLoggers.SaveDataLogger.Log($"Stopped auto saver.");
        }

        private static IEnumerator AutoSave()
        {
            //run until explicitly stopped
            while (true)
            {
                yield return new WaitForSeconds(timeBetweenAutoSaveInSeconds);

                SaveAllAsync();
            }
        }

        private static void OnQuit()
        {
            onBeforeSaveAllDataOnAppExit?.Invoke();
            SaveAllSync();
        }

        #endregion

        protected readonly string identifier;
        protected Dictionary<string, object> currentValues = new();
        
        private readonly object accessLock = new();

        public bool IsLoaded { get; private set; }
        public bool IsDirty => dirtyProviders.Contains(this);

        public DataProvider(string identifier)
        {
            this.identifier = identifier;
        }

        public async void LoadFromSourceAsync(Action onComplete = null)
        {
            await Task.Run(LoadFromSourceSync);
            
            onComplete?.Invoke();
        }

        public void LoadFromSourceSync()
        {
            lock (accessLock)
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                LoadFromSource();
                stopwatch.Stop();

                IsLoaded = true;
                GlobalLoggers.SaveDataLogger.Log($"Loaded from source '{identifier}' ({stopwatch.ElapsedMilliseconds}ms)");
            }
        }

        public bool HasKey(string key)
        {
            CheckIfLoaded();
            return currentValues.ContainsKey(key);
        }

        public void Set(string key, object value)
        {
            CheckIfLoaded();

            if (currentValues.ContainsKey(key) && currentValues[key].Equals(value))
                return;

            if (!currentValues.ContainsKey(key) && value == null)
                return;

            if (value == null)
            {
                RemoveKey(key);
                return;
            }
            
            currentValues[key] = value;
            SetDirty();

            GlobalLoggers.SaveDataLogger.Log($"Set {key} to '{value}' in {identifier}.");
        }

        public T Get<T>(string key, T defaultValue = default)
        {
            CheckIfLoaded();

            if (!currentValues.ContainsKey(key))
                return defaultValue;
            return (T)currentValues[key];
        }

        public void RemoveKey(string key)
        {
            CheckIfLoaded();

            if (!currentValues.ContainsKey(key))
            {
                GlobalLoggers.SaveDataLogger.Log($"Tried removing key '{key}' from {identifier}, but it didn't exist.");
                return;
            }

            currentValues.Remove(key);
            SetDirty();
        }

        /// <summary>
        /// Remove all values and do any cleanup from the source.
        /// </summary>
        public void RemoveFromSource()
        {
            lock (accessLock)
            {
                currentValues.Clear();
                IsLoaded = false;
                RemoveDirty();
                
                OnRemoveFromSource();
                
                if (GlobalLoggers.HasLoaded)
                    GlobalLoggers.SaveDataLogger.Log($"Removed all keys from {identifier}.");
            }
        }
        
        /// <summary>
        /// Forces a synchronous reload.
        /// </summary>
        public void ReloadFromSource()
        {
            lock (accessLock)
            {
                CheckIfLoaded();

                RemoveDirty();
                LoadFromSource();
            }
        }

        public abstract bool SourceExists();
        protected abstract void SaveToSource();
        protected abstract void LoadFromSource();
        protected abstract void OnRemoveFromSource();
        
        private async void SaveToSourceAsync(Action onComplete = null)
        {
#if UNITY_EDITOR
            if (!DataEditorOptions.DataProvidersEnabled)
                return; //don't save to source
#endif

            
            Stopwatch stopwatch = Stopwatch.StartNew();
            await Task.Run(SaveOrRemoveFromSource);
            stopwatch.Stop();

            GlobalLoggers.SaveDataLogger.Log($"Saved all to '{identifier}' (async - {stopwatch.ElapsedMilliseconds}ms)");
            onComplete?.Invoke();
        }

        /// <summary>
        /// Saves to the source if there's values, otherwise removes from the source.
        /// </summary>
        private void SaveOrRemoveFromSource()
        {
            lock (accessLock)
            {
                if (currentValues.Count == 0)
                {
                    //nothing left
                    RemoveFromSource();
                    return;
                }

                SaveToSource();
            }
        }

        private void SetDirty()
        {
            if (dirtyProviders.Contains(this))
                return; //is already dirty

            dirtyProviders.Add(this);
            if (!IsAutoSaveActive)
                StartAutoSave();
        }

        private void RemoveDirty()
        {
            dirtyProviders.Remove(this);
        }

        private void CheckIfLoaded()
        {
            if (IsLoaded)
                return;

#if UNITY_EDITOR
            lock (accessLock) {
                LoadFromSourceSync();
                Debug.LogWarning($"Tried accessing {identifier} before it was loaded, so force loaded it.");
                return;
            }
#endif

            throw new InvalidOperationException($"Tried setting value in {identifier} but is has not fully loaded yet. You should wait for loading to complete first.");
        }
    }
}