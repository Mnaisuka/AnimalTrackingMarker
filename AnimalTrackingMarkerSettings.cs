using ModSettings;
using UnityEngine;

namespace AnimalTrackingMarker;

internal sealed class AnimalTrackingMarkerSettings : JsonModSettings
{
    [Section("General")]

    [Name("Enable Animal Markers")]
    [Description("Enable or disable animal markers on the screen.")]
    public bool enableMarkers = true;

    [Name("Detection Range")]
    [Description("Only show animal markers within this range.")]
    [Slider(0, 500)]
    public int detectionRange = 300;

    [Name("Show Distance")]
    [Description("Show the distance from the player after each animal's name.")]
    public bool showDistance = true;

    [Name("Toggle Markers with a Key")]
    [Description("Show or hide animal markers by pressing the toggle key.")]
    public bool useToggleKey = true;

    [Name("Toggle Key")]
    [Description("The key used to show or hide animal markers.")]
    public KeyCode toggleKey = KeyCode.Keypad0;

    [Name("Text Color")]
    [Description("Set the color of animal marker text.")]
    public MarkerTextColor textColor = MarkerTextColor.白色;

    [Name("Use Separate Colors for Dangerous and Non-Dangerous Animals")]
    [Description("Use separate text colors for dangerous and non-dangerous animals.")]
    public bool useSeparateDangerColors = true;

    [Name("Dangerous Animal Text Color")]
    [Description("Used for wolves, timberwolves, bears, moose, and cougars.")]
    public MarkerTextColor dangerousAnimalTextColor = MarkerTextColor.红色;

    [Name("Non-Dangerous Animal Text Color")]
    [Description("Used for crows, rabbits, stags, does, and ptarmigans.")]
    public MarkerTextColor nonDangerousAnimalTextColor = MarkerTextColor.白色;

    [Name("Show Text Outline")]
    [Description("Draw a black outline around text to improve readability against busy backgrounds.")]
    public bool showTextOutline = false;

    [Section("Animal Types")]

    [Name("Show Crows")]
    public bool showCrows = false;

    [Name("Show Rabbits")]
    public bool showRabbits = true;

    [Name("Show Stags")]
    public bool showStags = true;

    [Name("Show Does")]
    public bool showDoes = true;

    [Name("Show Wolves")]
    public bool showWolves = true;

    [Name("Show Timberwolves")]
    public bool showTimberwolves = true;

    [Name("Show Bears")]
    public bool showBears = true;

    [Name("Show Moose")]
    public bool showMoose = true;

    [Name("Show Ptarmigans")]
    public bool showPtarmigans = true;

    [Name("Show Cougars")]
    public bool showCougars = true;

    public bool IsAnimalEnabled(AnimalKind kind)
    {
        return kind switch
        {
            AnimalKind.Crow => showCrows,
            AnimalKind.Rabbit => showRabbits,
            AnimalKind.Stag => showStags,
            AnimalKind.Doe => showDoes,
            AnimalKind.Wolf => showWolves,
            AnimalKind.Timberwolf => showTimberwolves,
            AnimalKind.Bear => showBears,
            AnimalKind.Moose => showMoose,
            AnimalKind.Ptarmigan => showPtarmigans,
            AnimalKind.Cougar => showCougars,
            _ => false
        };
    }
}

internal enum MarkerTextColor
{
    [Name("White")]
    白色,
    [Name("Red")]
    红色,
    [Name("Green")]
    绿色,
    [Name("Cyan")]
    青色,
    [Name("Yellow")]
    黄色,
    [Name("Blue")]
    蓝色
}
