using MyRoguelike.Data;

namespace MyRoguelike.Tests;

public class DataManagerTests
{
    private static DataManager CreateDataManager()
    {
        var dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Json");
        return new DataManager(dataDir);
    }

    [Fact]
    public void LoadAll_LoadsAllDataFiles_Successfully()
    {
        var dm = CreateDataManager();
        var result = dm.LoadAll();

        Assert.True(result);
    }

    [Fact]
    public void LoadAll_LoadsFiveClasses()
    {
        var dm = CreateDataManager();
        dm.LoadAll();

        Assert.Equal(5, dm.Classes.Count);
    }

    [Theory]
    [InlineData("warrior")]
    [InlineData("mage")]
    [InlineData("wizard")]
    [InlineData("cleric")]
    [InlineData("monk")]
    public void GetClass_ValidId_ReturnsClass(string id)
    {
        var dm = CreateDataManager();
        dm.LoadAll();

        var cls = dm.GetClass(id);
        Assert.NotNull(cls);
        Assert.Equal(id, cls.Id);
    }

    [Fact]
    public void LoadAll_LoadsTenEnemies()
    {
        var dm = CreateDataManager();
        dm.LoadAll();

        Assert.Equal(10, dm.Enemies.Count);
    }

    [Theory]
    [InlineData("goblin_scout")]
    [InlineData("giant_rat")]
    [InlineData("slime")]
    [InlineData("skeleton")]
    [InlineData("bat")]
    [InlineData("spider")]
    [InlineData("wolf")]
    [InlineData("bandit")]
    [InlineData("mushroom_man")]
    [InlineData("fire_sprite")]
    public void GetEnemy_ValidId_ReturnsEnemy(string id)
    {
        var dm = CreateDataManager();
        dm.LoadAll();

        var enemy = dm.GetEnemy(id);
        Assert.NotNull(enemy);
        Assert.Equal(id, enemy.Id);
    }

    [Fact]
    public void LoadAll_LoadsTenItems()
    {
        var dm = CreateDataManager();
        dm.LoadAll();

        Assert.Equal(10, dm.Items.Count);
    }

    [Theory]
    [InlineData("iron_sword", "weapon")]
    [InlineData("leather_armor", "armor")]
    [InlineData("wooden_shield", "shield")]
    [InlineData("simple_ring", "accessory")]
    public void GetItem_ValidId_ReturnsCorrectCategory(string id, string expectedCategory)
    {
        var dm = CreateDataManager();
        dm.LoadAll();

        var item = dm.GetItem(id);
        Assert.NotNull(item);
        Assert.Equal(id, item.Id);
        Assert.Equal(expectedCategory, item.Category);
    }

    [Fact]
    public void LoadAll_LoadsFivePotions()
    {
        var dm = CreateDataManager();
        dm.LoadAll();

        Assert.Equal(5, dm.Potions.Count);
    }

    [Fact]
    public void LoadAll_LoadsFiveScrolls()
    {
        var dm = CreateDataManager();
        dm.LoadAll();

        Assert.Equal(5, dm.Scrolls.Count);
    }

    [Fact]
    public void LoadAll_LoadsFifteenTiles()
    {
        var dm = CreateDataManager();
        dm.LoadAll();

        Assert.Equal(15, dm.Tiles.Count);
    }

    [Fact]
    public void LoadAll_LoadsFiveLootTables()
    {
        var dm = CreateDataManager();
        dm.LoadAll();

        Assert.Equal(5, dm.LootTables.Count);
    }

    [Fact]
    public void LoadAll_LoadsSixSpecialAttacks()
    {
        var dm = CreateDataManager();
        dm.LoadAll();

        Assert.Equal(6, dm.SpecialAttacks.Count);
    }

    [Fact]
    public void GetClass_InvalidId_ReturnsNull()
    {
        var dm = CreateDataManager();
        dm.LoadAll();

        Assert.Null(dm.GetClass("nonexistent"));
    }

    [Fact]
    public void GetEnemy_InvalidId_ReturnsNull()
    {
        var dm = CreateDataManager();
        dm.LoadAll();

        Assert.Null(dm.GetEnemy("nonexistent"));
    }

    [Fact]
    public void GetItem_InvalidId_ReturnsNull()
    {
        var dm = CreateDataManager();
        dm.LoadAll();

        Assert.Null(dm.GetItem("nonexistent"));
    }

    [Fact]
    public void Warrior_HasCorrectBaseStats()
    {
        var dm = CreateDataManager();
        dm.LoadAll();

        var warrior = dm.GetClass("warrior");
        Assert.NotNull(warrior);
        Assert.Equal(14, warrior.BaseStats.Strength);
        Assert.Equal(10, warrior.BaseStats.Dexterity);
        Assert.Equal(14, warrior.BaseStats.Constitution);
        Assert.Equal(8, warrior.BaseStats.Intelligence);
        Assert.Equal(10, warrior.BaseStats.Wisdom);
    }

