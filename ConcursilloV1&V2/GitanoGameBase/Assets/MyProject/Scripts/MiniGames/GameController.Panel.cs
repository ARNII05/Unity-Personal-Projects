using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;

public partial class GameController
{
    public void FilterByRole(int roleIndex)
    {
        ResetFilters();

        if (actualRoleIndex == roleIndex)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetPanel().GetComponent<RectTransform>());
            actualRoleIndex = -1;
            return;
        }

        actualRoleIndex = roleIndex;

        GameObject activeFilters = GetActiveFilterPanel();
        activeFilters.transform.GetChild(roleIndex).GetComponent<Image>().color = selectedColor;

        if (gameModeType == GameModeType.MapImg)
        {
            FilterMapsByType(roleIndex);
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetPanel().GetComponent<RectTransform>());
            return;
        }

        HeroRole role = GetHeroRoleByIndex(roleIndex);

        foreach (Transform child in GetPanel().transform)
        {
            if (child.name == "HeroTemplate")
                continue;

            Chars charName = (Chars)ParseAnswer(GetAnswerEnumType(gameModeType), child.name);

            if (GetHeroRole(charName) != role)
                child.gameObject.SetActive(false);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(GetPanel().GetComponent<RectTransform>());
    }

    GameObject GetActiveFilterPanel()
    {
        return gameModeType == GameModeType.MapImg ? mapRoleFilter : rolFilters;
    }

    void FilterMapsByType(int mapTypeIndex)
    {
        MapType mapType = GetMapTypeByIndex(mapTypeIndex);

        foreach (Transform child in GetPanel().transform)
        {
            if (child.name.Contains("Template", StringComparison.OrdinalIgnoreCase))
                continue;

            Maps mapName = (Maps)ParseAnswer(GetAnswerEnumType(gameModeType), child.name);

            if (GetMapType(mapName) != mapType)
                child.gameObject.SetActive(false);
        }
    }

    HeroRole GetHeroRoleByIndex(int index)
    {
        return index switch
        {
            0 => HeroRole.Tank,
            1 => HeroRole.Dps,
            2 => HeroRole.Support,
            _ => HeroRole.None
        };
    }

    MapType GetMapTypeByIndex(int index)
    {
        return index switch
        {
            0 => MapType.FlashPoint,
            1 => MapType.Control,
            2 => MapType.Escort,
            3 => MapType.Hybrid,
            4 => MapType.Push,
            5 => MapType.Clash,
            6 => MapType.Assault,
            _ => MapType.None
        };
    }

    void ResetFilters()
    {
        if (actualRoleIndex != -1)
            GetActiveFilterPanel().transform.GetChild(actualRoleIndex).GetComponent<Image>().color = filtersBackGroundBaseColor;

        foreach (Transform child in GetPanel().transform)
        {
            if (child.name.Contains("Template", StringComparison.OrdinalIgnoreCase))
                continue;

            child.gameObject.SetActive(true);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(GetPanel().GetComponent<RectTransform>());
    }

    void LoadPanel(GameModeType gmType, Transform selectionPanelTransform, GameObject template)
    {
        string path = ChoosePanelType(gmType);

        if (path == "") return;

        Sprite[] allImgs = Resources.LoadAll<Sprite>(path);

        foreach (Transform child in selectionPanelTransform)
        {
            if (child.gameObject != template) Destroy(child.gameObject);
        }

        foreach (Sprite s in OrderPanelSprites(gmType, allImgs))
        {
            GameObject newHero = Instantiate(template, selectionPanelTransform);
            newHero.SetActive(true);

            Transform childTransform = newHero.transform.GetChild(0);

            if (childTransform.TryGetComponent<Image>(out var heroImage))
            {
                heroImage.preserveAspect = true;
                heroImage.sprite = s;
                heroImage.color = Color.white;
            }

            string heroName = s.name;

            newHero.name = heroName;

            if (newHero.TryGetComponent<Image>(out var backgroundImage))
                backgroundImage.color = backgroundBaseColor;

            SetRoleIcon(gmType, newHero.transform, heroName);
            SetMapTypeIcon(gmType, newHero.transform, heroName);

            if (!TryParseAnswer(GetAnswerEnumType(gameModeType), heroName, out var answer))
                continue;

            newHero.GetComponent<Button>().onClick.AddListener(() =>
            {
                RegisterUserAnswer(answer, childTransform);
            });

            RightClickHandler handler = newHero.AddComponent<RightClickHandler>();

            handler.onRightClick += () =>
            {
                RegisterDescartedAnswers(answer, childTransform);
            };
        }

        template.SetActive(false);
    }

    string ChoosePanelType(GameModeType gmType)
    {
        if (activePanel == gmType) return "";

        activePanel = gmType;

        return activePanel != GameModeType.MapImg ? "ConcursilloV2/HeroeIcons" : "ConcursilloV2/MapIcons";
    }

    IEnumerable<Sprite> OrderPanelSprites(GameModeType gmType, IEnumerable<Sprite> sprites)
    {
        if (gmType == GameModeType.MapImg)
        {
            return sprites
                .OrderBy(s => GetMapTypeOrder(s.name))
                .ThenBy(s => s.name);
        }

        return sprites
            .OrderBy(s => GetHeroRoleOrder(s.name))
            .ThenBy(s => s.name);
    }

    int GetMapTypeOrder(string mapName)
    {
        if (!TryParseAnswer(typeof(Maps), mapName, out var answer))
            return 99;

        return GetMapType((Maps)answer) switch
        {
            MapType.FlashPoint => 0,
            MapType.Control => 1,
            MapType.Escort => 2,
            MapType.Hybrid => 3,
            MapType.Push => 4,
            MapType.Clash => 5,
            MapType.Assault => 6,
            _ => 99
        };
    }

    int GetHeroRoleOrder(string heroName)
    {
        if (!TryParseAnswer(typeof(Chars), heroName, out var answer))
            return 99;

        return GetHeroRole((Chars)answer) switch
        {
            HeroRole.Tank => 0,
            HeroRole.Dps => 1,
            HeroRole.Support => 2,
            _ => 99
        };
    }

    HeroRole GetHeroRole(Chars hero)
    {
        return hero switch
        {
            Chars.Doomfist or Chars.Dva or Chars.Hazard or Chars.JunkerQueen
                or Chars.Mauga or Chars.Orisa or Chars.Ramattra or Chars.Reinhardt
                or Chars.Roadhog or Chars.Sigma or Chars.Winston or Chars.WreckingBall
                or Chars.Zarya or Chars.Domina => HeroRole.Tank,

            Chars.Ana or Chars.Baptiste or Chars.Brigitte or Chars.Illari
                or Chars.Juno or Chars.Kiriko or Chars.LifeWeaver or Chars.Lucio
                or Chars.Mercy or Chars.Moira or Chars.Wuyang or Chars.Zenyatta or Chars.JetpackCat
                or Chars.Mizuki => HeroRole.Support,

            _ => HeroRole.Dps
        };
    }

    MapType GetMapType(Maps map)
    {
        return map switch
        {
            Maps.Hanamura or Maps.IndustriasVolskaya or Maps.ColoniaLunarHorizon
                => MapType.Assault,

            Maps.Busan or Maps.Ilios or Maps.Nepal or Maps.Oasis
                or Maps.PeninsulaAntartica or Maps.Samoa or Maps.TorreLijiang
                => MapType.Control,

            Maps.CircuitRoyal or Maps.Dorado or Maps.JunkerTown or Maps.LaHabana
                or Maps.MonasterioShambali or Maps.ObservatorioGibraltar
                or Maps.Rialto or Maps.Ruta66
                => MapType.Escort,

            Maps.BlizzardWorld or Maps.Eichenwalde or Maps.Hollywood
                or Maps.KingsRow or Maps.Midtown or Maps.Numbani or Maps.Paraiso
                => MapType.Hybrid,

            Maps.Colosseo or Maps.Esperanca or Maps.NewQueenStreet or Maps.Runasapi
                => MapType.Push,

            Maps.Hanaoka or Maps.TronoDeAnubis
                => MapType.Clash,

            Maps.Aatlis or Maps.NewJunkCity or Maps.Suravasa
                => MapType.FlashPoint,

            _ => MapType.None
        };
    }

    void SetRoleIcon(GameModeType gmType, Transform heroTransform, string heroName)
    {
        if (gmType == GameModeType.MapImg)
            return;

        if (heroTransform.childCount <= 1)
            return;

        if (!TryParseAnswer(typeof(Chars), heroName, out var answer))
            return;

        Transform roleIconTransform = heroTransform.GetChild(1);

        if (!roleIconTransform.TryGetComponent<Image>(out var roleIconImage))
            return;

        roleIconImage.sprite = Resources.Load<Sprite>($"ConcursilloV2/RolIcons/{GetRoleIconName(GetHeroRole((Chars)answer))}");
        roleIconImage.preserveAspect = true;
        roleIconImage.color = Color.white;
        roleIconTransform.gameObject.SetActive(roleIconImage.sprite != null);
    }

    void SetMapTypeIcon(GameModeType gmType, Transform mapTransform, string mapName)
    {
        if (gmType != GameModeType.MapImg)
            return;

        if (mapTransform.childCount <= 1)
            return;

        if (!TryParseAnswer(typeof(Maps), mapName, out var answer))
            return;

        Transform mapTypeIconTransform = mapTransform.GetChild(1);

        if (!mapTypeIconTransform.TryGetComponent<Image>(out var mapTypeIconImage))
            return;

        mapTypeIconImage.sprite = Resources.Load<Sprite>($"ConcursilloV2/MapTypeIcons/{GetMapTypeIconName(GetMapType((Maps)answer))}");
        mapTypeIconImage.preserveAspect = true;
        mapTypeIconImage.color = Color.white;
        mapTypeIconTransform.gameObject.SetActive(mapTypeIconImage.sprite != null);
    }

    string GetRoleIconName(HeroRole role)
    {
        return role switch
        {
            HeroRole.Tank => "tankIcon",
            HeroRole.Support => "supportIcon",
            _ => "dpsIcon"
        };
    }

    string GetMapTypeIconName(MapType mapType)
    {
        return mapType switch
        {
            MapType.Assault => "assaultIcon",
            MapType.Control => "controlcon",
            MapType.Escort => "escortIcon",
            MapType.Hybrid => "hybridIcon",
            MapType.Push => "pushIcon",
            MapType.Clash => "clashIcon",
            MapType.FlashPoint => "flashpointIcon",
            _ => ""
        };
    }

    void LoadCorrectPanel()
    {
        if (gameModeType == GameModeType.MapImg)
        {
            LoadPanel(gameModeType, mapSelectionPanel.transform, mapSelectionTemplate);
            mapSrollView.SetActive(true);
            return;
        }

        LoadPanel(gameModeType, heroSelectionPanel.transform, heroTemplate);
        heroSelectionPanel.SetActive(true);
    }

    void ResetChildren()
    {
        foreach (Transform child in GetPanel().transform)
        {
            if (child.name.Contains("Template", StringComparison.OrdinalIgnoreCase))
                continue;

            child.GetComponent<Button>().interactable = true;

            if (child.TryGetComponent<CanvasGroup>(out var cg))
            {
                cg.alpha = 1f;
                child.localScale = Vector3.one;
            }
        }
    }

    void ClearColors(GameObject panel)
    {
        foreach (Transform child in panel.transform)
        {
            if (child.name.Contains("Template", StringComparison.OrdinalIgnoreCase))
                continue;

            if (child.childCount > 0 && child.GetChild(0).TryGetComponent<Image>(out var iconImage))
                iconImage.color = baseColor;

            if (child.TryGetComponent<Image>(out var backgroundImage))
                backgroundImage.color = backgroundBaseColor;
        }
    }

    GameObject GetPanel()
    {
        return gameModeType == GameModeType.MapImg ? mapSelectionPanel : heroSelectionPanel;
    }
}
