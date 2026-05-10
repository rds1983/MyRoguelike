using MyRoguelike.Data;
using MyRoguelike.Entities;

namespace MyRoguelike.Tests;

public class EntityComponentTests
{
    public EntityComponentTests()
    {
        // Set up DataManager for tests that need it (Enemy loot generation)
        if (Game1.Data == null)
        {
            var dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Json");
            var dm = new DataManager(dataDir);
            dm.LoadAll();
            Game1.Data = dm;
        }
    }

    // ── Entity ───────────────────────────────────────────────────────

    [Fact]
    public void Entity_AddComponent_StoresAndRetrieves()
    {
        var entity = new Entity();
        entity.AddComponent<StatsComponent>();

        Assert.True(entity.HasComponent<StatsComponent>());
        Assert.NotNull(entity.GetComponent<StatsComponent>());
    }

    [Fact]
    public void Entity_GetComponent_Missing_ReturnsNull()
    {
        var entity = new Entity();
        Assert.Null(entity.GetComponent<StatsComponent>());
    }

    [Fact]
    public void Entity_RemoveComponent_RemovesIt()
    {
        var entity = new Entity();
        entity.AddComponent<StatsComponent>();
        entity.RemoveComponent<StatsComponent>();

        Assert.False(entity.HasComponent<StatsComponent>());
    }

    [Fact]
    public void Entity_HasMultipleComponents()
    {
        var entity = new Entity();
        entity.AddComponent<StatsComponent>();
        entity.AddComponent<InventoryComponent>();
        entity.AddComponent<EquipmentComponent>();

        Assert.True(entity.HasComponent<StatsComponent>());
        Assert.True(entity.HasComponent<InventoryComponent>());
        Assert.True(entity.HasComponent<EquipmentComponent>());
    }

    [Fact]
    public void Entity_Components_ReadOnlyList()
    {
        var entity = new Entity();
        entity.AddComponent<StatsComponent>();
        Assert.Single(entity.Components);
    }

    // ── StatsComponent ───────────────────────────────────────────────

    [Fact]
    public void Stats_SetHp_SetsCorrectValues()
    {
        var stats = new StatsComponent();
        stats.SetHp(100);

        Assert.Equal(100, stats.MaxHp);
        Assert.Equal(100, stats.CurrentHp);
    }

    [Fact]
    public void Stats_SetHp_WithCurrent_SetsCorrectly()
    {
        var stats = new StatsComponent();
        stats.SetHp(100, 50);

        Assert.Equal(100, stats.MaxHp);
        Assert.Equal(50, stats.CurrentHp);
    }

    [Fact]
    public void Stats_ApplyDamage_ReducesHp()
    {
        var stats = new StatsComponent();
        stats.SetHp(100);
        stats.ApplyDamage(30);

        Assert.Equal(70, stats.CurrentHp);
    }

    [Fact]
    public void Stats_ApplyDamage_ReturnsDamageDealt()
    {
        var stats = new StatsComponent();
        stats.SetHp(100);
        var dealt = stats.ApplyDamage(30);

        Assert.Equal(30, dealt);
    }

    [Fact]
    public void Stats_ApplyDamage_ClampsToZero()
    {
        var stats = new StatsComponent();
        stats.SetHp(20);
        stats.ApplyDamage(100);

        Assert.Equal(0, stats.CurrentHp);
        Assert.False(stats.IsAlive);
    }

    [Fact]
    public void Stats_ApplyDamage_NegativeIsZero()
    {
        var stats = new StatsComponent();
        stats.SetHp(100);
        stats.ApplyDamage(-10);

        Assert.Equal(100, stats.CurrentHp);
    }

    [Fact]
    public void Stats_Heal_RestoresHp()
    {
        var stats = new StatsComponent();
        stats.SetHp(100, 30);
        stats.Heal(40);

        Assert.Equal(70, stats.CurrentHp);
    }

