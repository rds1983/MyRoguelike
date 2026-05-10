using MyRoguelike.Components;
using MyRoguelike.Data.Models;

namespace MyRoguelike.Entities;

public class StatsComponent : IComponent
{
    public int MaxHp { get; set; }
    public int CurrentHp { get; set; }
    public int MaxMp { get; set; }
    public int CurrentMp { get; set; }

    public StatBlock BaseStats { get; set; }
    public StatBlock BonusStats { get; set; }

    public int TotalStrength => BaseStats.Strength + BonusStats.Strength;
    public int TotalDexterity => BaseStats.Dexterity + BonusStats.Dexterity;
    public int TotalConstitution => BaseStats.Constitution + BonusStats.Constitution;
    public int TotalIntelligence => BaseStats.Intelligence + BonusStats.Intelligence;
    public int TotalWisdom => BaseStats.Wisdom + BonusStats.Wisdom;

    public bool IsAlive => CurrentHp > 0;

    public void SetHp(int max, int? current = null)
    {
        MaxHp = max;
        CurrentHp = current ?? max;
    }

    public void SetMp(int max, int? current = null)
    {
        MaxMp = max;
        CurrentMp = current ?? max;
    }

    public int ApplyDamage(int amount)
    {
        amount = Math.Max(0, amount);
        CurrentHp = Math.Max(0, CurrentHp - amount);
        return amount;
    }

    public int Heal(int amount)
    {
        var before = CurrentHp;
        CurrentHp = Math.Min(MaxHp, CurrentHp + amount);
        return CurrentHp - before;
    }

    public int RestoreMana(int amount)
    {
        var before = CurrentMp;
        CurrentMp = Math.Min(MaxMp, CurrentMp + amount);
        return CurrentMp - before;
    }

    public void AddBonusStats(StatBlock bonus)
    {
        BonusStats = new StatBlock
        {
            Strength = BonusStats.Strength + bonus.Strength,
            Dexterity = BonusStats.Dexterity + bonus.Dexterity,
            Constitution = BonusStats.Constitution + bonus.Constitution,
            Intelligence = BonusStats.Intelligence + bonus.Intelligence,
            Wisdom = BonusStats.Wisdom + bonus.Wisdom
        };
    }

    public void RemoveBonusStats(StatBlock bonus)
    {
        BonusStats = new StatBlock
        {
            Strength = Math.Max(0, BonusStats.Strength - bonus.Strength),
            Dexterity = Math.Max(0, BonusStats.Dexterity - bonus.Dexterity),
            Constitution = Math.Max(0, BonusStats.Constitution - bonus.Constitution),
            Intelligence = Math.Max(0, BonusStats.Intelligence - bonus.Intelligence),
            Wisdom = Math.Max(0, BonusStats.Wisdom - bonus.Wisdom)
        };
    }
}
