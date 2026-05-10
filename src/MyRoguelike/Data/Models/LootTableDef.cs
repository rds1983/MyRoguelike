namespace MyRoguelike.Data.Models;

public class LootTableDef
{
    public string Id { get; set; } = string.Empty;
    public List<LootEntry> Entries { get; set; } = [];
}

public class LootEntry
{
    public string ItemId { get; set; } = string.Empty;
    public int Weight { get; set; }
    public int Min { get; set; } = 1;
    public int Max { get; set; } = 1;
}
