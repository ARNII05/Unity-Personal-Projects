
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class OtherPlayerVisual : MonoBehaviour
{
    private static readonly int DirectionHash =
        Animator.StringToHash("Direction");

    private static readonly int IsWalkingHash =
        Animator.StringToHash("IsWalking");

    [SerializeField] private GameObject visualPrefab;

    private Player ownerPlayer;
    private GameObject visualRoot;
    private GameObject visualInstance;
    private Animator visualAnimator;

    private Coroutine showCoroutine;

    public void Initialize(Player player)
    {
        ownerPlayer = player;

        if (visualRoot != null)
            return;

        visualRoot = new GameObject("Other Player Visual Root");

        visualRoot.transform.SetPositionAndRotation(
            ownerPlayer.transform.position,
            ownerPlayer.transform.rotation
        );

        visualRoot.transform.localScale = Vector3.one * 1.5f;

        visualInstance = Instantiate(
            visualPrefab,
            visualRoot.transform
        );

        visualAnimator = visualInstance.GetComponent<Animator>();

        visualRoot.SetActive(false);
    }

    public void UpdateVisual()
    {
        if (ownerPlayer == null || !ownerPlayer.IsOwner)
            return;

        if (visualRoot == null)
            return;

        Player otherPlayer =
            MapManager.Instance.GetOtherPlayer(ownerPlayer);

        if (otherPlayer == null ||
            ownerPlayer.currentZone == null ||
            otherPlayer.currentZone == null)
        {
            visualRoot.SetActive(false);
            return;
        }

        bool sameZone =
            ownerPlayer.NetworkMapPos.Value ==
            otherPlayer.NetworkMapPos.Value;

        if (!sameZone)
        {
            visualRoot.SetActive(false);
            return;
        }

        Vector3 visualPosition =
            ownerPlayer.currentZone.transform.position +
            (otherPlayer.transform.position -
             otherPlayer.currentZone.transform.position);

        visualRoot.transform.position = visualPosition;
        visualRoot.SetActive(true);

        if (visualAnimator == null)
            return;

        visualAnimator.SetInteger(
            DirectionHash,
            otherPlayer.NetworkDirection.Value
        );

        visualAnimator.SetBool(
            IsWalkingHash,
            otherPlayer.IsWalking.Value
        );
    }

    public void HideTemporarily(float delay = 0.1f)
    {
        if (ownerPlayer == null || !ownerPlayer.IsOwner)
            return;

        if (visualRoot == null)
            return;

        if (showCoroutine != null)
            StopCoroutine(showCoroutine);

        visualRoot.SetActive(false);
        showCoroutine = StartCoroutine(ShowAfterDelay(delay));
    }

    private IEnumerator ShowAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        UpdateVisual();

        showCoroutine = null;
    }

    private void OnDestroy()
    {
        if (showCoroutine != null)
            StopCoroutine(showCoroutine);

        if (visualRoot != null)
            Destroy(visualRoot);
    }
}