using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public Vector2Int initialPos;
    public GameObject currentZone;
    private BorderZone currentBorderZone;

    [SerializeField] private GameObject otherPlayerVisualPrefab;

    private GameObject otherPlayerVisualRoot;
    private GameObject otherPlayerVisual;

    private Animator otherPlayerAnimator;

    public NetworkVariable<bool> IsWalking = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    public NetworkVariable<bool> FacingRight = new(
        true,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    public NetworkVariable<Vector2Int> NetworkMapPos = new(
        Vector2Int.zero,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private void Update()
    {
        if (currentBorderZone != null && Input.GetKeyDown(KeyCode.F))
        {
            Direction direction = currentBorderZone.direction;
            currentBorderZone = null;
            MapManager.Instance.OnSwapingZone(this, direction);
        }

        if (IsOwner)
        {
            UpdateOtherPlayerVisual();
        }
    }

    [ServerRpc]
    public void SetNetworkMapPosServerRpc(Vector2Int newPos)
    {
        NetworkMapPos.Value = newPos;
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log(
            $"{name} spawned | OwnerClientId: {OwnerClientId} | IsOwner: {IsOwner}"
        );

        base.OnNetworkSpawn();

        NetworkMapPos.OnValueChanged += OnNetworkMapPosChanged;

        if (IsOwner)
        {
            otherPlayerVisualRoot = new GameObject("Other Player Visual Root");

            otherPlayerVisualRoot.transform.SetPositionAndRotation(
                transform.position,
                transform.rotation);
            
            otherPlayerVisualRoot.transform.localScale =
                Vector3.one;

            otherPlayerVisual = Instantiate(
                otherPlayerVisualPrefab,
                otherPlayerVisualRoot.transform
            );

            otherPlayerAnimator =
                otherPlayerVisual.GetComponent<Animator>();

            otherPlayerVisualRoot.SetActive(false);
        }

        MapManager.Instance.RegisterPlayer(this);
    }

    private void OnNetworkMapPosChanged(
        Vector2Int oldPos,
        Vector2Int newPos)
    {
        MapManager.Instance.UpdateActualZone(this);
    }

    private void UpdateOtherPlayerVisual()
    {
        if (!IsOwner)
            return;

        if (MapManager.Instance.Player1 == null ||
            MapManager.Instance.Player2 == null)
            return;

        Player otherPlayer;

        if (this == MapManager.Instance.Player1)
            otherPlayer = MapManager.Instance.Player2;
        else
            otherPlayer = MapManager.Instance.Player1;

        bool sameZone =
            NetworkMapPos.Value ==
            otherPlayer.NetworkMapPos.Value;

        otherPlayerVisualRoot.SetActive(sameZone);

        if (!sameZone)
            return;

        otherPlayerVisualRoot.transform.position =
            currentZone.transform.position +
            (otherPlayer.transform.position -
             otherPlayer.currentZone.transform.position);

        otherPlayerAnimator.SetBool(
            "Walk",
            otherPlayer.IsWalking.Value
        );

        otherPlayerAnimator.SetBool(
            "Idle",
            !otherPlayer.IsWalking.Value
        );

        Vector3 scale =
            otherPlayerVisualRoot.transform.localScale;

        scale.x = otherPlayer.FacingRight.Value
            ? Mathf.Abs(scale.x)
            : -Mathf.Abs(scale.x);

        otherPlayerVisualRoot.transform.localScale = scale;
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Border"))
        {
            BorderZone borderZone = other.GetComponent<BorderZone>();

            currentBorderZone = borderZone;
        }
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Border"))
        {
            currentBorderZone = null;
        }
    }
}