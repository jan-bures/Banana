using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace Banana.Patches;

[HarmonyPatch]
public class Imgui
{
    private static readonly HashSet<int> BlockedWindows = new();
    private static readonly HashSet<int> OpenWindows = new();

    // ReSharper disable once UnusedMember.Global
    public static IEnumerable<MethodBase> TargetMethods()
    {
        return new[] { typeof(GUI), typeof(GUILayout) }
            .SelectMany(type => type.GetMethods())
            .Where(method => method.Name == "Window");
    }

    // ReSharper disable once InconsistentNaming
    // ReSharper disable once UnusedMember.Global
    public static void Postfix(object[] __args)
    {
        RegisterBlocking((int)__args[0], (Rect)__args[1]);
    }

#pragma warning disable Harmony003
    private static void RegisterBlocking(int id, Rect windowRect)
    {
        OpenWindows.Add(id);
        var position = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
        var containsPosition = windowRect.Contains(position);
        switch (containsPosition)
        {
            case true when !BlockedWindows.Contains(id):
                BlockedWindows.Add(id);
                BananaPlugin.StartBlocking();
                break;
            case false when BlockedWindows.Contains(id):
                BlockedWindows.Remove(id);
                BananaPlugin.StopBlocking();
                break;
        }
    }
#pragma warning restore Harmony003

    internal static void LateUpdate()
    {
        foreach (var id in BlockedWindows.Where(id => !OpenWindows.Contains(id)).ToList())
        {
            BlockedWindows.Remove(id);
            BananaPlugin.StopBlocking();
        }

        OpenWindows.Clear();
    }
}