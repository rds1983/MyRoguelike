namespace MyRoguelike.Data.Models;

public class ItemDef
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Subcategory { get; set; } = string.Empty;
    public int Tier { get; set; }
    public string Rarity { get; set; } = "common";
    public Dictionary<string, int> Stats { get; set; } = [];
    public int Value { get; set; }
    public string Description { get; set; } = string.Empty;
}
