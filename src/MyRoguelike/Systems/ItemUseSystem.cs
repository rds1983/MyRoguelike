using MyRoguelike.Entities;
using MyRoguelike.World;
using Point = Microsoft.Xna.Framework.Point;

namespace MyRoguelike.Systems;

public static class ItemUseSystem
{
    public static bool TryUse(Player player, Item item, Map map, List<Enemy>? enemies, out string message)
    {
        if (item.Def == null)
        {
            message = "You can't use that.";
            return false;
        }

        if (item.Def.Category == "potion")
        {
            var potion = Game1.Data.GetPotion(item.Id);
            if (potion == null)
            {
                message = "This potion seems inert.";
                return false;
            }

            var ok = EffectSystem.ApplyPotion(player, potion, out message);
            return ok;
        }

        if (item.Def.Category == "scroll")
        {
            var scroll = Game1.Data.GetScroll(item.Id);
            if (scroll == null)
            {
                message = "The scroll crumbles without effect.";
                return false;
            }

            return TryUseScroll(player, scroll, map, enemies, out message);
        }

        message = "You can't use that.";
        return false;
    }

    private static bool TryUseScroll(Player player, Data.Models.ScrollDef scroll, Map map, List<Enemy>? enemies, out string message)
    {
        switch (scroll.EffectType)
        {
            case "teleport":
            {
                var rng = Random.Shared;
                for (var attempt = 0; attempt < 200; attempt++)
                {
                    var x = rng.Next(1, map.Width - 1);
                    var y = rng.Next(1, map.Height - 1);
                    if (!map.IsWalkable(x, y)) continue;
                    player.Position = new Point(x, y);
                    message = "Space folds around you. You reappear elsewhere!";
                    return true;
                }

                message = "The scroll fizzles.";
                return false;
            }
            case "identify":
            {
                var inv = player.GetComponent<InventoryComponent>();
                if (inv == null)
                {
                    message = "Nothing happens.";
                    return false;
                }

                foreach (var it in inv.Items)
                    it.IsIdentified = true;

                message = "Your pack's contents become clear to you.";
                return true;
            }
            case "aoe_damage":
            {
                if (enemies == null || enemies.Count == 0)
                {
                    message = "The air crackles briefly, then stills.";
                    return true;
                }

                var radius = 2;
                var hits = 0;
                foreach (var e in enemies.ToList())
                {
                    var stats = e.GetComponent<StatsComponent>();
                    if (stats?.IsAlive != true) continue;

                    var dist = Math.Abs(e.Position.X - player.Position.X) + Math.Abs(e.Position.Y - player.Position.Y);
                    if (dist > radius) continue;

                    stats.ApplyDamage(scroll.EffectValue);
                    hits++;
                    if (!stats.IsAlive)
                        EventSystem.RaiseEntityKilled(e, player);
                }

                message = hits > 0
                    ? $"A {scroll.Element ?? "mystic"} blast erupts! Hit {hits} foe(s)."
                    : "The blast scorches empty air.";
                return true;
            }
            default:
                message = "The magic is beyond your understanding... for now.";
                return true; // consume but no effect yet
        }
    }
}