    [Fact]
    public void Stats_Heal_ReturnsActualHealedAmount()
    {
        var stats = new StatsComponent();
        stats.SetHp(100, 30);
        var healed = stats.Heal(40);

        Assert.Equal(40, healed);
    }

    [Fact]
    public void Stats_Heal_DoesNotExceedMax()
    {
        var stats = new StatsComponent();
        stats.SetHp(100, 80);
        stats.Heal(50);

        Assert.Equal(100, stats.CurrentHp);
    }

    [Fact]
    public void Stats_RestoreMana_Works()
    {
        var stats = new StatsComponent();
        stats.SetMp(50);
        stats.CurrentMp = 10;
        stats.RestoreMana(20);

        Assert.Equal(30, stats.CurrentMp);
    }

    [Fact]
    public void Stats_IsAlive_WhenHpZero_ReturnsFalse()
    {
        var stats = new StatsComponent();
        stats.SetHp(100);
        stats.ApplyDamage(100);

        Assert.False(stats.IsAlive);
    }

    [Fact]
    public void Stats_AddBonusStats_IncreasesTotal()
    {
        var stats = new StatsComponent();
        stats.BaseStats = new Data.Models.StatBlock { Strength = 10, Dexterity = 10 };
        stats.AddBonusStats(new Data.Models.StatBlock { Strength = 5, Dexterity = 3 });

        Assert.Equal(15, stats.TotalStrength);
        Assert.Equal(13, stats.TotalDexterity);
    }

    [Fact]
    public void Stats_RemoveBonusStats_DecreasesTotal()
    {
        var stats = new StatsComponent();
        stats.BaseStats = new Data.Models.StatBlock { Strength = 10 };
        stats.AddBonusStats(new Data.Models.StatBlock { Strength = 5 });
        stats.RemoveBonusStats(new Data.Models.StatBlock { Strength = 3 });

        Assert.Equal(12, stats.TotalStrength);
    }

    // ── InventoryComponent & Item ────────────────────────────────────

    [Fact]
    public void Inventory_AddItem_AddsToInventory()
    {
        var inv = new InventoryComponent();
        var item = new Item { Id = "iron_sword", Quantity = 1 };

        var result = inv.AddItem(item);

        Assert.True(result);
        Assert.Equal(1, inv.Count);
    }

    [Fact]
    public void Inventory_AddItem_WhenFull_ReturnsFalse()
    {
        var inv = new InventoryComponent { Capacity = 1 };
        inv.AddItem(new Item { Id = "item1" });

        var result = inv.AddItem(new Item { Id = "item2" });

        Assert.False(result);
    }

    [Fact]
    public void Inventory_AddItem_Stackable_Stacks()
    {
        var inv = new InventoryComponent();
        var potion = new Item { Id = "health_potion", Quantity = 1, IsStackable = true };
        inv.AddItem(potion);

        inv.AddItem(new Item { Id = "health_potion", Quantity = 2, IsStackable = true });

        Assert.Equal(1, inv.Count); // still one slot
        Assert.Equal(3, inv.Items[0].Quantity);
    }

    [Fact]
    public void Inventory_RemoveItem_ReducesQuantity()
    {
        var inv = new InventoryComponent();
        inv.AddItem(new Item { Id = "health_potion", Quantity = 5, IsStackable = true });

        var result = inv.RemoveItem("health_potion", 2);

        Assert.True(result);
        Assert.Equal(3, inv.Items[0].Quantity);
    }

    [Fact]
    public void Inventory_RemoveItem_RemovesSlotWhenZero()
    {
        var inv = new InventoryComponent();
        inv.AddItem(new Item { Id = "iron_sword", Quantity = 1 });

        var result = inv.RemoveItem("iron_sword");

        Assert.True(result);
        Assert.Empty(inv.Items);
    }

    [Fact]
    public void Inventory_HasItem_ReturnsTrue()
    {
        var inv = new InventoryComponent();
        inv.AddItem(new Item { Id = "iron_sword" });

        Assert.True(inv.HasItem("iron_sword"));
    }

