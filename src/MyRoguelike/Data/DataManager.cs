using System.Diagnostics;
using System.Text.Json;
using MyRoguelike.Data.Converters;
using MyRoguelike.Data.Models;

namespace MyRoguelike.Data;

public class DataManager
{
    private readonly string _dataDir;

    public Dictionary<string, ClassDef> Classes { get; private set; } = [];
    public Dictionary<string, EnemyDef> Enemies { get; private set; } = [];
    public Dictionary<string, ItemDef> Items { get; private set; } = [];
    public Dictionary<string, PotionDef> Potions { get; private set; } = [];
    public Dictionary<string, ScrollDef> Scrolls { get; private set; } = [];
    public Dictionary<string, TileDef> Tiles { get; private set; } = [];
    public Dictionary<string, LootTableDef> LootTables { get; private set; } = [];
    public Dictionary<string, SpecialAttackDef> SpecialAttacks { get; private set; } = [];

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        Converters = { new ColorJsonConverter() }
    };

    public DataManager(string dataDir)
    {
        _dataDir = dataDir;
    }

    public bool LoadAll()
    {
        var success = true;

        Classes        = LoadData<ClassDef>("classes.json")         ?? Classes;
        Enemies        = LoadData<EnemyDef>("enemies.json")         ?? Enemies;
        Items          = LoadData<ItemDef>("items.json")            ?? Items;
        Potions        = LoadData<PotionDef>("potions.json")        ?? Potions;
        Scrolls        = LoadData<ScrollDef>("scrolls.json")        ?? Scrolls;
        Tiles          = LoadData<TileDef>("tiles.json")            ?? Tiles;
        LootTables     = LoadData<LootTableDef>("loot_tables.json") ?? LootTables;
        SpecialAttacks = LoadData<SpecialAttackDef>("special_attacks.json") ?? SpecialAttacks;

        success = Classes.Count > 0 && Enemies.Count > 0 && Items.Count > 0
               && Potions.Count > 0 && Scrolls.Count > 0 && Tiles.Count > 0
               && LootTables.Count > 0 && SpecialAttacks.Count > 0;

        if (success)
            ValidateReferences();

        return success;
    }

    private Dictionary<string, T> LoadData<T>(string fileName)
        where T : class
    {
        var path = Path.Combine(_dataDir, fileName);

        if (!File.Exists(path))
        {
            Debug.WriteLine($"[DataManager] WARNING: File not found: {path}");
            return [];
        }

        try
        {
            var json = File.ReadAllText(path);
            var items = JsonSerializer.Deserialize<List<T>>(json, _jsonOptions);

            if (items == null || items.Count == 0)
            {
                Debug.WriteLine($"[DataManager] WARNING: No items loaded from {fileName}");
                return [];
            }

            var dict = items.ToDictionary(
                item => GetId(item),
                item => item
            );

            Debug.WriteLine($"[DataManager] Loaded {dict.Count} entries from {fileName}");
            return dict;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[DataManager] ERROR loading {fileName}: {ex.Message}");
            return [];
        }
    }

    private static string GetId<T>(T item) where T : class
    {
        return item switch
        {
            ClassDef c => c.Id,
            EnemyDef e => e.Id,
            ItemDef i => i.Id,
            PotionDef p => p.Id,
            ScrollDef s => s.Id,
            TileDef t => t.Id,
            LootTableDef l => l.Id,
            SpecialAttackDef sa => sa.Id,
            _ => throw new ArgumentException($"Unknown type: {typeof(T).Name}")
        };
    }

    private void ValidateReferences()
    {
        // Validate enemy loot table references
        foreach (var enemy in Enemies.Values)
        {
            if (enemy.LootTable != null && !LootTables.ContainsKey(enemy.LootTable))
            {
                Debug.WriteLine($"[DataManager] WARNING: Enemy '{enemy.Id}' references unknown loot table '{enemy.LootTable}'");
            }

            if (enemy.SpecialAttack != null && !SpecialAttacks.ContainsKey(enemy.SpecialAttack))
            {
                Debug.WriteLine($"[DataManager] WARNING: Enemy '{enemy.Id}' references unknown special attack '{enemy.SpecialAttack}'");
            }
        }

        // Validate loot table item references
        foreach (var table in LootTables.Values)
        {
            foreach (var entry in table.Entries)
            {
                // "gold" is handled specially (currency) — skip validation
                if (entry.ItemId == "gold") continue;

                if (!Items.ContainsKey(entry.ItemId))
                {
                    Debug.WriteLine($"[DataManager] WARNING: Loot table '{table.Id}' references unknown item '{entry.ItemId}'");
                }
            }
        }

        // Validate starting equipment
        foreach (var cls in Classes.Values)
        {
            if (cls.StartingEquipment == null) continue;

            if (cls.StartingEquipment.Weapon != null && !Items.ContainsKey(cls.StartingEquipment.Weapon) && cls.StartingEquipment.Weapon != "unarmed")
            {
                Debug.WriteLine($"[DataManager] WARNING: Class '{cls.Id}' starting weapon '{cls.StartingEquipment.Weapon}' not found in items");
            }
            if (cls.StartingEquipment.Armor != null && !Items.ContainsKey(cls.StartingEquipment.Armor))
            {
                Debug.WriteLine($"[DataManager] WARNING: Class '{cls.Id}' starting armor '{cls.StartingEquipment.Armor}' not found in items");
            }
            if (cls.StartingEquipment.Shield != null && !Items.ContainsKey(cls.StartingEquipment.Shield))
            {
                Debug.WriteLine($"[DataManager] WARNING: Class '{cls.Id}' starting shield '{cls.StartingEquipment.Shield}' not found in items");
            }
        }
    }

    // Convenience accessors
    public ClassDef? GetClass(string id) => Classes.GetValueOrDefault(id);
    public EnemyDef? GetEnemy(string id) => Enemies.GetValueOrDefault(id);
    public ItemDef? GetItem(string id) => Items.GetValueOrDefault(id);
    public PotionDef? GetPotion(string id) => Potions.GetValueOrDefault(id);
    public ScrollDef? GetScroll(string id) => Scrolls.GetValueOrDefault(id);
    public TileDef? GetTile(string id) => Tiles.GetValueOrDefault(id);
    public LootTableDef? GetLootTable(string id) => LootTables.GetValueOrDefault(id);
    public SpecialAttackDef? GetSpecialAttack(string id) => SpecialAttacks.GetValueOrDefault(id);
}
