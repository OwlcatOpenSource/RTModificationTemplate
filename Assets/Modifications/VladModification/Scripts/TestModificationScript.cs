using System;
using HarmonyLib;
using System.Reflection;
using Kingmaker.Modding;
using Kingmaker.PubSubSystem;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

public static class TestModificationScript
{
    public static Kingmaker.Modding.OwlcatModification Modification { get; private set; }

    public static bool IsEnabled { get; private set; } = true;

    public static LogChannel Logger => Modification.Logger;

    // ReSharper disable once UnusedMember.Global
    [OwlcatModificationEnterPoint]
    public static void Initialize(Kingmaker.Modding.OwlcatModification modification)
    {
        Modification = modification;
        Debug.LogError("VMCHAR TEST");
        var harmony = new Harmony(modification.Manifest.UniqueName);
        harmony.PatchAll(Assembly.GetExecutingAssembly());

        modification.OnDrawGUI += OnGUI;
        modification.IsEnabled += () => IsEnabled;
        modification.OnSetEnabled += enabled => IsEnabled = enabled;
        modification.OnShowGUI += () => Logger.Log("OnShowGUI");
        modification.OnHideGUI += () => Logger.Log("OnHideGUI");
    }
    
    private static void OnGUI()
    {
        GUILayout.Label("Hello Vlad!");
        GUILayout.Button("Some Button");
    }

}
