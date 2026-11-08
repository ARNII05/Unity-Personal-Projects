
using UnityEngine;
using UnityEngine.UI;

public class ParallaxBackgroundUI : MonoBehaviour
{
    [SerializeField] private float globalSpeed = 1f;

    // Velocidades ordenadas de arriba abajo en la jerarquía.
    private readonly float[] speeds =
    {
        0f,        // 10 - Sky
        0.001f,    // 9  - Forest
        0.002f,    // 8  - Forest
        0.003f,    // 7  - Forest
        0.0045f,   // 6  - Forest
        0.006f,    // 5  - Particles
        0.008f,    // 4  - Forest
        0.010f,    // 3  - Particles
        0.012f,    // 2  - Bushes
        0.003f     // 1  - Mist
    };

    private RawImage[] layers;

    private void Awake()
    {
        layers = new RawImage[speeds.Length];

        for (int i = 0; i < speeds.Length; i++)
        {
            layers[i] = transform.GetChild(i).GetComponent<RawImage>();
        }
    }

    private void Update()
    {
        for (int i = 0; i < layers.Length; i++)
        {
            Rect uv = layers[i].uvRect;

            uv.x = Mathf.Repeat(
                uv.x + speeds[i] * globalSpeed * Time.deltaTime,
                1f
            );

            layers[i].uvRect = uv;
        }
    }
}
