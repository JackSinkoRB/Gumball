using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelManager : Singleton<PanelManager>
{

    [Tooltip("Panels that aren't under this object, that should be a part of the panel lookup.")]
    [SerializeField] private AnimatedPanel[] additionalPanels;
    
    private readonly Dictionary<Type, AnimatedPanel> panelLookup = new();
    
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
