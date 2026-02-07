using UnityEngine;

[CreateAssetMenu(menuName = "NPC/Character Stats")]
public class CharacterStats : ScriptableObject
{
    public enum StatType
    {
        Creativity,
        Strength,
        Resourcefulness,
        Fieldcraft,
        Efficiency
    }

    public enum NPCSpecialization
    {
        Worker,
        Fighter,
        Cook,
        Explorer
    }

    [Range(1, 10)] public int Creativity = 1;
    [Range(1, 10)] public int Strength = 1;
    [Range(1, 10)] public int Resourcefulness = 1;
    [Range(1, 10)] public int Fieldcraft = 1;
    [Range(1, 10)] public int Efficiency = 1;

    public void Randomize(int min = 1, int max = 10)
    {
        Creativity = Random.Range(min, max + 1);
        Strength = Random.Range(min, max + 1);
        Resourcefulness = Random.Range(min, max + 1);
        Fieldcraft = Random.Range(min, max + 1);
        Efficiency = Random.Range(min, max + 1);
    }

    public int Get(StatType type)
    {
        if (type == StatType.Creativity) return Creativity;
        if (type == StatType.Strength) return Strength;
        if (type == StatType.Resourcefulness) return Resourcefulness;
        if (type == StatType.Fieldcraft) return Fieldcraft;
        return Efficiency;
    }

    public void Set(StatType type, int value)
    {
        if (type == StatType.Creativity) Creativity = value;
        else if (type == StatType.Strength) Strength = value;
        else if (type == StatType.Resourcefulness) Resourcefulness = value;
        else if (type == StatType.Fieldcraft) Fieldcraft = value;
        else Efficiency = value;
    }

    public StatType GetHighestStat()
    {
        var c = Creativity;
        var s = Strength;
        var r = Resourcefulness;
        var f = Fieldcraft;
        var e = Efficiency;

        var max = c;
        var type = StatType.Creativity;

        if (s > max) { max = s; type = StatType.Strength; }
        if (r > max) { max = r; type = StatType.Resourcefulness; }
        if (f > max) { max = f; type = StatType.Fieldcraft; }
        if (e > max) { max = e; type = StatType.Efficiency; }

        return type;
    }
}