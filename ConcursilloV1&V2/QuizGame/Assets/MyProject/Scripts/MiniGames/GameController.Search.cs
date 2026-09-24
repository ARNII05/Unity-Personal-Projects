using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class GameController
{
    public void OnSearchValueChanged()
    {
        string search = Normalize(userSearch.text);

        if (gameModeType != GameModeType.MapImg)
        {
            bool isSearchEmpty = string.IsNullOrWhiteSpace(search);
        }

        if (ShouldDisableRoleFilter(search))
        {
            ResetFilters();
            actualRoleIndex = -1;
        }

        foreach (Transform child in GetPanel().transform)
        {
            if (child.name.Contains("Template", StringComparison.OrdinalIgnoreCase)) continue;

            if (!string.IsNullOrWhiteSpace(search))
            {
                bool matchesSearch = Normalize(child.name).Contains(search, StringComparison.OrdinalIgnoreCase);
                child.gameObject.SetActive(matchesSearch);
                continue;
            }

            if (actualRoleIndex != -1)
            {
                child.gameObject.SetActive(MatchesActiveFilter(child.name));
                continue;
            }

            child.gameObject.SetActive(true);
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(miScroll.content);
        miScroll.verticalNormalizedPosition = 1f;
    }

    private bool MatchesActiveFilter(string itemName)
    {
        if (gameModeType == GameModeType.MapImg)
        {
            Maps mapEnum = (Maps)ParseAnswer(GetAnswerEnumType(gameModeType), itemName);
            return GetMapType(mapEnum) == GetMapTypeByIndex(actualRoleIndex);
        }

        Chars heroEnum = (Chars)ParseAnswer(GetAnswerEnumType(gameModeType), itemName);
        return GetHeroRole(heroEnum) == GetHeroRoleByIndex(actualRoleIndex);
    }

    private bool ShouldDisableRoleFilter(string searchText)
    {
        if (actualRoleIndex == -1 || string.IsNullOrWhiteSpace(searchText))
            return false;

        int currentRoleMatches = 0;
        int otherRoleMatches = 0;
        HeroRole activeRole = GetHeroRoleByIndex(actualRoleIndex);
        MapType activeMapType = GetMapTypeByIndex(actualRoleIndex);

        foreach (Transform child in GetPanel().transform)
        {
            if (child.name.Contains("Template", StringComparison.OrdinalIgnoreCase))
                continue;

            if (Normalize(child.name).Contains(searchText, StringComparison.OrdinalIgnoreCase))
            {
                if (gameModeType == GameModeType.MapImg)
                {
                    Maps mapEnum = (Maps)ParseAnswer(GetAnswerEnumType(gameModeType), child.name);

                    if (GetMapType(mapEnum) == activeMapType)
                        currentRoleMatches++;
                    else
                        otherRoleMatches++;
                }
                else
                {
                    Chars heroEnum = (Chars)ParseAnswer(GetAnswerEnumType(gameModeType), child.name);

                    if (GetHeroRole(heroEnum) == activeRole)
                        currentRoleMatches++;
                    else
                        otherRoleMatches++;
                }
            }
        }

        return currentRoleMatches == 0 && otherRoleMatches > 0;
    }

    string GetBasePlaceHolderMsg()
    {
        return gameModeType == GameModeType.MapImg ? "Elige un mapa..." : "Elige un héroe...";
    }

    System.Collections.IEnumerator ChangePlaceHolderMsg(string msg, string baseMsg)
    {
        userSearch.placeholder.GetComponent<TextMeshProUGUI>().text = msg;

        yield return new WaitForSeconds(2.5f);

        userSearch.placeholder.GetComponent<TextMeshProUGUI>().text = baseMsg;
    }
}
