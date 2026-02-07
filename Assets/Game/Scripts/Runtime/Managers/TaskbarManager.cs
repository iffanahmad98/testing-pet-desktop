using UnityEngine;

public class TaskbarManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       string result = TaskbarPosition.GetTaskbarPosition ();
       Debug.LogError ($"Taskbar {result}");
     //  int taskbarHeight = TaskbarHeight.GetTaskbarHeight ();
      // Debug.LogError ($"Taskbar Height {taskbarHeight}");
    }
}
