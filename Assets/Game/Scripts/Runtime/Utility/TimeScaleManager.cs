using UnityEngine;

public class TimeScaleManager : MonoBehaviour
{
    public float TargetTimeScale = 1f;
    void Start()
    {
        Time.timeScale = TargetTimeScale;
    }
}
