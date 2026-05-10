using Microsoft.Xna.Framework;
using MyRoguelike.Data;
using MyRoguelike.Data.Models;
using MyRoguelike.Entities;
using MyRoguelike.Systems;
using MyRoguelike.UI;
using MyRoguelike.World;

namespace MyRoguelike.Tests;

public class CombatSystemTests
{
    public CombatSystemTests()
    {
        if (Game1.Data == null)
        {
            var dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Json");
            var dm = new DataManager(dataDir);
            dm.LoadAll();
            Game1.Data = dm;
        }
        EventSystem.Reset();
    }

    private static (Entity attacker, Entity defender) CreateCombatants()
    {
        var attacker = new Entity { Id = "attacker", Name = "Attacker" };
        var attackerStats = attacker.AddComponent<StatsComponent>();
        attackerStats.BaseStats = new StatBlock { Strength = 14, Dexterity = 10, Constitution = 12 };
        attackerStats.SetHp(50);
        var attackerCombat = attacker.AddComponent<CombatComponent>();
        attackerCombat.Recalculate(attackerStats, new EquipmentComponent());

        var defender = new Entity { Id = "defender", Name = "Defender" };
        var defenderStats = defender.AddComponent<StatsComponent>();
        defenderStats.BaseStats = new StatBlock { Strength = 10, Dexterity = 8, Constitution = 10 };
        defenderStats.SetHp(30);
        var defenderCombat = defender.AddComponent<CombatComponent>();
        defenderCombat.Recalculate(defenderStats, new EquipmentComponent());

        return (attacker, defender);
    }

    [Fact]
    public void MeleeAttack_Hit_DealsDamage()
    {
        var (attacker, defender) = CreateCombatants();

        var results = new List<CombatResult>();
        for (var i = 0; i < 50; i++)
        {
            var defender2 = new Entity { Id = "defender", Name = "Defender" };
            var ds = defender2.AddComponent<StatsComponent>();
            ds.SetHp(100);
            ds.BaseStats = new StatBlock { Strength = 10, Dexterity = 8, Constitution = 10 };
            var dc = defender2.AddComponent<CombatComponent>();
            dc.Recalculate(ds, new EquipmentComponent());

            var result = CombatSystem.MeleeAttack(attacker, defender2);
            results.Add(result);
        }

        var hitResults = results.Where(r => r.Hit).ToList();
        Assert.True(hitResults.Count > 0, "Should have at least some hits");
        Assert.All(hitResults, r => Assert.True(r.Damage > 0));
    }

    [Fact]
    public void MeleeAttack_Hit_MessageContainsNames()
    {
        var (attacker, defender) = CreateCombatants();
        var result = CombatSystem.MeleeAttack(attacker, defender);

        if (result.Hit)
        {
            Assert.Contains("Attacker", result.Message);
            Assert.Contains("Defender", result.Message);
        }
        else
        {
            Assert.Contains("misses", result.Message);
        }
    }

    [Fact]
    public void MeleeAttack_CanCrit()
    {
        var (attacker, defender) = CreateCombatants();

        var results = new List<CombatResult>();
        for (var i = 0; i < 200; i++)
        {
            var defender2 = new Entity { Id = "defender", Name = "Defender" };
            var ds = defender2.AddComponent<StatsComponent>();
            ds.SetHp(1000);
            ds.BaseStats = new StatBlock { Strength = 10, Dexterity = 8, Constitution = 10 };
            var dc = defender2.AddComponent<CombatComponent>();
            dc.Recalculate(ds, new EquipmentComponent());

            var result = CombatSystem.MeleeAttack(attacker, defender2);
            results.Add(result);
        }

        Assert.True(results.Any(r => r.Crit), "Should have at least one crit in 200 attempts");
    }

    [Fact]
    public void MeleeAttack_Kill_SetsTargetKilled()
    {
        var (attacker, defender) = CreateCombatants();
        var defenderStats = defender.GetComponent<StatsComponent>()!;
        defenderStats.SetHp(1);

        var result = CombatSystem.MeleeAttack(attacker, defender);

        if (result.Hit)
            Assert.True(result.TargetKilled);
    }

