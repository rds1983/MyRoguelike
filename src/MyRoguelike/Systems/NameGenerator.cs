namespace MyRoguelike.Systems;

public static class NameGenerator
{
    private static readonly string[] Consonants =
        ["b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "n", "p", "r", "s", "t", "v", "w", "z"];

    private static readonly string[] Vowels =
        ["a", "e", "i", "o", "u", "ae", "ai", "ea", "ee", "ou"];

    private static readonly string[] Prefixes =
        ["", "Al", "Ar", "Bel", "Bron", "Cor", "Dal", "Dor", "El", "Em",
         "Far", "Gar", "Gor", "Hel", "Iron", "Kar", "Lor", "Mar", "Mor",
         "Nel", "Nor", "Or", "Por", "Ran", "Sil", "Sun", "Tar", "Tor",
         "Ul", "Ur", "Val", "Vor", "Zar"];

    private static readonly string[] Suffixes =
        ["", "a", "ak", "an", "ar", "as", "ath", "dor", "el", "en",
         "er", "ia", "ian", "is", "ius", "mar", "on", "or", "os",
         "thor", "us", "vane"];

    private static readonly string[] CitySuffixes =
        ["burg", "dale", "fall", "ford", "gate", "haven", "hold", "keep",
         "march", "more", "reach", "ridge", "shire", "stead", "town", "vale"];

    private static readonly string[] DungeonPrefixes =
        ["Caverns of ", "Depths of ", "Dungeons of ", "Halls of ", "Lair of the ",
         "Mines of ", "Pits of ", "Ruins of ", "Sanctum of ", "Tomb of ",
         "Tower of ", "Vault of "];

    private static readonly string[] DungeonSuffixes =
        ["Despair", "Doom", "Eternity", "Fate", "Gloom", "Horror", "Night",
         "Shadow", "Sorrow", "Terror", "the Ancient", "the Damned", "the Fallen",
         "the Forgotten", "the Lost"];

    public static string GenerateName(Random rng)
    {
        var usePrefix = rng.Next(3) > 0;
        var syllables = rng.Next(2, 4);

        var name = usePrefix ? Prefixes[rng.Next(Prefixes.Length)] : "";

        for (var i = 0; i < syllables; i++)
        {
            name += Consonants[rng.Next(Consonants.Length)];
            name += Vowels[rng.Next(Vowels.Length)];
            if (rng.Next(2) == 0)
                name += Consonants[rng.Next(Consonants.Length)];
        }

        name += Suffixes[rng.Next(Suffixes.Length)];

        if (name.Length < 3)
            name = Prefixes[rng.Next(Prefixes.Length)] + "on";

        return char.ToUpper(name[0]) + name[1..];
    }

    public static string GenerateCityName(Random rng)
    {
        if (rng.Next(2) == 0)
            return GenerateName(rng) + CitySuffixes[rng.Next(CitySuffixes.Length)];

        var prefixes = new[] { "New ", "Old ", "North ", "South ", "East ", "West ", "Port ", "Fort " };
        return prefixes[rng.Next(prefixes.Length)] + GenerateName(rng);
    }

    public static string GenerateDungeonName(Random rng)
    {
        var prefix = DungeonPrefixes[rng.Next(DungeonPrefixes.Length)];
        var suffix = DungeonSuffixes[rng.Next(DungeonSuffixes.Length)];
        return prefix + suffix;
    }
}
