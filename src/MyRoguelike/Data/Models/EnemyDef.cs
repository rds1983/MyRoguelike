namespace MyRoguelike.Data.Models;

public class EnemyDef
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Tier { get; set; }
    public StatBlock Stats { get; set; }
    public int Hp { get; set; }
    public int Mp { get; set; }
    public int XpReward { get; set; }
    public IntRange? GoldReward { get; set; }
    public string? LootTable { get; set; }
    public List<string> Abilities { get; set; } = [];
    public string? SpecialAttack { get; set; }
    public List<string> SpawnBiomes { get; set; } = [];
    public string Behavior { get; set; } = "aggressive";
    public string Description { get; set; } = string.Empty;
}