    [Fact]
    public void MeleeAttack_MissingComponents_ReturnsFailure()
    {
        var attacker = new Entity { Id = "a", Name = "A" };
        var defender = new Entity { Id = "b", Name = "B" };

        var result = CombatSystem.MeleeAttack(attacker, defender);

        Assert.False(result.Hit);
        Assert.NotEmpty(result.Message);
    }

    [Fact]
    public void MeleeAttack_EventSystem_InvokesDamaged()
    {
        var (attacker, defender) = CreateCombatants();
        Entity? damagedEntity = null;
        Entity? damageAttacker = null;
        var damageValue = 0;

        EventSystem.EntityDamaged += (d, a, dmg) =>
        {
            damagedEntity = d;
            damageAttacker = a;
            damageValue = dmg;
        };

        var result = CombatSystem.MeleeAttack(attacker, defender);

        if (result.Hit)
        {
            Assert.Same(defender, damagedEntity);
            Assert.Same(attacker, damageAttacker);
            Assert.Equal(result.Damage, damageValue);
        }
    }

    [Fact]
    public void MeleeAttack_EventSystem_InvokesKilled()
    {
        var (attacker, defender) = CreateCombatants();
        var defenderStats = defender.GetComponent<StatsComponent>()!;
        defenderStats.SetHp(1);
        Entity? killedEntity = null;

        EventSystem.EntityKilled += (killed, killer) => { killedEntity = killed; };

        var result = CombatSystem.MeleeAttack(attacker, defender);

        if (result.Hit)
            Assert.Same(defender, killedEntity);
    }
}

public class TurnSystemTests
{
    [Fact]
    public void AddEntity_AddsToQueue()
    {
        var ts = new TurnSystem();
        var e = new Entity { Id = "test" };
        ts.AddEntity(e, 10);

        Assert.Same(e, ts.CurrentActor);
        Assert.Equal(1, ts.ActorCount);
    }

    [Fact]
    public void AddEntity_SortsBySpeed()
    {
        var ts = new TurnSystem();
        var slow = new Entity { Id = "slow" };
        var fast = new Entity { Id = "fast" };
        ts.AddEntity(slow, 5);
        ts.AddEntity(fast, 20);

        Assert.Same(fast, ts.CurrentActor);
    }

    [Fact]
    public void NextTurn_Advances()
    {
        var ts = new TurnSystem();
        var e1 = new Entity { Id = "e1" };
        var e2 = new Entity { Id = "e2" };
        ts.AddEntity(e1, 10);
        ts.AddEntity(e2, 5);

        ts.NextTurn();
        Assert.Same(e2, ts.CurrentActor);
    }

    [Fact]
    public void NextTurn_WrapsAround()
    {
        var ts = new TurnSystem();
        var e1 = new Entity { Id = "e1" };
        var e2 = new Entity { Id = "e2" };
        ts.AddEntity(e1, 10);
        ts.AddEntity(e2, 5);

        ts.NextTurn();
        ts.NextTurn();

        Assert.Same(e1, ts.CurrentActor);
        Assert.Equal(2, ts.TurnNumber);
    }

    [Fact]
    public void RemoveEntity_RemovesFromQueue()
    {
        var ts = new TurnSystem();
        var e1 = new Entity { Id = "e1" };
        var e2 = new Entity { Id = "e2" };
        ts.AddEntity(e1, 10);
        ts.AddEntity(e2, 5);

        ts.RemoveEntity(e1);
        Assert.Same(e2, ts.CurrentActor);
        Assert.Equal(1, ts.ActorCount);
    }

    [Fact]
    public void RemoveEntity_AdjustsIndex()
    {
        var ts = new TurnSystem();
        var e1 = new Entity { Id = "e1" };
        var e2 = new Entity { Id = "e2" };
        var e3 = new Entity { Id = "e3" };
        ts.AddEntity(e1, 30);
        ts.AddEntity(e2, 20);
        ts.AddEntity(e3, 10);

        ts.NextTurn();
        ts.RemoveEntity(e1);

        Assert.Same(e2, ts.CurrentActor);
    }

