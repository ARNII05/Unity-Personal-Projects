using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class Player1Controller : NetworkBehaviour
{
    public Animator anim;

    [Header("Configuración de Velocidad")]
    private const float moveSpeed = 35;

    private Rigidbody2D rb;
    private Vector2 movementInput;

    private Player player;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GetComponent<Player>();

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        if (InventoryUI.Instance.IsOpen)
            return;

        movementInput.x = Input.GetAxisRaw("Horizontal");
        movementInput.y = Input.GetAxisRaw("Vertical");

        movementInput = movementInput.normalized;

        player.IsWalking.Value = movementInput != Vector2.zero;

        if (movementInput != Vector2.zero)
            UpdateDirection();
    }

    private void FixedUpdate()
    {
        if (!IsOwner)
            return;

        rb.velocity = movementInput * moveSpeed;
    }

    private void UpdateDirection()
    {
        if (Mathf.Abs(movementInput.x) > Mathf.Abs(movementInput.y))
        {
            player.NetworkDirection.Value =
                movementInput.x > 0 ? 2 : 3;

            return;
        }

        player.NetworkDirection.Value =
            movementInput.y > 0 ? 1 : 0;
    }
}