using UnityEngine.UI;
public interface ITutorialDialogView
{
    Button NextButton { get; }
    void SetDialog(string speakerName, string text, bool isLastStep);
    void Show();
    void Hide();
}