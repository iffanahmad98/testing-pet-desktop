using UnityEngine;

public class GameManager : MonoBehaviour
{
  public static GameManager instance;
  public bool isQuitting = false;

  void Awake () {
    instance = this;
  }

  private void OnApplicationQuit()
  {
    isQuitting = true;
  }
}
