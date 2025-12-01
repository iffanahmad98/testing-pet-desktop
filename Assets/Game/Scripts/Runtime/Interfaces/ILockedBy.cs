using UnityEngine;

public interface ILockedBy { // CameraDragLocker
    void AddLockedBy (GameObject value);
    void RemoveLockedBy (GameObject value);
    bool IsCan ();
}
