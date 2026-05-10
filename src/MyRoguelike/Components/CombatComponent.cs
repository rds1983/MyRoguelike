using MyRoguelike.Components;

namespace MyRoguelike.Entities;

public class CombatComponent : IComponent
{
    private const int BaseAccuracy = 80;
    private const int BaseEvasion = 10;
    private const double BaseCritChance = 0.05;

    public int AttackPower { get; set; }
    public int MagicAttack { get; set; }
    public int Defense { get; set; }

    public int Accuracy => BaseAccuracy;
    public int Evasion => BaseEvasion;
    public double CritChance => BaseCritChance;

    public void Recalculate(StatsComponent stats, EquipmentComponent equipment)
    {
        var eqBonuses = equipment.GetAllBonuses();

        AttackPower = stats.TotalStrength * 2 + eqBonuses.GetValueOrDefault("attack", 0);
        MagicAttack = stats.TotalIntelligence * 2 + eqBonuses.GetValueOrDefault("magic_attack", 0);
        Defense = stats.TotalConstitution + eqBonuses.GetValueOrDefault("defense", 0);
    }

    public bool RollHit(int targetEvasion)
    {
        var hitChance = (double)Accuracy / (Accuracy + targetEvasion);
        return Random.Shared.NextDouble() < hitChance;
    }

    public bool RollCrit()
    {
        return Random.Shared.NextDouble() < CritChance;
    }

    public int CalculateDamage(int attackerAtk, int defenderDef)
    {
        return Math.Max(1, attackerAtk - defenderDef);
    }
}
