
using UnityEngine;

public class WolfSound : MonoBehaviour
{
    [SerializeField] private AudioSource wolfSound;
    [SerializeField] private int probToTriggerSound = 15;
    [SerializeField] private float minTimeBetweenSounds = 30f;
    [SerializeField] private float cdAfterSound = 60f;

    private float timeSinceLastSound = 0f;
    private float cooldownTimer = 0f;

    private bool soundTriggered = false;

    private void Update()
    {
        if (soundTriggered)
        {
            cooldownTimer += Time.deltaTime;

            if (cooldownTimer >= cdAfterSound)
            {
                soundTriggered = false;
                cooldownTimer = 0f;
            }

            return;
        }

        timeSinceLastSound += Time.deltaTime;

        if (timeSinceLastSound >= minTimeBetweenSounds)
        {
            timeSinceLastSound = 0f;

            int randomValue = Random.Range(0, 100);

            if (randomValue < probToTriggerSound)
            {
                wolfSound.Play();
                soundTriggered = true;
            }
        }
    }
}