    [Fact]
    public void Clear_RemovesAll()
    {
        var ts = new TurnSystem();
        ts.AddEntity(new Entity { Id = "e1" }, 10);
        ts.AddEntity(new Entity { Id = "e2" }, 5);
        ts.Clear();

        Assert.Equal(0, ts.ActorCount);
        Assert.Null(ts.CurrentActor);
    }

    [Fact]
    public void CurrentActor_Empty_ReturnsNull()
    {
        var ts = new TurnSystem();
        Assert.Null(ts.CurrentActor);
    }
}

public class AiSystemTests
{
    public AiSystemTests()
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
    public void GetAction_Adjacent_ReturnsMeleeAttack()
    {
        var enemy = new Enemy { Id = "e", Name = "Goblin", Position = new Point(5, 5) };
        enemy.AddComponent<StatsComponent>();
        enemy.AddComponent<CombatComponent>();
        enemy.AddComponent<AiComponent>();

        var player = new Entity { Id = "p", Name = "Player", Position = new Point(5, 6) };
        player.AddComponent<StatsComponent>();

        var map = new Map(20, 20);
        var action = AiSystem.GetAction(enemy, player, map);

        Assert.Equal(AiActionType.MeleeAttack, action.Type);
        Assert.Same(player, action.Target);
    }

    [Fact]
    public void GetAction_InRange_ReturnsMove()
    {
        var enemy = new Enemy { Id = "e", Name = "Goblin", Position = new Point(5, 5) };
        enemy.AddComponent<StatsComponent>();
        enemy.AddComponent<CombatComponent>();
        var ai = enemy.AddComponent<AiComponent>();
        ai.DetectionRange = 8;

        var player = new Entity { Id = "p", Name = "Player", Position = new Point(8, 5) };
        player.AddComponent<StatsComponent>();

        var map = new Map(20, 20);
        var action = AiSystem.GetAction(enemy, player, map);

        Assert.Equal(AiActionType.Move, action.Type);
    }

    [Fact]
    public void GetAction_OutOfRange_ReturnsIdle()
    {
        var enemy = new Enemy { Id = "e", Name = "Goblin", Position = new Point(0, 0) };
        enemy.AddComponent<StatsComponent>();
        enemy.AddComponent<CombatComponent>();
        var ai = enemy.AddComponent<AiComponent>();
        ai.DetectionRange = 8;

        var player = new Entity { Id = "p", Name = "Player", Position = new Point(19, 19) };
        player.AddComponent<StatsComponent>();

        var map = new Map(20, 20);
        var action = AiSystem.GetAction(enemy, player, map);

        Assert.Equal(AiActionType.Idle, action.Type);
    }

    [Fact]
    public void GetAction_NoAiComponent_ReturnsIdle()
    {
        var enemy = new Enemy { Id = "e", Name = "Goblin" };
        var player = new Entity { Id = "p", Name = "Player" };
        var map = new Map(20, 20);

        var action = AiSystem.GetAction(enemy, player, map);

        Assert.Equal(AiActionType.Idle, action.Type);
    }
}

public class PathfindingSystemTests
{
    public PathfindingSystemTests()
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
    public void FindPath_Adjacent_ReturnsTwoPoints()
    {
        var map = new Map(10, 10);
        var start = new Point(5, 5);
        var end = new Point(5, 6);

        var path = PathfindingSystem.FindPath(map, start, end);

        Assert.Equal(2, path.Count);
        Assert.Equal(start, path[0]);
        Assert.Equal(end, path[1]);
    }

    [Fact]
    public void FindPath_SamePoint_ReturnsSinglePoint()
    {
        var map = new Map(10, 10);
        var point = new Point(5, 5);

        var path = PathfindingSystem.FindPath(map, point, point);

        Assert.Single(path);
        Assert.Equal(point, path[0]);
    }

