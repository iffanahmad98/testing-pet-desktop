using UnityEngine;

public partial class TutorialManager
{
    private void StartDialogStep(TutorialStep step)
    {
        HideAllTutorialPanels();

        _activeDialogStep = step;
        _activeDialogIndex = 0;

        Transform parent = dialogRoot != null ? dialogRoot : transform.root;
        var viewInstance = UnityEngine.Object.Instantiate(step.dialogPrefab, parent);
        _activeDialogView = viewInstance;

        BindDialogNextButton();
        ShowCurrentDialogLine();
    }

    private void BindDialogNextButton()
    {
        if (_activeDialogView == null)
            return;

        var nextButton = _activeDialogView.NextButton;
        if (nextButton == null)
            return;

        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(OnDialogNextClicked);
        Debug.Log($"[TutorialManager] BindDialogNextButton ke {nextButton.gameObject.name}");
    }

    private void OnDialogNextClicked()
    {
        Debug.Log($"TutorialManager: Next dialog line untuk stepIndex={_currentStepIndex}, index sebelumnya = {_activeDialogIndex}.");
        _activeDialogIndex++;

        if (_activeDialogIndex >= _activeDialogStep.dialogLines.Count)
        {
            Debug.Log($"TutorialManager: semua dialog line untuk stepIndex={_currentStepIndex} sudah selesai. Menghancurkan dialog dan menandai step selesai.");
            var dialogGo = (_activeDialogView as MonoBehaviour)?.gameObject;
            if (dialogGo != null)
            {
                UnityEngine.Object.Destroy(dialogGo);
            }

            _activeDialogView = null;
            _activeDialogStep = null;
            _activeDialogIndex = 0;

            Debug.Log($"TutorialManager: dialog tutorial untuk stepIndex={_currentStepIndex} selesai, memanggil CompleteCurrent.");
            CompleteCurrent();
        }
        else
        {
            ShowCurrentDialogLine();
        }
    }

    private void ShowCurrentDialogLine()
    {
        if (_activeDialogView == null || _activeDialogStep == null)
            return;

        if (_activeDialogIndex < 0 || _activeDialogIndex >= _activeDialogStep.dialogLines.Count)
            return;

        var line = _activeDialogStep.dialogLines[_activeDialogIndex];
        bool isLast = _activeDialogIndex == _activeDialogStep.dialogLines.Count - 1;

        Debug.Log($"TutorialManager: tampilkan dialog line {_activeDialogIndex + 1}/{_activeDialogStep.dialogLines.Count} untuk stepIndex={_currentStepIndex}.");
        _activeDialogView.SetDialog(line.speakerName, line.text, isLast);
        _activeDialogView.Show();
    }
}
