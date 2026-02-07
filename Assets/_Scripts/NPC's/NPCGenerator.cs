using UnityEngine;
using static CharacterStats;

public class NPCGenerator : MonoBehaviour
{
    [SerializeField] int npcCount = 5;
    [SerializeField] string npcNamePrefix = "NPC";

    public void Awake() => Generate();

    public void Generate()
    {
        if (npcCount < 1) return;

        for (var i = 0; i < npcCount; i++)
        {
            var stats = ScriptableObject.CreateInstance<CharacterStats>();
            stats.Randomize();

            var highest = stats.GetHighestStat();
            var go = new GameObject($"{npcNamePrefix}_{i + 1}");

            Character c;
            if (highest == StatType.Strength) c = go.AddComponent<Fighter>();
            else if (highest == StatType.Resourcefulness) c = go.AddComponent<Cook>();
            else if (highest == StatType.Fieldcraft) c = go.AddComponent<Explorer>();
            else c = go.AddComponent<Worker>();

            c.Initialize(stats);

            Debug.Log($"{go.name} -> Class: {c.Class} | C:{stats.Creativity} S:{stats.Strength} R:{stats.Resourcefulness} F:{stats.Fieldcraft} E:{stats.Efficiency}");
        }
    }

}