    [Fact]
    public void Mage_HasHighIntelligence()
    {
        var dm = CreateDataManager();
        dm.LoadAll();

        var mage = dm.GetClass("mage");
        Assert.NotNull(mage);
        Assert.Equal(16, mage.BaseStats.Intelligence);
        Assert.Equal(6, mage.HpPerLevel);
        Assert.Equal(10, mage.MpPerLevel);
    }

    [Fact]
    public void GoblinScout_HasCorrectLootTable()
    {
        var dm = CreateDataManager();
        dm.LoadAll();

        var goblin = dm.GetEnemy("goblin_scout");
        Assert.NotNull(goblin);
        Assert.Equal("goblin", goblin.LootTable);
        Assert.Equal("aggressive", goblin.Behavior);
    }

    [Fact]
    public void Spider_HasWebTrapSpecialAttack()
    {
        var dm = CreateDataManager();
        dm.LoadAll();

        var spider = dm.GetEnemy("spider");
        Assert.NotNull(spider);
        Assert.Equal("web_trap", spider.SpecialAttack);

        var webTrap = dm.GetSpecialAttack("web_trap");
        Assert.NotNull(webTrap);
    }

    [Fact]
    public void DragonBreath_HasCorrectValues()
    {
        var dm = CreateDataManager();
        dm.LoadAll();

        var breath = dm.GetSpecialAttack("dragon_breath");
        Assert.NotNull(breath);
        Assert.Equal("cone_aoe", breath.Type);
        Assert.Equal("fire", breath.Element);
        Assert.Equal(3.0, breath.DamageMultiplier);
        Assert.Equal(4, breath.Cooldown);
    }

    [Fact]
    public void HealthPotion_Restores30HP()
    {
        var dm = CreateDataManager();
        dm.LoadAll();

        var potion = dm.GetPotion("health_potion");
        Assert.NotNull(potion);
        Assert.Equal("heal", potion.EffectType);
        Assert.Equal(30, potion.EffectValue);
    }

    [Fact]
    public void GrassTile_IsWalkableAndTransparent()
    {
        var dm = CreateDataManager();
        dm.LoadAll();

        var grass = dm.GetTile("grass");
        Assert.NotNull(grass);
        Assert.True(grass.IsWalkable);
        Assert.True(grass.IsTransparent);
    }

    [Fact]
    public void StoneWall_IsNotWalkableAndNotTransparent()
    {
        var dm = CreateDataManager();
        dm.LoadAll();

        var wall = dm.GetTile("stone_wall");
        Assert.NotNull(wall);
        Assert.False(wall.IsWalkable);
        Assert.False(wall.IsTransparent);
    }

    [Fact]
    public void LootTable_EntriesHaveValidWeights()
    {
        var dm = CreateDataManager();
        dm.LoadAll();

        foreach (var table in dm.LootTables.Values)
        {
            foreach (var entry in table.Entries)
            {
                Assert.True(entry.Weight > 0, $"Entry {entry.ItemId} in {table.Id} has non-positive weight");
                Assert.True(entry.Min <= entry.Max, $"Entry {entry.ItemId} in {table.Id} has Min > Max");
            }
        }
    }

    [Fact]
    public void AllEnemies_HavePositiveHP()
    {
        var dm = CreateDataManager();
        dm.LoadAll();

        foreach (var enemy in dm.Enemies.Values)
        {
            Assert.True(enemy.Hp > 0, $"Enemy {enemy.Id} has non-positive HP");
        }
    }

    [Fact]
    public void AllEnemies_HavePositiveXpReward()
    {
        var dm = CreateDataManager();
        dm.LoadAll();

        foreach (var enemy in dm.Enemies.Values)
        {
            Assert.True(enemy.XpReward > 0, $"Enemy {enemy.Id} has non-positive XP reward");
        }
    }

    [Fact]
    public void AllItems_HavePositiveValue()
    {
        var dm = CreateDataManager();
        dm.LoadAll();

        foreach (var item in dm.Items.Values)
        {
            Assert.True(item.Value > 0, $"Item {item.Id} has non-positive value");
        }
    }

    [Fact]
    public void Classes_AllHaveStartingEquipment()
    {
        var dm = CreateDataManager();
        dm.LoadAll();

        foreach (var cls in dm.Classes.Values)
        {
            Assert.NotNull(cls.StartingEquipment);
            Assert.NotNull(cls.StartingEquipment.Weapon);
            Assert.NotNull(cls.StartingEquipment.Armor);
        }
    }

    [Fact]
    public void AllTiles_HaveNonNullColor()
    {
        var dm = CreateDataManager();
        dm.LoadAll();

        foreach (var tile in dm.Tiles.Values)
        {
            Assert.NotNull(tile.Color);
        }
    }
}
