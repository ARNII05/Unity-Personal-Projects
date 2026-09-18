using System;

public partial class GameController
{
    string NormalizeKeepingCommas(string s)
    {
        return s
            .Trim()
            .Replace(" ", "")
            .Replace(":", "")
            .Replace(".", "");
    }

    string Normalize(string s)
    {
        return s
            .Trim()
            .Replace(" ", "")
            .Replace(":", "")
            .Replace(".", "")
            .Replace(",", "");
    }

    Type GetAnswerEnumType(GameModeType gmType)
    {
        return gmType == GameModeType.MapImg ? typeof(Maps) : typeof(Chars);
    }

    bool TryParseAnswer(Type enumType, string value, out Enum answer)
    {
        string normalizedValue = Normalize(value);

        try
        {
            answer = (Enum)Enum.Parse(enumType, normalizedValue, true);
            return true;
        }
        catch (ArgumentException)
        {
            UnityEngine.Debug.LogWarning($"Could not parse answer '{value}' as {enumType.Name}.");
            answer = null;
            return false;
        }
    }

    Enum ParseAnswer(Type enumType, string value)
    {
        if (TryParseAnswer(enumType, value, out var answer))
            return answer;

        throw new ArgumentException($"Invalid answer '{value}' for enum {enumType.Name}.");
    }

    GameModeType GetGameModeType(GameMode gameMode)
    {
        return gameMode.basePath switch
        {
            "CharImg" => GameModeType.CharImg,
            "MapImg" => GameModeType.MapImg,
            "CharDesc" => GameModeType.CharDesc,
            "CharSound" => GameModeType.CharSound,
            "TfSound" => GameModeType.TfSound,
            "ShuffledHeroes" => GameModeType.ShuffledHeroes,
            _ => throw new ArgumentException($"Invalid game mode basePath: {gameMode.basePath}")
        };
    }
}
