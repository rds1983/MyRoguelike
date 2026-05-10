using MyRoguelike.Components;

namespace MyRoguelike.Entities;

public class AiComponent : IComponent
{
    public string BehaviorType { get; set; } = "aggressive";
    public AiState CurrentState { get; set; } = AiState.Idle;
    public int DetectionRange { get; set; } = 8;
    public string? TargetEntityId { get; set; }
}

public enum AiState
{
    Idle,
    Alert,
    Attacking,
    Fleeing,
    Patrolling
}
