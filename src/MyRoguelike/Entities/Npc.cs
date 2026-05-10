namespace MyRoguelike.Entities;

public enum NpcType
{
    Shopkeeper,
    QuestGiver,
    Citizen,
    Innkeeper
}

public class Npc : Entity
{
    public NpcType NpcType { get; set; } = NpcType.Citizen;
    public List<string> DialogueLines { get; set; } = [];
    public List<Item> ShopInventory { get; set; } = [];
    public bool HasShop => NpcType == NpcType.Shopkeeper || NpcType == NpcType.Innkeeper;
}
