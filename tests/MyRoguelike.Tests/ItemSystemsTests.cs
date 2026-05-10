using Microsoft.Xna.Framework;
using MyRoguelike.Data;
using MyRoguelike.Entities;
using MyRoguelike.Systems;
using MyRoguelike.World;

namespace MyRoguelike.Tests;

public class ItemSystemsTests
{
    public ItemSystemsTests()
    {
        if (Game1.Data == null)
        {
            var dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Json");
            var dm = new DataManager(dataDir);
            dm.LoadAll();
            Game1.Data = dm;
        }
    }

    [Fact]
    public void ItemFactory_Create_Potion_IsStackableAndCategorized()
    {
        var item = ItemFactory.Create("health_potion", 2);
        Assert.NotNull(item);
        Assert.Equal("potion", item!.Def!.Category);
        Assert.True(item.IsStackable);
        Assert.Equal(2, item.Quantity);
    }

    [Fact]
    public void EffectSystem_ApplyPotion_HealRestoresHp()
    {
        var player = new Player { Id = "p", Name = "Hero" };
        var stats = player.AddComponent<StatsComponent>();
        player.AddComponent<EffectComponent>();
        stats.BaseStats = new MyRoguelike.Data.Models.StatBlock { Constitution = 10 };
        stats.SetHp(100, 50);

        var potion = Game1.Data.GetPotion("health_potion")!;
        var ok = EffectSystem.ApplyPotion(player, potion, out var msg);

        Assert.True(ok);
        Assert.Contains("Recovered", msg);
        Assert.True(stats.CurrentHp > 50);
    }

    [Fact]
    public void EffectSystem_BuffExpires_RemovesBonusStats()
    {
        var player = new Player { Id = "p", Name = "Hero" };
        var stats = player.AddComponent<StatsComponent>();
        player.AddComponent<EffectComponent>();
        stats.BaseStats = new MyRoguelike.Data.Models.StatBlock { Strength = 10 };
        stats.BonusStats = new MyRoguelike.Data.Models.StatBlock();
        stats.SetHp(100, 100);

        var potion = Game1.Data.GetPotion("strength_potion")!;
        var ok = EffectSystem.ApplyPotion(player, potion, out _);
        Assert.True(ok);
        Assert.Equal(14, stats.TotalStrength);

        // Duration is 5 turns in json
        for (var i = 0; i < 5; i++)
            EffectSystem.Tick(player);

        Assert.Equal(10, stats.TotalStrength);
    }

    [Fact]
    public void ItemUseSystem_TeleportScroll_MovesPlayerToWalkableTile()
    {
        var player = new Player { Id = "p", Name = "Hero", Position = new Point(2, 2) };
        player.AddComponent<StatsComponent>();
        player.AddComponent<EquipmentComponent>();
        player.AddComponent<InventoryComponent>();
        player.AddComponent<EffectComponent>();

        var map = new Map(30, 30, "stone_floor");
        var scroll = ItemFactory.Create("scroll_teleportation")!;

        var ok = ItemUseSystem.TryUse(player, scroll, map, enemies: null, out var msg);
        Assert.True(ok);
        Assert.Contains("reappear", msg, StringComparison.OrdinalIgnoreCase);
        Assert.True(map.IsWalkable(player.Position.X, player.Position.Y));
    }
}

