using Banana.Patches;
using BepInEx;
using HarmonyLib;
using SpaceWarp;
using SpaceWarp.API.Mods;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

// ReSharper disable MemberCanBePrivate.Global

namespace Banana;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(SpaceWarpPlugin.ModGuid, SpaceWarpPlugin.ModVer)]
[HarmonyPatch]
public class BananaPlugin : BaseSpaceWarpPlugin
{
    // These are useful in case some other mod wants to add a dependency to this one
    // ReSharper disable UnusedMember.Global
    public const string ModGuid = MyPluginInfo.PLUGIN_GUID;
    public const string ModName = MyPluginInfo.PLUGIN_NAME;
    public const string ModVer = MyPluginInfo.PLUGIN_VERSION;
    // ReSharper restore UnusedMember.Global

    public static BananaPlugin Instance { get; set; }

    private bool _loaded;
    private static int _blocking;

    /// <summary>
    /// Runs when the mod is first initialized.
    /// </summary>
    public override void OnInitialized()
    {
        Instance = this;

        Harmony.CreateAndPatchAll(typeof(BananaPlugin).Assembly);

        CreateCanvas();

        _loaded = true;
        Logger.LogInfo($"{ModName} loaded.");
    }

    private static RectTransform _blockerTransform;

    private static void CreateCanvas()
    {
        // Canvas object setup
        var canvasObject = new GameObject
        {
            name = "BananaCanvas",
            transform =
            {
                parent = Instance.transform
            }
        };
        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        canvasObject.AddComponent<GraphicRaycaster>();

        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;

        // Blocker object setup
        var blockerObject = new GameObject
        {
            name = "BananaBlocker",
            transform =
            {
                parent = canvasObject.transform
            }
        };

        var blocker = blockerObject.AddComponent<Image>();
        blocker.color = Color.clear;

        _blockerTransform = blocker.GetComponent<RectTransform>();
        _blockerTransform.sizeDelta = new Vector2(50, 50);
    }

    private void Update()
    {
        if (_loaded && _blocking > 0)
        {
            _blockerTransform.position = Input.mousePosition;
        }
    }

    private void LateUpdate()
    {
        if (_loaded && _blocking > 0)
        {
            Imgui.LateUpdate();
        }
    }

    internal static void StartBlocking()
    {
        if (++_blocking > 0)
        {
            _blockerTransform.gameObject.SetActive(true);
        }
        Instance.Logger.LogDebug($"Blocking++: {_blocking}");
    }

    internal static void StopBlocking(int count = 1)
    {
        _blocking -= count;
        if (_blocking == 0)
        {
            _blockerTransform.gameObject.SetActive(false);
        }
        Instance.Logger.LogDebug($"Blocking--{count}: {_blocking}");
    }
}