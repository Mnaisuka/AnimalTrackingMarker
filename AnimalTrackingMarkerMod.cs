using System.Reflection;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(AnimalTrackingMarker.AnimalTrackingMarkerMod), "AnimalTrackingMarker", "1.0.0", "Mnaisuka")]
[assembly: MelonGame("Hinterland", "TheLongDark")]

namespace AnimalTrackingMarker;

public sealed class AnimalTrackingMarkerMod : MelonMod
{
    private static AnimalTrackingMarkerSettings? settings;
    private static GUIStyle? markerStyle;
    private static Camera? cachedCamera;
    private static float nextScanTime;
    private static bool sceneAvailable = true;
    private static bool markersVisible = true;

    private static readonly Dictionary<AnimalKind, string> AnimalNames = new()
    {
        { AnimalKind.Crow, "Crow" },
        { AnimalKind.Rabbit, "Rabbit" },
        { AnimalKind.Stag, "Stag" },
        { AnimalKind.Doe, "Doe" },
        { AnimalKind.Wolf, "Wolf" },
        { AnimalKind.Timberwolf, "Timberwolf" },
        { AnimalKind.Bear, "Bear" },
        { AnimalKind.Moose, "Moose" },
        { AnimalKind.Ptarmigan, "Ptarmigan" },
        { AnimalKind.Cougar, "Cougar" }
    };

    public override void OnInitializeMelon()
    {
        MelonLogger.Msg($"[AnimalTrackingMarker] Version {Assembly.GetExecutingAssembly().GetName().Version}");
        settings = new AnimalTrackingMarkerSettings();
        settings.AddToModSettings("Animal Tracking");
    }

    public override void OnUpdate()
    {
        if (!sceneAvailable || settings == null || !settings.enableMarkers ||
            !settings.useToggleKey)
        {
            return;
        }

        if (InputManager.GetKeyDown(InputManager.m_CurrentContext, settings.toggleKey))
        {
            markersVisible = !markersVisible;
        }
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        sceneAvailable = !sceneName.Contains("MainMenu", StringComparison.OrdinalIgnoreCase);
        AnimalTargetRegistry.Clear();
        cachedCamera = null;
        nextScanTime = 0f;
        markersVisible = true;
    }

    public override void OnGUI()
    {
        if (!sceneAvailable || settings == null || !settings.enableMarkers ||
            (settings.useToggleKey && !markersVisible))
        {
            return;
        }

        Transform player = GameManager.GetPlayerTransform();
        if (!player)
        {
            return;
        }

        if (Time.unscaledTime >= nextScanTime)
        {
            AnimalTargetRegistry.RemoveUnavailableTargets();
            nextScanTime = Time.unscaledTime + 0.5f;
        }

        Camera? camera = GetCamera();
        if (camera is null || !camera)
        {
            return;
        }

        GUIStyle style = markerStyle ??= CreateMarkerStyle();

        foreach (AnimalTarget target in AnimalTargetRegistry.Targets)
        {
            if (!target.GameObject || !target.GameObject.activeInHierarchy ||
                !settings.IsAnimalEnabled(target.Kind))
            {
                continue;
            }

            Vector3 targetPosition = target.GameObject.transform.position;
            float distance = Vector3.Distance(player.position, targetPosition);
            if (distance > settings.detectionRange)
            {
                continue;
            }

            Vector3 markerPosition = targetPosition + Vector3.up * GetHeightOffset(target.Kind);
            Vector3 screenPosition = camera.WorldToScreenPoint(markerPosition);
            if (screenPosition.z <= 0f ||
                screenPosition.x < 0f || screenPosition.x > Screen.width ||
                screenPosition.y < 0f || screenPosition.y > Screen.height)
            {
                continue;
            }

            string label = settings.showDistance
                ? $"{AnimalNames[target.Kind]} [{distance:F0} m]"
                : AnimalNames[target.Kind];
            float screenX = screenPosition.x;
            float screenY = Screen.height - screenPosition.y;
            Vector2 textSize = style.CalcSize(new GUIContent(label));
            Rect rect = new(
                screenX - textSize.x / 2f,
                screenY - textSize.y / 2f,
                textSize.x,
                textSize.y
            );

            if (settings.showTextOutline)
            {
                style.normal.textColor = Color.black;
                GUI.Label(new Rect(rect.x - 1f, rect.y, rect.width, rect.height), label, style);
                GUI.Label(new Rect(rect.x + 1f, rect.y, rect.width, rect.height), label, style);
                GUI.Label(new Rect(rect.x, rect.y - 1f, rect.width, rect.height), label, style);
                GUI.Label(new Rect(rect.x, rect.y + 1f, rect.width, rect.height), label, style);
            }

            style.normal.textColor = GetMarkerTextColor(target.Kind);
            GUI.Label(rect, label, style);
        }
    }

