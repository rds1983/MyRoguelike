using MyRoguelike.Entities;

namespace MyRoguelike.Systems;

public static class EventSystem
{
    public delegate void EntityKilledHandler(Entity killed, Entity? killer);
    public delegate void EntityDamagedHandler(Entity damaged, Entity? attacker, int damage);
    public delegate void LevelUpHandler(Player player, int newLevel);
    public delegate void ItemPickedUpHandler(Player player, Item item);

    private static event EntityKilledHandler? _entityKilled;
    private static event EntityDamagedHandler? _entityDamaged;
    private static event LevelUpHandler? _levelUp;
    private static event ItemPickedUpHandler? _itemPickedUp;

    public static event EntityKilledHandler? EntityKilled
    {
        add => _entityKilled += value;
        remove => _entityKilled -= value;
    }

    public static event EntityDamagedHandler? EntityDamaged
    {
        add => _entityDamaged += value;
        remove => _entityDamaged -= value;
    }

    public static event LevelUpHandler? LevelUp
    {
        add => _levelUp += value;
        remove => _levelUp -= value;
    }

    public static event ItemPickedUpHandler? ItemPickedUp
    {
        add => _itemPickedUp += value;
        remove => _itemPickedUp -= value;
    }

    public static void RaiseEntityKilled(Entity killed, Entity? killer) =>
        _entityKilled?.Invoke(killed, killer);

    public static void RaiseEntityDamaged(Entity damaged, Entity? attacker, int damage) =>
        _entityDamaged?.Invoke(damaged, attacker, damage);

    public static void RaiseLevelUp(Player player, int newLevel) =>
        _levelUp?.Invoke(player, newLevel);

    public static void RaiseItemPickedUp(Player player, Item item) =>
        _itemPickedUp?.Invoke(player, item);

    public static void Reset()
    {
        _entityKilled = null;
        _entityDamaged = null;
        _levelUp = null;
        _itemPickedUp = null;
    }
}
