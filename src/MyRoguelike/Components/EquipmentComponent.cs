using MyRoguelike.Components;
using MyRoguelike.Data.Models;

namespace MyRoguelike.Entities;

public class EquipmentComponent : IComponent
{
    public Item? Weapon { get; private set; }
    public Item? Armor { get; private set; }
    public Item? Shield { get; private set; }
    public Item? Accessory1 { get; private set; }
    public Item? Accessory2 { get; private set; }

    public bool IsEquipped(Item item) =>
        Weapon == item || Armor == item || Shield == item ||
        Accessory1 == item || Accessory2 == item;

    public bool Equip(Item item)
    {
        if (item.Def == null) return false;

        var slot = item.Def.Category switch
        {
            "weapon" => EquipmentSlot.Weapon,
            "armor" => EquipmentSlot.Armor,
            "shield" => EquipmentSlot.Shield,
            "accessory" => GetFreeAccessorySlot(),
            _ => (EquipmentSlot?)null
        };

        if (slot == null) return false;
        return EquipToSlot(item, slot.Value);
    }

    private EquipmentSlot GetFreeAccessorySlot()
    {
        if (Accessory1 == null) return EquipmentSlot.Accessory1;
        return EquipmentSlot.Accessory2;
    }

    private bool EquipToSlot(Item item, EquipmentSlot slot)
    {
        UnequipSlot(slot);
        switch (slot)
        {
            case EquipmentSlot.Weapon:     Weapon = item; break;
            case EquipmentSlot.Armor:      Armor = item; break;
            case EquipmentSlot.Shield:     Shield = item; break;
            case EquipmentSlot.Accessory1: Accessory1 = item; break;
            case EquipmentSlot.Accessory2: Accessory2 = item; break;
        }
        return true;
    }

    public void UnequipSlot(EquipmentSlot slot)
    {
        switch (slot)
        {
            case EquipmentSlot.Weapon:     Weapon = null; break;
            case EquipmentSlot.Armor:      Armor = null; break;
            case EquipmentSlot.Shield:     Shield = null; break;
            case EquipmentSlot.Accessory1: Accessory1 = null; break;
            case EquipmentSlot.Accessory2: Accessory2 = null; break;
        }
    }

    public void Unequip(Item item)
    {
        if (Weapon == item)     { Weapon = null; return; }
        if (Armor == item)      { Armor = null; return; }
        if (Shield == item)     { Shield = null; return; }
        if (Accessory1 == item) { Accessory1 = null; return; }
        if (Accessory2 == item) { Accessory2 = null; return; }
    }

    public StatBlock GetTotalStatBonuses()
    {
        var total = new StatBlock();
        foreach (var item in GetAllEquipped())
        {
            if (item.Def == null) continue;
            total = new StatBlock
            {
                Strength = total.Strength + (item.Def.Stats.GetValueOrDefault("strength", 0)),
                Dexterity = total.Dexterity + (item.Def.Stats.GetValueOrDefault("dexterity", 0)),
                Constitution = total.Constitution + (item.Def.Stats.GetValueOrDefault("constitution", 0)),
                Intelligence = total.Intelligence + (item.Def.Stats.GetValueOrDefault("intelligence", 0)),
                Wisdom = total.Wisdom + (item.Def.Stats.GetValueOrDefault("wisdom", 0))
            };
        }
        return total;
    }

    public Dictionary<string, int> GetAllBonuses()
    {
        var bonuses = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in GetAllEquipped())
        {
            if (item.Def == null) continue;
            foreach (var kvp in item.Def.Stats)
            {
                bonuses[kvp.Key] = bonuses.GetValueOrDefault(kvp.Key) + kvp.Value;
            }
        }
        return bonuses;
    }

    public IEnumerable<Item> GetAllEquipped()
    {
        if (Weapon != null) yield return Weapon;
        if (Armor != null) yield return Armor;
        if (Shield != null) yield return Shield;
        if (Accessory1 != null) yield return Accessory1;
        if (Accessory2 != null) yield return Accessory2;
    }
}

public enum EquipmentSlot
{
    Weapon,
    Armor,
    Shield,
    Accessory1,
    Accessory2
}
