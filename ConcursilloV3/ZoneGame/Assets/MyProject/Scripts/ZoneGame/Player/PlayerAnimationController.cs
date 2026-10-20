
using UnityEngine;

[RequireComponent(typeof(Player))]
[RequireComponent(typeof(Player1Controller))]
public class PlayerAnimationController : MonoBehaviour
{
    private Player player;
    private Player1Controller playerController;

    private void Awake()
    {
        player = GetComponent<Player>();
        playerController = GetComponent<Player1Controller>();
    }

    public void UpdateAnimator()
    {
        if (player == null || !player.IsOwner)
            return;

        if (playerController == null || playerController.anim == null)
            return;

        playerController.anim.SetInteger(
            "Direction",
            player.NetworkDirection.Value
        );

        playerController.anim.SetBool(
            "IsWalking",
            player.IsWalking.Value
        );
    }
}