    [Fact]
    public void Inventory_HasItem_Missing_ReturnsFalse()
    {
        var inv = new InventoryComponent();
        Assert.False(inv.HasItem("nonexistent"));
    }

    [Fact]
    public void Inventory_CountItem_ReturnsCorrectTotal()
    {
        var inv = new InventoryComponent();
        inv.AddItem(new Item { Id = "health_potion", Quantity = 3, IsStackable = true });
        inv.AddItem(new Item { Id = "health_potion", Quantity = 2, IsStackable = true });

        Assert.Equal(5, inv.CountItem("health_potion"));
    }

    [Fact]
    public void Inventory_Clear_EmptiesInventory()
    {
        var inv = new InventoryComponent();
        inv.AddItem(new Item { Id = "iron_sword" });
        inv.Clear();

        Assert.Empty(inv.Items);
    }

    // ── EquipmentComponent ───────────────────────────────────────────

    [Fact]
    public void Equipment_EquipWeapon_SetsSlot()
    {
        var eq = new EquipmentComponent();
        var sword = new Item { Id = "iron_sword", Def = Game1.Data.GetItem("iron_sword") };

        eq.Equip(sword);

        Assert.Same(sword, eq.Weapon);
    }

    [Fact]
    public void Equipment_EquipArmor_SetsSlot()
    {
        var eq = new EquipmentComponent();
        var armor = new Item { Id = "leather_armor", Def = Game1.Data.GetItem("leather_armor") };

        eq.Equip(armor);

        Assert.Same(armor, eq.Armor);
    }

    [Fact]
    public void Equipment_EquipAccessory_FillsSlot1First()
    {
        var eq = new EquipmentComponent();
        var ring1 = new Item { Id = "simple_ring", Def = Game1.Data.GetItem("simple_ring") };
        var ring2 = new Item { Id = "simple_ring", Def = Game1.Data.GetItem("simple_ring") };

        eq.Equip(ring1);
        eq.Equip(ring2);

        Assert.Same(ring1, eq.Accessory1);
        Assert.Same(ring2, eq.Accessory2);
    }

    [Fact]
    public void Equipment_EquipReplacesOccupiedSlot()
    {
        var eq = new EquipmentComponent();
        var sword1 = new Item { Id = "iron_sword", Def = Game1.Data.GetItem("iron_sword") };
        var sword2 = new Item { Id = "iron_sword", Def = Game1.Data.GetItem("iron_sword") };

        eq.Equip(sword1);
        eq.Equip(sword2);

        Assert.Same(sword2, eq.Weapon);
    }

    [Fact]
    public void Equipment_Unequip_ClearsSlot()
    {
        var eq = new EquipmentComponent();
        var sword = new Item { Id = "iron_sword", Def = Game1.Data.GetItem("iron_sword") };
        eq.Equip(sword);
        eq.Unequip(sword);

        Assert.Null(eq.Weapon);
    }

    [Fact]
    public void Equipment_IsEquipped_ReturnsTrue()
    {
        var eq = new EquipmentComponent();
        var sword = new Item { Id = "iron_sword", Def = Game1.Data.GetItem("iron_sword") };
        eq.Equip(sword);

        Assert.True(eq.IsEquipped(sword));
    }

    [Fact]
    public void Equipment_IsEquipped_NotEquipped_ReturnsFalse()
    {
        var eq = new EquipmentComponent();
        var sword = new Item { Id = "iron_sword", Def = Game1.Data.GetItem("iron_sword") };

        Assert.False(eq.IsEquipped(sword));
    }

    [Fact]
    public void Equipment_GetAllEquipped_ReturnsAll()
    {
        var eq = new EquipmentComponent();
        eq.Equip(new Item { Id = "iron_sword", Def = Game1.Data.GetItem("iron_sword") });
        eq.Equip(new Item { Id = "leather_armor", Def = Game1.Data.GetItem("leather_armor") });

        Assert.Equal(2, eq.GetAllEquipped().Count());
    }

