using MyRoguelike.Data.Models;

namespace MyRoguelike.Entities;

public class Enemy : Entity
{
    public string EnemyDefId { get; set; } = string.Empty;

    public EnemyDef? GetDef()
    {
        return Game1.Data.GetEnemy(EnemyDefId);
    }

    public List<Item> GenerateLoot()
    {
        var items = new List<Item>();
        var def = GetDef();
        if (def?.LootTable == null) return items;

        var table = Game1.Data.GetLootTable(def.LootTable);
        if (table == null) return items;

        foreach (var entry in table.Entries)
        {
            var roll = Random.Shared.Next(0, 100);
            if (roll >= entry.Weight) continue;

            var quantity = entry.Min == entry.Max
                ? entry.Min
                : Random.Shared.Next(entry.Min, entry.Max + 1);

            if (entry.ItemId == "gold")
            {
                // Gold is handled directly
                continue;
            }

            var itemDef = Game1.Data.GetItem(entry.ItemId);
            items.Add(new Item
            {
                Id = entry.ItemId,
                Quantity = quantity,
                Def = itemDef
            });
        }

        return items;
    }

    public int GenerateGold()
    {
        var def = GetDef();
        if (def?.GoldReward == null) return 0;

        return def.GoldReward.Min == def.GoldReward.Max
            ? def.GoldReward.Min
            : Random.Shared.Next(def.GoldReward.Min, def.GoldReward.Max + 1);
    }
}
