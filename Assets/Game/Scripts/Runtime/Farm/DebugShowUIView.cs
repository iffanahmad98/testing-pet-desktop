using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    [SerializeField]
    private GameObject[] _uiObjects;

    private bool show = false;

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.U))
        {
            show = !show;

            foreach (GameObject go in _uiObjects)
            {
                go.SetActive(show);
            }
        }
#endif
    }
}