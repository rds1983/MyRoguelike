namespace MyRoguelike.Data.Models;

public class ClassDef
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public StatBlock BaseStats { get; set; }
    public StatBlock StatGrowth { get; set; }
    public int HpPerLevel { get; set; }
    public int MpPerLevel { get; set; }
    public List<string> Skills { get; set; } = [];
    public List<string> AllowedArmor { get; set; } = [];
    public List<string> AllowedWeapons { get; set; } = [];
    public StartingEquipmentDef? StartingEquipment { get; set; }
}

public class StartingEquipmentDef
{
    public string? Weapon { get; set; }
    public string? Armor { get; set; }
    public string? Shield { get; set; }
}
