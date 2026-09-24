using System.Collections;
using TMPro;
using UnityEngine;

public partial class GameController
{
    IEnumerator ControlAnimation(Animator animator, string text, float animTime)
    {
        animator.Play("textAnim", 0, 0f);
        animator.SetBool("DoAnim", true);

        yield return new WaitForSecondsRealtime(animTime);

        animator.SetBool("DoAnim", false);

        instructionsCoroutine = null;
    }

    void SetIndications(Animator animator, string text, float animTime)
    {
        if (animator == null) return;

        if (instructionsCoroutine != null)
        {
            StopCoroutine(instructionsCoroutine);

            animator.SetBool("DoAnim", false);
            animator.Play("BaseText", 0, 0f);
        }

        instructionsText.GetComponent<TextMeshProUGUI>().text = text;

        instructionsCoroutine = StartCoroutine(ControlAnimation(animator, text, animTime));
    }

    void ResetInstructionsAnimation()
    {
        if (textAnimator == null) return;

        if (instructionsCoroutine != null)
        {
            StopCoroutine(instructionsCoroutine);
            instructionsCoroutine = null;
        }

        if (!textAnimator.gameObject.activeSelf) return;

        textAnimator.SetBool("DoAnim", false);
        textAnimator.Play("BaseText", 0, 0f);
        textAnimator.Update(0f);
    }
}
