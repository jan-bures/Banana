using System.Reflection;
using HarmonyLib;
using UitkForKsp2.API;
using UnityEngine.UIElements;

namespace Banana.Patches;

[HarmonyPatch]
public class Uitk
{
    // ReSharper disable once UnusedMember.Global
    public static IEnumerable<MethodBase> TargetMethods()
    {
        return typeof(Window).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(method => method.Name.StartsWith("Create"));
    }

    // ReSharper disable once InconsistentNaming
    // ReSharper disable once UnusedMember.Global
    public static void Postfix(UIDocument __result)
    {
        RegisterBlockingCallbacks(__result);
    }

    private static void RegisterBlockingCallbacks(UIDocument document)
    {
        var root = document.rootVisualElement;
        root.RegisterCallback<MouseEnterEvent>(evt =>
        {
            if (evt.currentTarget != root)
            {
                return;
            }

            BananaPlugin.StartBlocking();
        });

        root.RegisterCallback<MouseLeaveEvent>(evt =>
        {
            if (evt.currentTarget != root)
            {
                return;
            }

            BananaPlugin.StopBlocking();
        });
    }
}