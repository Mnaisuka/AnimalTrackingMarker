using UnityEngine;

namespace AnimalTrackingMarker;

internal enum AnimalKind
{
    Crow,
    Rabbit,
    Stag,
    Doe,
    Wolf,
    Timberwolf,
    Bear,
    Moose,
    Ptarmigan,
    Cougar
}

internal sealed class AnimalTarget
{
    public AnimalTarget(GameObject gameObject, AnimalKind kind)
    {
        GameObject = gameObject;
        Kind = kind;
    }

    public GameObject GameObject { get; }
    public AnimalKind Kind { get; }
}

internal static class AnimalTargetRegistry
{
    private static readonly Dictionary<int, AnimalTarget> trackedAnimals = new();

    public static Dictionary<int, AnimalTarget>.ValueCollection Targets => trackedAnimals.Values;

    public static void Register(GameObject gameObject, AnimalKind kind)
    {
        trackedAnimals[gameObject.GetInstanceID()] = new AnimalTarget(gameObject, kind);
    }

    public static void RemoveUnavailableTargets()
    {
        List<int> unavailable = new();
        foreach (KeyValuePair<int, AnimalTarget> pair in trackedAnimals)
        {
            if (!pair.Value.GameObject || !pair.Value.GameObject.activeInHierarchy)
            {
                unavailable.Add(pair.Key);
            }
        }

        foreach (int instanceId in unavailable)
        {
            trackedAnimals.Remove(instanceId);
        }
    }

    public static void Clear()
    {
        trackedAnimals.Clear();
    }
}
