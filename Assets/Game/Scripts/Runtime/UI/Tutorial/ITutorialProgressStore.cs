public interface ITutorialProgressStore
{
    bool IsCompleted(string tutorialId);
    void MarkCompleted(string tutorialId);
    void Clear(string tutorialId);
}

public interface ITutorialService
{
    bool HasCompleted(string tutorialId);
    bool TryStart(string tutorialId);
    void Complete(string tutorialId);
    void Reset(string tutorialId);
    void ResetAll();
}

