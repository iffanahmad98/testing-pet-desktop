using UnityEngine;

public class HelperHote : MonoBehaviour
{

    public void Awake()
    {
        SceneFocusManager.SetFocusTarget(SceneFocusManager.FocusTarget.Hotel);
        SceneLoadManager.Instance.SetUIEqualsFocus("Hotel");
    }

}