    [Fact]
    public void FindPath_AroundObstacle()
    {
        var map = new Map(10, 10);
        map.Fill("grass");
        map.SetTile(5, 5, "stone_wall");
        map.SetTile(5, 6, "stone_wall");

        var start = new Point(4, 5);
        var end = new Point(6, 5);

        var path = PathfindingSystem.FindPath(map, start, end);

        Assert.True(path.Count > 0);
        Assert.Equal(end, path[^1]);
    }

    [Fact]
    public void FindPath_WalledOff_ReturnsEmpty()
    {
        var map = new Map(5, 5);
        map.Fill("stone_wall");

        var start = new Point(0, 0);
        var end = new Point(4, 4);

        var path = PathfindingSystem.FindPath(map, start, end);

        Assert.Empty(path);
    }

    [Fact]
    public void FindPath_OutOfBounds_ReturnsEmpty()
    {
        var map = new Map(5, 5);
        var start = new Point(-1, 0);
        var end = new Point(10, 10);

        var path = PathfindingSystem.FindPath(map, start, end);

        Assert.Empty(path);
    }

    [Fact]
    public void FindPath_WithBlockedPredicate_RespectsBlocked()
    {
        var map = new Map(10, 10);
        map.Fill("grass");
        var start = new Point(0, 0);
        var end = new Point(2, 0);

        var path = PathfindingSystem.FindPath(map, start, end,
            p => p.X == 1 && p.Y == 0);

        Assert.NotEmpty(path);
        Assert.Equal(end, path[^1]);
        Assert.DoesNotContain(path, p => p.X == 1 && p.Y == 0);
    }
}

public class MessageLogTests
{
    [Fact]
    public void Add_StoresMessage()
    {
        var log = new MessageLog();
        log.Add("Hello", Color.White);

        Assert.Single(log.Entries);
        Assert.Equal("Hello", log.Entries[0].Message);
        Assert.Equal(Color.White, log.Entries[0].Color);
    }

    [Fact]
    public void Add_DefaultColor_IsWhite()
    {
        var log = new MessageLog();
        log.Add("Hello");

        Assert.Equal(Color.White, log.Entries[0].Color);
    }

    [Fact]
    public void Add_MaxEntries_TrimsOldest()
    {
        var log = new MessageLog();
        for (var i = 0; i < 110; i++)
            log.Add($"Message {i}");

        Assert.Equal(100, log.Count);
        Assert.Equal("Message 10", log.Entries[0].Message);
    }

    [Fact]
    public void Clear_RemovesAll()
    {
        var log = new MessageLog();
        log.Add("Hello");
        log.Add("World");
        log.Clear();

        Assert.Empty(log.Entries);
    }
}

public class EventSystemTests
{
    [Fact]
    public void EntityDamaged_InvokesEvent()
    {
        EventSystem.Reset();

        Entity? damaged = null;
        Entity? attacker = null;
        var damageValue = 0;

        EventSystem.EntityDamaged += (d, a, dmg) =>
        {
            damaged = d;
            attacker = a;
            damageValue = dmg;
        };

        var entity = new Entity { Id = "target" };
        var source = new Entity { Id = "source" };
        EventSystem.RaiseEntityDamaged(entity, source, 15);

        Assert.Same(entity, damaged);
        Assert.Same(source, attacker);
        Assert.Equal(15, damageValue);

        EventSystem.Reset();
    }

    [Fact]
    public void EntityKilled_InvokesEvent()
    {
        EventSystem.Reset();

        Entity? killed = null;
        Entity? killer = null;

        EventSystem.EntityKilled += (k, kr) =>
        {
            killed = k;
            killer = kr;
        };

        var entity = new Entity { Id = "target" };
        var source = new Entity { Id = "source" };
        EventSystem.RaiseEntityKilled(entity, source);

        Assert.Same(entity, killed);
        Assert.Same(source, killer);

        EventSystem.Reset();
    }

    [Fact]
    public void Reset_ClearsAllEvents()
    {
        var invoked = false;

        EventSystem.EntityDamaged += (_, _, _) => invoked = true;
        EventSystem.Reset();

        EventSystem.RaiseEntityDamaged(new Entity(), new Entity(), 1);
        Assert.False(invoked);
    }
}
