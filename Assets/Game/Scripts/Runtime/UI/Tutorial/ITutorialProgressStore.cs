public interface ITutorialProgressStore
{
    bool IsCompleted(int stepIndex);
    void MarkCompleted(int stepIndex);
    void ClearAll(int stepCount);
}

public interface ITutorialService
{
    bool HasAnyPending();
    bool TryStartNext();
    void CompleteCurrent();
    void ResetAll();
}

