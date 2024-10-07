using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Gumball;
using MyBox;
using UnityEngine;
using UnityEngine.SceneManagement;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

public class PanelManager : PersistentSingleton<PanelManager>
{

    [SerializeField, ReadOnly] private List<AnimatedPanel> panelStack = new();
    
    private readonly Dictionary<Type, AnimatedPanel> panelLookup = new();

    public ReadOnlyCollection<AnimatedPanel> PanelStack => panelStack.AsReadOnly();

    protected override void Initialise()
    {
        base.Initialise();

        panelLookup.Clear();
        
        SceneManager.activeSceneChanged -= OnSceneChange;
        SceneManager.activeSceneChanged += OnSceneChange;

        CreatePanelLookup();
    }

    private void OnSceneChange(Scene oldScene, Scene newScene)
    {
        CreatePanelLookup();
    }

    public static T GetPanel<T>() where T : AnimatedPanel
    {
        return GetPanel(typeof(T)) as T;
    }
    
    public static AnimatedPanel GetPanel(Type panelType)
    {
        if (!PanelExists(panelType))
            throw new NullReferenceException($"Could not find panel {panelType} in lookup. Scene is: {SceneManager.GetActiveScene().name}");
        
        return Instance.panelLookup[panelType];
    }

    public static bool PanelExists<T>() where T : AnimatedPanel
    {
        return PanelExists(typeof(T));
    }
    
    public static bool PanelExists(Type panelType)
    {
        return Instance.panelLookup.ContainsKey(panelType) && Instance.panelLookup[panelType] != null;
    }

    public void AddToStack<T>() where T : AnimatedPanel
    {
        AddToStack(GetPanel<T>());
    }
    
    public void AddToStack(AnimatedPanel animatedPanel)
    {
        panelStack.Add(animatedPanel);
        
        animatedPanel.OnAddToStack();
        GlobalLoggers.PanelLogger.Log($"Added {animatedPanel.gameObject.name} to stack.");
    }

    public void RemoveFromStack<T>() where T : AnimatedPanel
    {
        RemoveFromStack(GetPanel<T>());
    }
    
    public void RemoveFromStack(AnimatedPanel animatedPanel)
    {
        RemoveNullPanelsFromStack();

        //show the previous panel if this was the last panel
        if (panelStack.Count >= 2 && panelStack[^1] == animatedPanel)
            panelStack[^2].Show();
        
        panelStack.Remove(animatedPanel);

        animatedPanel.OnRemoveFromStack();
        GlobalLoggers.PanelLogger.Log($"Removed {animatedPanel.gameObject.name} from stack.");
    }

    /// <summary>
    /// Clear the panel lookup and find and add all the panels in the current scene.
    /// </summary>
    private void CreatePanelLookup()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        
        Stopwatch stopwatch = Stopwatch.StartNew();

        //remove null panels (if scene was unloaded etc.)
        List<Type> keys = new List<Type>(panelLookup.Keys);
        foreach (Type key in keys)
        {
            if (panelLookup[key] == null)
            {
                GlobalLoggers.PanelLogger.Log($"Removing {key} from panel lookup");
                panelLookup.Remove(key);
            }
        }
        
        foreach (AnimatedPanel panel in SceneUtils.GetAllComponentsInActiveScene<AnimatedPanel>(true))
        {
            GlobalLoggers.PanelLogger.Log($"Adding {panel.GetType()} to panel lookup");
            panelLookup[panel.GetType()] = panel;
            if (panel.gameObject.activeInHierarchy)
                panel.Show(); //starts showing
            else
                panel.Hide(instant: true);
        }
        
        stopwatch.Stop();
        GlobalLoggers.LoadingLogger.Log($"Took {stopwatch.Elapsed.ToPrettyString(true)} to create the panel lookup for {sceneName}.");
        
        Canvas.ForceUpdateCanvases();
    }
    
    /// <summary>
    /// If the scene has lost reference to a panel in the stack (scene change etc.), remove it from the stack.
    /// </summary>
    private void RemoveNullPanelsFromStack()
    {
        for (int index = 0; index < panelStack.Count; index++)
        {
            AnimatedPanel panel = panelStack[index];
            if (panel == null)
                panelStack.RemoveAt(index);
        }
    }

}
