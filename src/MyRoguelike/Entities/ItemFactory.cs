using MyRoguelike.Data.Models;

namespace MyRoguelike.Entities;

public static class ItemFactory
{
    public static Item? Create(string id, int quantity = 1, bool identified = true)
    {
        var itemDef = Game1.Data.GetItem(id);
        if (itemDef != null)
        {
            var item = new Item { Id = id, Quantity = Math.Max(1, quantity), Def = itemDef, IsIdentified = identified };
            item.UpdateStackable();
            return item;
        }

        var potionDef = Game1.Data.GetPotion(id);
        if (potionDef != null)
        {
            var def = new ItemDef
            {
                Id = potionDef.Id,
                Name = potionDef.Name,
                Category = "potion",
                Subcategory = potionDef.EffectType,
                Tier = 1,
                Rarity = potionDef.Rarity,
                Stats = [],
                Value = potionDef.Value,
                Description = potionDef.Description
            };

            var item = new Item { Id = id, Quantity = Math.Max(1, quantity), Def = def, IsIdentified = identified };
            item.UpdateStackable();
            return item;
        }

        var scrollDef = Game1.Data.GetScroll(id);
        if (scrollDef != null)
        {
            var def = new ItemDef
            {
                Id = scrollDef.Id,
                Name = scrollDef.Name,
                Category = "scroll",
                Subcategory = scrollDef.EffectType,
                Tier = 1,
                Rarity = scrollDef.Rarity,
                Stats = [],
                Value = scrollDef.Value,
                Description = scrollDef.Description
            };

            var item = new Item { Id = id, Quantity = Math.Max(1, quantity), Def = def, IsIdentified = identified };
            item.UpdateStackable();
            return item;
        }

        return null;
    }
}

