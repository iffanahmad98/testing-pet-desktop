using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class SceneLoadManager : MonoBehaviour {
    public static SceneLoadManager Instance;

    void Awake () {
        Instance = this;
    }

    public void SetUIEqualsFocus (string value) { // BoardSign
        StartCoroutine (nSetUIEqualsFocus (value));
    }

    IEnumerator nSetUIEqualsFocus (string value) {
        yield return new WaitUntil (() => MagicalGarden.Farm.UIManager.Instance);
      //  Debug.Log ("Focus Scene 2 ");
        MagicalGarden.Farm.UIManager.Instance.SetUIEqualsFocus(value);
        if (value == "Hotel") {
            FarmMainUI.instance.Hide ();
        } else if (value == "Farm") {
            FarmMainUI.instance.Show ();
        }
    }
}
