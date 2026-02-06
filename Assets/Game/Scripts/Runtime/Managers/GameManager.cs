using UnityEngine;
using UnityEngine.SceneManagement; 
public class GameManager : MonoBehaviour
{
  public static GameManager instance;
  public bool isQuitting = false;

  void Awake () {
    instance = this;
  }

  void Start () {
    SceneManager.LoadScene("TooltipScene", LoadSceneMode.Additive);
  }

  private void OnApplicationQuit()
  {
    isQuitting = true;
  }
}
