
using Unity.Netcode;
using UnityEngine;
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]

[RequireComponent(typeof(Player))]
public class Player1Controller : NetworkBehaviour
{
    public Animator anim;

    [Header("Configuración de Velocidad")]
    private const float moveSpeed = 45;

    private Rigidbody2D rb;
    private Vector2 movementInput;
    private Player player;

    private ZoneGameControls controls;

    public float movementSpeedMultiplier = 1f;

    [SerializeField] private float footstepInterval = 0.5f;
    [SerializeField, Range(0f, 1f)] private float footstepVolume = 0.05f;

    private float footstepTimer;

    private void Awake()
    {
        controls = new ZoneGameControls();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
            controls.Gameplay.Enable();
    }

    public override void OnNetworkDespawn()
    {
        controls.Gameplay.Disable();
    }

    public override void OnDestroy()
    {
        controls?.Dispose();
        base.OnDestroy();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GetComponent<Player>();

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        UpdateMovementSpeed();

        player.inventory.OnInventoryChanged += UpdateMovementSpeed;
    }

    private void UpdateMovementSpeed()
    {
        bool hasAgilityPotion = player.inventory.TryToGet(
            ItemType.AgilityPotion,
            ItemVault.IsBuff(ItemType.AgilityPotion)
        ) != ItemType.None;

        int bonus = hasAgilityPotion
            ? ItemVault.GetBuffValue(ItemType.AgilityPotion, BuffApplierType.Movement)
            : 0;

        movementSpeedMultiplier = 1f + bonus / 100f;
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        if (player.LocalState != PlayerState.Normal)
        {
            movementInput = Vector2.zero;
            player.IsWalking.Value = false;
            return;
        }

        movementInput = controls.Gameplay.Move.ReadValue<Vector2>();
        movementInput = movementInput.normalized;

        player.IsWalking.Value = movementInput != Vector2.zero;

        if (movementInput != Vector2.zero)
            UpdateDirection();

        //UpdateFootsteps();
    }

    private void UpdateFootsteps()
    {
        if (!IsOwner)
            return;

        if (movementInput == Vector2.zero)
        {
            footstepTimer = 0f;
            return;
        }

        footstepTimer += Time.deltaTime;

        if (footstepTimer >= footstepInterval)
        {
            footstepTimer = 0f;

            AudioManager.Instance.PlaySFXSound(
                GameSounds.GetSFXSound(SFXType.Footstep),
                footstepVolume
            );
        }
    }

    private void FixedUpdate()
    {
        if (!IsOwner)
            return;

        if (player.LocalState != PlayerState.Normal)
        {
            movementInput = Vector2.zero;
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = movementInput * (moveSpeed * movementSpeedMultiplier);
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
