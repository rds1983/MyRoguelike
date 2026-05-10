using MyRoguelike.Data.Models;

namespace MyRoguelike.Entities;

public class Player : Entity
{
    public string ClassId { get; set; } = string.Empty;
    public int Level { get; set; } = 1;
    public int Experience { get; set; }
    public int ExperienceToNext { get; set; } = 100;
    public int StatPointsAvailable { get; set; }
    public int Gold { get; set; }

    public ClassDef? GetClassDef()
    {
        return Game1.Data.GetClass(ClassId);
    }

    public void AddXp(int amount)
    {
        Experience += amount;
        while (Experience >= ExperienceToNext && Level < Constants.MaxLevel)
            LevelUp();
    }

    private void LevelUp()
    {
        Experience -= ExperienceToNext;
        Level++;
        ExperienceToNext = CalculateXpForLevel(Level);
        StatPointsAvailable += 3;
    }

    public static int CalculateXpForLevel(int level)
    {
        return level * 100;
    }
}
