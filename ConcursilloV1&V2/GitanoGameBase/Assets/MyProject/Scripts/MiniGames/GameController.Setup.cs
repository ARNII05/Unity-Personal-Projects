using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public partial class GameController
{
    void ShuffleModes(List<GameMode> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);

            (list[j], list[i]) = (list[i], list[j]);
        }
    }

    void ChooseFirstGameMode(List<GameMode> list, GameModeType gm)
    {
        GameModeType gameTypeList = GetGameModeType(list[0]);

        if (gameTypeList == gm) return;

        for (int i = 1; i < list.Count; i++)
        {
            gameTypeList = GetGameModeType(list[i]);

            if (gameTypeList == gm)
            {
                (list[0], list[i]) = (list[i], list[0]);
                return;
            }
        }
    }

    void FillGameModeNames()
    {
        gameModeNames = new string[gameModeList.gameModes.Count];

        for (int i = 0; i < gameModeList.gameModes.Count; i++)
        {
            gameModeNames[i] = gameModeList.gameModes[i].name;
        }
    }
}
