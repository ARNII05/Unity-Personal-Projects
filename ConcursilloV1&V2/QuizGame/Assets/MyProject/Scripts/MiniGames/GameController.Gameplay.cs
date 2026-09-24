using UnityEngine;
using UnityEngine.EventSystems;

public partial class GameController
{
    void ManageKeys()
    {
        bool userSearchSelected = EventSystem.current.currentSelectedGameObject == userSearch.gameObject;

        if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) && !userSearchSelected)
            CheckAnswerWithActualMode();

        if (Input.GetKeyDown(KeyCode.RightArrow))
            SwapCheckpoint(KeyCode.RightArrow, green);

        if (Input.GetKeyDown(KeyCode.LeftArrow))
            SwapCheckpoint(KeyCode.LeftArrow, purple);
    }

    void ToggleOptions()
    {
        bool openOptions = !optionsPanel.activeSelf;

        if (inQuiz) quiz.SetActive(!openOptions);
        else summary.SetActive(!openOptions);

        optionsPanel.SetActive(openOptions);
        ResetInstructionsAnimation();

        if (!openOptions)
            RefreshVideoPreview();
    }
}
