namespace MyRoguelike.Data.Models;

public class SpecialAttackDef
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "single_target";
    public string Element { get; set; } = "physical";
    public double DamageMultiplier { get; set; } = 1.0;
    public int Range { get; set; } = 1;
    public int Cooldown { get; set; }
    public string? StatusEffect { get; set; }
    public int StatusDuration { get; set; }
    public string Description { get; set; } = string.Empty;
}
