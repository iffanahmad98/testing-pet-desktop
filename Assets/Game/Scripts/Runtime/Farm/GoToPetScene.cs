using UnityEngine;

public class GoToPetScene : MonoBehaviour
{
    public void PetScene()
    {
        var sceneLoader = AdditiveSceneLoader.Instance;
        if (sceneLoader != null)
        {
            sceneLoader.SwitchToPetScene();
        }
        else
        {
            Debug.LogError("[GoToPetScene] AdditiveSceneLoader instance not found!");
        }
    }
}
