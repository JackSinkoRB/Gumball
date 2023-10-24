using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AYellowpaper.SerializedCollections;
using MyBox;
using UnityEngine;

public class PanelManager : Singleton<PanelManager>
{

    [Tooltip("Panels that aren't under this object, that should be a part of the panel lookup.")]
    [SerializeField] private AnimatedPanel[] additionalPanels;

    [SerializeField, ReadOnly] private List<AnimatedPanel> panelStack = new();
    [SerializedDictionary, ReadOnly] private Dictionary<Type, AnimatedPanel> panelLookup = new();

    public ReadOnlyCollection<AnimatedPanel> PanelStack => panelStack.AsReadOnly();
    
    public static T GetPanel<T>() where T : AnimatedPanel
    {
        return GetPanel(typeof(T)) as T;
    }
    
    public static AnimatedPanel GetPanel(Type panelType)
    {
        return Instance.panelLookup.ContainsKey(panelType) ? Instance.panelLookup[panelType] : null;
    }
    
    protected override void Initialise()
    {
        base.Initialise();
        
        CreatePanelLookup();
    }

    public void AddToStack(AnimatedPanel animatedPanel)
    {
        panelStack.Add(animatedPanel);
        animatedPanel.OnAddToStack();
    }

    public void RemoveFromStack(AnimatedPanel animatedPanel)
    {
        //show the previous panel if this was the last panel
        if (panelStack.Count >= 2 && panelStack[^1] == animatedPanel)
            panelStack[^2].Show();
        
        panelStack.Remove(animatedPanel);

        animatedPanel.OnRemoveFromStack();
    }
    
    private void CreatePanelLookup()
    {
        AnimatedPanel[] panels = GetComponentsInChildren<AnimatedPanel>(true);
        foreach (AnimatedPanel panel in panels)
        {
            TryAddPanelToLookup(panel);
        }

        foreach (AnimatedPanel panel in additionalPanels)
        {
            TryAddPanelToLookup(panel);
        }
    }

    private void TryAddPanelToLookup(AnimatedPanel panel)
    {
        var panelType = panel.GetType();
        if (panelLookup.ContainsKey(panelType))
        {
            Debug.LogError($"Multiple panels detected of type: {panelType.Name}");
            return;
        }

        panel.gameObject.SetActive(false);
        panelLookup.Add(panelType, panel);
    }
    
}
