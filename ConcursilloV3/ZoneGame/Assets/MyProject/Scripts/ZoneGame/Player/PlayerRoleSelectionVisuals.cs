
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerRoleSelectionVisuals : MonoBehaviour
{
    public void UpdateSamePlayer(
        PlayerRole playerRole,
        SelectionRolType selectionRolType)
    {
        bool isGardener = playerRole == PlayerRole.Gardener;

        GameObject roleObject = isGardener
            ? RoleSelectorUI.Instance.gardenerRoleObject
            : RoleSelectorUI.Instance.builderRoleObject;

        switch (selectionRolType)
        {
            case SelectionRolType.Select:
                RoleSelectorUI.Instance.SwapBox(roleObject, true);
                break;

            case SelectionRolType.Deselect:
                RoleSelectorUI.Instance.SwapBox(roleObject, false);
                break;
        }
    }

    public void UpdateDifferentPlayer(
        PlayerRole playerRole,
        SelectionRolType selectionRolType,
        string playerName)
    {
        bool isGardener = playerRole == PlayerRole.Gardener;

        GameObject roleObject = isGardener
            ? RoleSelectorUI.Instance.gardenerRoleObject
            : RoleSelectorUI.Instance.builderRoleObject;

        switch (selectionRolType)
        {
            case SelectionRolType.Select:
                string roleName = isGardener
                    ? "Jardinero"
                    : "Constructor";

                RoleSelectorUI.Instance.SetRoleTextBox(
                    roleObject,
                    true,
                    $"{playerName} ha escogido el rol de {roleName}"
                );
                break;

            case SelectionRolType.Deselect:
                RoleSelectorUI.Instance.SetRoleTextBox(
                    roleObject,
                    false
                );
                break;
        }
    }
}
