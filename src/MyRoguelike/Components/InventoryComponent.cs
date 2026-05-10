using MyRoguelike.Components;
using MyRoguelike.Data.Models;

namespace MyRoguelike.Entities;

public class Item
{
    public string Id { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public bool IsIdentified { get; set; } = true;

    public ItemDef? Def { get; set; }

    public string DisplayName => Def?.Name ?? Id;

    public bool IsStackable { get; set; }

    public void UpdateStackable()
    {
        if (Def is { Category: "potion" or "scroll" or "material" })
            IsStackable = true;
    }
}

public class InventoryComponent : IComponent
{
    private readonly List<Item> _items = [];
    public int Capacity { get; set; } = Constants.MaxInventorySize;

    public IReadOnlyList<Item> Items => _items.AsReadOnly();
    public int Count => _items.Count;
    public bool IsFull => Count >= Capacity;

    public bool AddItem(Item item)
    {
        if (IsFull) return false;

        if (item.IsStackable)
        {
            var existing = _items.FirstOrDefault(i => i.Id == item.Id && i.IsIdentified == item.IsIdentified);
            if (existing != null)
            {
                existing.Quantity += item.Quantity;
                return true;
            }
        }

        _items.Add(item);
        return true;
    }

    public bool RemoveItem(string itemId, int quantity = 1)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item == null) return false;

        if (item.IsStackable && item.Quantity > quantity)
        {
            item.Quantity -= quantity;
            return true;
        }

        return _items.Remove(item);
    }

    public bool HasItem(string itemId)
    {
        return _items.Any(i => i.Id == itemId);
    }

    public int CountItem(string itemId)
    {
        return _items.Where(i => i.Id == itemId).Sum(i => i.Quantity);
    }

    public void Clear()
    {
        _items.Clear();
    }
}
