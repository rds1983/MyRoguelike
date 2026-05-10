using MyRoguelike.Entities;

namespace MyRoguelike.Systems;

public struct CombatResult
{
    public bool Hit { get; set; }
    public bool Crit { get; set; }
    public int Damage { get; set; }
    public string Message { get; set; }
    public bool TargetKilled { get; set; }
}

public static class CombatSystem
{
    public static CombatResult MeleeAttack(Entity attacker, Entity defender)
    {
        var attackerCombat = attacker.GetComponent<CombatComponent>();
        var defenderCombat = defender.GetComponent<CombatComponent>();
        var defenderStats = defender.GetComponent<StatsComponent>();

        if (attackerCombat == null || defenderStats == null)
            return new CombatResult { Message = $"{attacker.Name} attacks {defender.Name} but fails!" };

        var hit = attackerCombat.RollHit(defenderCombat?.Evasion ?? 10);
        if (!hit)
            return new CombatResult { Hit = false, Message = $"{attacker.Name} misses {defender.Name}!" };

        var isCrit = attackerCombat.RollCrit();
        var damage = attackerCombat.CalculateDamage(
            attackerCombat.AttackPower,
            defenderCombat?.Defense ?? 0);

        if (isCrit)
            damage *= 2;

        var actualDamage = defenderStats.ApplyDamage(damage);

        EventSystem.RaiseEntityDamaged(defender, attacker, actualDamage);

        var killed = !defenderStats.IsAlive;
        if (killed)
            EventSystem.RaiseEntityKilled(defender, attacker);

        var message = isCrit
            ? $"{attacker.Name} critically hits {defender.Name} for {actualDamage} damage!"
            : $"{attacker.Name} hits {defender.Name} for {actualDamage} damage.";

        if (killed)
            message += $" {defender.Name} is defeated!";

        return new CombatResult
        {
            Hit = true,
            Crit = isCrit,
            Damage = actualDamage,
            Message = message,
            TargetKilled = killed
        };
    }
}