    [Fact]
    public void Equipment_GetTotalBonuses_Accumulates()
    {
        var eq = new EquipmentComponent();
        var sword = new Item
        {
            Id = "iron_sword",
            Def = new Data.Models.ItemDef
            {
                Id = "test_sword", Name = "Test Sword", Category = "weapon",
                Stats = new Dictionary<string, int> { { "strength", 3 }, { "attack", 5 } }
            }
        };
        eq.Equip(sword);

        var bonuses = eq.GetTotalStatBonuses();
        Assert.Equal(3, bonuses.Strength);
    }

    // ── EffectComponent ──────────────────────────────────────────────

    [Fact]
    public void Effects_ApplyEffect_Adds()
    {
        var ec = new EffectComponent();
        ec.ApplyEffect(new ActiveEffect { EffectId = "poison", RemainingTurns = 3 });

        Assert.Single(ec.Effects);
    }

    [Fact]
    public void Effects_ApplyEffect_DuplicateRefreshesDuration()
    {
        var ec = new EffectComponent();
        ec.ApplyEffect(new ActiveEffect { EffectId = "poison", RemainingTurns = 2 });
        ec.ApplyEffect(new ActiveEffect { EffectId = "poison", RemainingTurns = 5 });

        Assert.Single(ec.Effects);
        Assert.Equal(5, ec.Effects[0].RemainingTurns);
    }

    [Fact]
    public void Effects_TickEffects_ReducesDuration()
    {
        var ec = new EffectComponent();
        ec.ApplyEffect(new ActiveEffect { EffectId = "poison", RemainingTurns = 2 });
        ec.TickEffects();

        Assert.Equal(1, ec.Effects[0].RemainingTurns);
    }

    [Fact]
    public void Effects_TickEffects_RemovesExpired()
    {
        var ec = new EffectComponent();
        ec.ApplyEffect(new ActiveEffect { EffectId = "poison", RemainingTurns = 1 });
        ec.TickEffects();

        Assert.Empty(ec.Effects);
    }

    [Fact]
    public void Effects_HasEffect_ReturnsTrue()
    {
        var ec = new EffectComponent();
        ec.ApplyEffect(new ActiveEffect { EffectId = "stun", RemainingTurns = 2 });

        Assert.True(ec.HasEffect("stun"));
    }

    [Fact]
    public void Effects_RemoveEffect_Removes()
    {
        var ec = new EffectComponent();
        ec.ApplyEffect(new ActiveEffect { EffectId = "poison", RemainingTurns = 3 });
        ec.RemoveEffect("poison");

        Assert.Empty(ec.Effects);
    }

    [Fact]
    public void Effects_Clear_RemovesAll()
    {
        var ec = new EffectComponent();
        ec.ApplyEffect(new ActiveEffect { EffectId = "poison", RemainingTurns = 3 });
        ec.ApplyEffect(new ActiveEffect { EffectId = "stun", RemainingTurns = 2 });
        ec.Clear();

        Assert.Empty(ec.Effects);
    }

    // ── Player ───────────────────────────────────────────────────────

    [Fact]
    public void Player_StartsAtLevel1()
    {
        var player = new Player();
        Assert.Equal(1, player.Level);
    }

    [Fact]
    public void Player_AddXp_LevelsUp()
    {
        var player = new Player { ExperienceToNext = 100 };
        player.AddXp(100);

        Assert.Equal(2, player.Level);
    }

    [Fact]
    public void Player_LevelUp_GrantsStatPoints()
    {
        var player = new Player { ExperienceToNext = 100 };
        player.AddXp(100);

        Assert.Equal(2, player.Level);
        Assert.Equal(3, player.StatPointsAvailable);
    }

    [Fact]
    public void Player_AddXp_CarriesOverExcess()
    {
        var player = new Player { ExperienceToNext = 100 };
        player.AddXp(350);

        Assert.True(player.Level >= 3);
        Assert.True(player.Experience < player.ExperienceToNext);
    }