    private static GUIStyle CreateMarkerStyle()
    {
        GUIStyle style = new(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 15,
            fontStyle = FontStyle.Bold,
            wordWrap = false
        };
        style.normal.textColor = GetTextColor(settings?.textColor ?? MarkerTextColor.白色);
        return style;
    }

    private static Color GetMarkerTextColor(AnimalKind kind)
    {
        if (settings == null || !settings.useSeparateDangerColors)
        {
            return GetTextColor(settings?.textColor ?? MarkerTextColor.白色);
        }

        MarkerTextColor color = IsDangerousAnimal(kind)
            ? settings.dangerousAnimalTextColor
            : settings.nonDangerousAnimalTextColor;
        return GetTextColor(color);
    }

    private static bool IsDangerousAnimal(AnimalKind kind)
    {
        return kind is AnimalKind.Wolf or AnimalKind.Timberwolf or
            AnimalKind.Bear or AnimalKind.Moose or AnimalKind.Cougar;
    }

    private static Color GetTextColor(MarkerTextColor color)
    {
        return color switch
        {
            MarkerTextColor.红色 => Color.red,
            MarkerTextColor.绿色 => Color.green,
            MarkerTextColor.青色 => Color.cyan,
            MarkerTextColor.黄色 => Color.yellow,
            MarkerTextColor.蓝色 => Color.blue,
            _ => Color.white
        };
    }

    private static Camera? GetCamera()
    {
        if (cachedCamera is not null && cachedCamera &&
            cachedCamera.enabled && cachedCamera.gameObject.activeInHierarchy)
        {
            return cachedCamera;
        }

        cachedCamera = null;
        foreach (Camera camera in Camera.allCameras)
        {
            if (camera && camera.enabled && camera.gameObject.activeInHierarchy &&
                camera.name == "CameraGlobalRT")
            {
                cachedCamera = camera;
                return camera;
            }
        }

        return null;
    }

    private static float GetHeightOffset(AnimalKind kind)
    {
        return kind switch
        {
            AnimalKind.Crow or AnimalKind.Ptarmigan => 0.5f,
            AnimalKind.Rabbit => 0.4f,
            AnimalKind.Bear or AnimalKind.Moose => 1.8f,
            _ => 1.2f
        };
    }

    [HarmonyPatch(typeof(BaseAi), "Start")]
    private static class BaseAiStartPatch
    {
        private static void Postfix(BaseAi __instance)
        {
            if (!__instance || !__instance.gameObject ||
                __instance.m_CurrentMode == AiMode.Dead ||
                __instance.m_CurrentMode == AiMode.Disabled ||
                __instance.m_CurrentMode == AiMode.None)
            {
                return;
            }

            AnimalKind? kind = GetAnimalKind(__instance);
            if (kind.HasValue)
            {
                AnimalTargetRegistry.Register(__instance.gameObject, kind.Value);
            }
        }

        private static AnimalKind? GetAnimalKind(BaseAi animal)
        {
            if (animal.m_AiSubType == AiSubType.Moose)
            {
                return AnimalKind.Moose;
            }

            if (animal.m_AiSubType == AiSubType.Bear)
            {
                return AnimalKind.Bear;
            }

            if (animal.m_AiSubType == AiSubType.Cougar)
            {
                return AnimalKind.Cougar;
            }

            if (animal.m_AiSubType == AiSubType.Wolf)
            {
                return animal.gameObject.name.Contains("grey", StringComparison.OrdinalIgnoreCase)
                    ? AnimalKind.Timberwolf
                    : AnimalKind.Wolf;
            }

            if (animal.m_AiSubType == AiSubType.Stag)
            {
                return animal.gameObject.name.Contains("_Doe", StringComparison.OrdinalIgnoreCase)
                    ? AnimalKind.Doe
                    : AnimalKind.Stag;
            }

            if (animal.m_SnowImprintType == SnowImprintType.PtarmiganFootprint)
            {
                return AnimalKind.Ptarmigan;
            }

            return animal.m_AiSubType == AiSubType.Rabbit ? AnimalKind.Rabbit : null;
        }
    }

    [HarmonyPatch(typeof(FlockChild), "Start")]
    private static class FlockChildStartPatch
    {
        private static void Postfix(FlockChild __instance)
        {
            if (__instance && __instance.gameObject)
            {
                AnimalTargetRegistry.Register(__instance.gameObject, AnimalKind.Crow);
            }
        }
    }
}
