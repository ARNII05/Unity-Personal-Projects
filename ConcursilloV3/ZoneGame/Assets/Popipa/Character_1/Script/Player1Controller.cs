using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class Player1Controller : NetworkBehaviour
{
    public Animator anim;
    public bool facingRight = true;

    [Header("Configuración de Velocidad")]
    private float moveSpeed = 35;

    private Rigidbody2D rb;
    private Vector2 movementInput;

    private Player player;

    // NUEVO: detector de saltos (DESACTIVADO)
    // private Vector3 lastPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GetComponent<Player>();

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void Update()
    {
        if (!IsOwner)
            return;

        movementInput.x = Input.GetAxisRaw("Horizontal");
        movementInput.y = Input.GetAxisRaw("Vertical");

        movementInput = movementInput.normalized;

        if (movementInput != Vector2.zero)
        {
            Walk();
            player.IsWalking.Value = true;

            if (movementInput.x > 0 && !facingRight)
            {
                Flip();
            }
            else if (movementInput.x < 0 && facingRight)
            {
                Flip();
            }
        }
        else
        {
            WalkOff();
            player.IsWalking.Value = false;
        }
    }

    void FixedUpdate()
    {
        if (!IsOwner)
            return;

        rb.velocity = movementInput * moveSpeed;
    }

    private void Flip()
    {
        facingRight = !facingRight;

        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;

        player.FacingRight.Value = facingRight;
    }

    public void Walk()
    {
        if (anim != null)
        {
            anim.SetBool("Walk", true);
        }
    }

    public void WalkOff()
    {
        if (anim != null)
        {
            anim.SetBool("Walk", false);
        }
    }
}