using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    [SerializeField]
    private GameObject[] _uiObjects;

    private bool show = false;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            show = !show;

            foreach (GameObject go in _uiObjects)
            {
                go.SetActive(show);
            }
        }
    }
}
