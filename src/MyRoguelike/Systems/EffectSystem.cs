using MyRoguelike.Data.Models;
using MyRoguelike.Entities;

namespace MyRoguelike.Systems;

public static class EffectSystem
{
    public static void Tick(Entity entity)
    {
        var effects = entity.GetComponent<EffectComponent>();
        var stats = entity.GetComponent<StatsComponent>();
        if (effects == null || stats == null) return;

        // Decrement and capture expirations
        var expired = new List<ActiveEffect>();
        foreach (var eff in effects.Effects)
        {
            eff.RemainingTurns--;
            if (eff.RemainingTurns <= 0)
                expired.Add(eff);
        }

        foreach (var eff in expired)
        {
            RemoveEffect(stats, eff);
            effects.RemoveEffect(eff.EffectId);
        }
    }

    public static bool ApplyPotion(Player player, PotionDef potion, out string message)
    {
        var stats = player.GetComponent<StatsComponent>();
        var effects = player.GetComponent<EffectComponent>();

        switch (potion.EffectType)
        {
            case "heal" when stats != null:
            {
                var healed = stats.Heal(potion.EffectValue);
                message = healed > 0 ? $"Recovered {healed} HP." : "You feel no different.";
                return true;
            }
            case "restore_mana" when stats != null:
            {
                var restored = stats.RestoreMana(potion.EffectValue);
                message = restored > 0 ? $"Recovered {restored} MP." : "Your mana is already full.";
                return true;
            }
            case "cure_poison":
            {
                // Poison effects aren't implemented yet; keep this as a harmless consume.
                message = "You feel the toxins leave your body.";
                return true;
            }
            case "buff_stat" when stats != null && effects != null:
            {
                var (stat, label) = potion.Id switch
                {
                    "strength_potion" => ("strength", "STR"),
                    "speed_potion" => ("dexterity", "DEX"),
                    _ => ("strength", "STR")
                };

                var id = $"potion_{potion.Id}_{stat}";
                ApplyStatBuff(stats, effects, id, stat, potion.EffectValue, potion.Duration > 0 ? potion.Duration : 5);
                message = $"{label} increased by {potion.EffectValue} for {Math.Max(1, potion.Duration)} turns.";
                return true;
            }
        }

        message = "Nothing happens.";
        return false;
    }

    private static void ApplyStatBuff(StatsComponent stats, EffectComponent effects, string effectId, string stat, int magnitude, int turns)
    {
        // Remove previous instance first so refresh keeps magnitude sane.
        if (effects.HasEffect(effectId))
        {
            var old = effects.Effects.First(e => e.EffectId == effectId);
            RemoveEffect(stats, old);
            effects.RemoveEffect(effectId);
        }

        var bonus = new StatBlock();
        switch (stat)
        {
            case "strength": bonus.Strength = magnitude; break;
            case "dexterity": bonus.Dexterity = magnitude; break;
            case "constitution": bonus.Constitution = magnitude; break;
            case "intelligence": bonus.Intelligence = magnitude; break;
            case "wisdom": bonus.Wisdom = magnitude; break;
        }

        stats.AddBonusStats(bonus);
        effects.ApplyEffect(new ActiveEffect
        {
            EffectId = effectId,
            SourceId = "potion",
            RemainingTurns = Math.Max(1, turns),
            EffectType = $"stat:{stat}",
            Magnitude = magnitude
        });
    }

    private static void RemoveEffect(StatsComponent stats, ActiveEffect effect)
    {
        if (!effect.EffectType.StartsWith("stat:", StringComparison.OrdinalIgnoreCase))
            return;

        var stat = effect.EffectType.Substring("stat:".Length);
        var bonus = new StatBlock();
        switch (stat)
        {
            case "strength": bonus.Strength = effect.Magnitude; break;
            case "dexterity": bonus.Dexterity = effect.Magnitude; break;
            case "constitution": bonus.Constitution = effect.Magnitude; break;
            case "intelligence": bonus.Intelligence = effect.Magnitude; break;
            case "wisdom": bonus.Wisdom = effect.Magnitude; break;
        }

        stats.RemoveBonusStats(bonus);
    }
}

