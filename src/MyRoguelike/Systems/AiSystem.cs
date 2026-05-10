using Microsoft.Xna.Framework;
using MyRoguelike.Entities;
using MyRoguelike.World;
using Point = Microsoft.Xna.Framework.Point;

namespace MyRoguelike.Systems;

public enum AiActionType
{
    Idle,
    Move,
    MeleeAttack
}

public struct AiAction
{
    public AiActionType Type { get; set; }
    public Point TargetPosition { get; set; }
    public Entity? Target { get; set; }
}

public static class AiSystem
{
    public static AiAction GetAction(Enemy enemy, Entity player, Map map,
        Func<Point, bool>? isBlocked = null)
    {
        var ai = enemy.GetComponent<AiComponent>();
        if (ai == null)
            return new AiAction { Type = AiActionType.Idle };

        var distance = Math.Abs(enemy.Position.X - player.Position.X) +
                       Math.Abs(enemy.Position.Y - player.Position.Y);

        if (distance <= 1)
            return new AiAction { Type = AiActionType.MeleeAttack, Target = player };

        if (ai.CurrentState == AiState.Fleeing)
            return new AiAction { Type = AiActionType.Idle };

        if (distance <= ai.DetectionRange)
        {
            ai.CurrentState = AiState.Alert;

            var path = PathfindingSystem.FindPath(map, enemy.Position, player.Position, isBlocked);
            if (path.Count >= 2)
                return new AiAction { Type = AiActionType.Move, TargetPosition = path[1] };
        }
        else
        {
            ai.CurrentState = AiState.Idle;
        }

        return new AiAction { Type = AiActionType.Idle };
    }
}
