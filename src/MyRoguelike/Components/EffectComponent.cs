using MyRoguelike.Components;

namespace MyRoguelike.Entities;

public class ActiveEffect
{
    public string EffectId { get; set; } = string.Empty;
    public string SourceId { get; set; } = string.Empty;
    public int RemainingTurns { get; set; }
    public string EffectType { get; set; } = string.Empty;
    public int Magnitude { get; set; }
}

public class EffectComponent : IComponent
{
    private readonly List<ActiveEffect> _effects = [];

    public IReadOnlyList<ActiveEffect> Effects => _effects.AsReadOnly();

    public void ApplyEffect(ActiveEffect effect)
    {
        var existing = _effects.FirstOrDefault(e => e.EffectId == effect.EffectId);
        if (existing != null)
        {
            existing.RemainingTurns = Math.Max(existing.RemainingTurns, effect.RemainingTurns);
            return;
        }
        _effects.Add(effect);
    }

    public void TickEffects()
    {
        foreach (var effect in _effects)
            effect.RemainingTurns--;

        _effects.RemoveAll(e => e.RemainingTurns <= 0);
    }

    public bool HasEffect(string effectId)
    {
        return _effects.Any(e => e.EffectId == effectId);
    }

    public void RemoveEffect(string effectId)
    {
        _effects.RemoveAll(e => e.EffectId == effectId);
    }

    public void Clear()
    {
        _effects.Clear();
    }
}