    [Fact]
    public void Player_LevelCapsAt50()
    {
        var player = new Player { ExperienceToNext = 100 };
        player.AddXp(200_000);

        Assert.Equal(50, player.Level);
    }

    [Fact]
    public void Player_CalculateXpForLevel_Increases()
    {
        Assert.Equal(100, Player.CalculateXpForLevel(1));
        Assert.Equal(500, Player.CalculateXpForLevel(5));
    }

    [Fact]
    public void Player_GetClassDef_ReturnsValidClass()
    {
        var player = new Player { ClassId = "warrior" };
        var def = player.GetClassDef();

        Assert.NotNull(def);
        Assert.Equal("Warrior", def.Name);
    }

    [Fact]
    public void Player_GetClassDef_UnknownId_ReturnsNull()
    {
        var player = new Player { ClassId = "nonexistent" };
        Assert.Null(player.GetClassDef());
    }

    // ── Enemy ────────────────────────────────────────────────────────

    [Fact]
    public void Enemy_GetDef_ReturnsValidDef()
    {
        var enemy = new Enemy { EnemyDefId = "goblin_scout" };
        var def = enemy.GetDef();

        Assert.NotNull(def);
        Assert.Equal("Goblin Scout", def.Name);
    }

    [Fact]
    public void Enemy_GenerateGold_ReturnsWithinRange()
    {
        var enemy = new Enemy { EnemyDefId = "goblin_scout" };
        for (var i = 0; i < 20; i++)
        {
            var gold = enemy.GenerateGold();
            Assert.InRange(gold, 1, 5);
        }
    }

    [Fact]
    public void Enemy_GenerateLoot_ReturnsItems()
    {
        var enemy = new Enemy { EnemyDefId = "goblin_scout" };
        var loot = enemy.GenerateLoot();

        Assert.NotNull(loot);
    }

    [Fact]
    public void Enemy_GenerateLoot_NoTable_ReturnsEmpty()
    {
        var enemy = new Enemy { EnemyDefId = "bat" };
        var loot = enemy.GenerateLoot();

        Assert.NotNull(loot);
    }

    // ── Npc ──────────────────────────────────────────────────────────

    [Fact]
    public void Npc_Shopkeeper_HasShop()
    {
        var npc = new Npc { NpcType = NpcType.Shopkeeper };
        Assert.True(npc.HasShop);
    }

    [Fact]
    public void Npc_Citizen_DoesNotHaveShop()
    {
        var npc = new Npc { NpcType = NpcType.Citizen };
        Assert.False(npc.HasShop);
    }

    [Fact]
    public void Npc_DialogueLines_CanBeAdded()
    {
        var npc = new Npc();
        npc.DialogueLines.Add("Hello!");
        npc.DialogueLines.Add("Goodbye!");

        Assert.Equal(2, npc.DialogueLines.Count);
    }

    // ── CombatComponent ──────────────────────────────────────────────

    [Fact]
    public void Combat_Recalculate_ComputesCorrectValues()
    {
        var stats = new StatsComponent
        {
            BaseStats = new Data.Models.StatBlock { Strength = 14, Constitution = 12, Intelligence = 10 }
        };
        var eq = new EquipmentComponent();
        var sword = new Item
        {
            Id = "iron_sword",
            Def = new Data.Models.ItemDef
            {
                Id = "iron_sword", Name = "Iron Sword", Category = "weapon",
                Stats = new Dictionary<string, int> { { "attack", 5 } }
            }
        };
        eq.Equip(sword);
        eq.Equip(new Item
        {
            Id = "leather_armor",
            Def = new Data.Models.ItemDef
            {
                Id = "leather_armor", Name = "Leather Armor", Category = "armor",
                Stats = new Dictionary<string, int> { { "defense", 4 } }
            }
        });

        var combat = new CombatComponent();
        combat.Recalculate(stats, eq);

        Assert.Equal(14 * 2 + 5, combat.AttackPower); // STR*2 + attack bonus
        Assert.Equal(12 + 4, combat.Defense);          // CON + defense bonus
    }
}
