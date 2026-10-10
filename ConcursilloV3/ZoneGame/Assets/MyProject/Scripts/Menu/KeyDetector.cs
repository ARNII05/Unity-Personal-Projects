using UnityEngine;
using UnityEngine.SceneManagement;

public class KeyDetector : MonoBehaviour
{
    private ZoneGameControls controls;

    private void Awake()
    {
        controls = new ZoneGameControls();
    }

    private void OnEnable()
    {
        controls.Gameplay.Confirm.performed += OnConfirm;
        controls.Gameplay.Enable();
    }

    private void OnDisable()
    {
        controls.Gameplay.Confirm.performed -= OnConfirm;
        controls.Gameplay.Disable();
    }

    private void OnDestroy()
    {
        controls?.Dispose();
    }

    private void OnConfirm(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        SceneManager.LoadScene(1);
    }
}