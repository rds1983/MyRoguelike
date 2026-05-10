namespace MyRoguelike.Data.Models;

public class ScrollDef
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string EffectType { get; set; } = string.Empty;
    public int EffectValue { get; set; }
    public string? Element { get; set; }
    public int Duration { get; set; }
    public int Range { get; set; }
    public int Value { get; set; }
    public string Rarity { get; set; } = "common";
    public string Description { get; set; } = string.Empty;
}
