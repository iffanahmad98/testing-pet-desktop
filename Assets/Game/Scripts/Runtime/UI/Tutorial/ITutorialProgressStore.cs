public interface ITutorialService
{
    bool HasAnyPending();
    bool TryStartNext();
    void CompleteCurrent();
    void ResetAll();
}

