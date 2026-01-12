using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GotItemMotion : MonoBehaviour {
    [SerializeField] Image itemIcon;
    [SerializeField] TMP_Text itemNumber;

    public void ChangeDisplay (Sprite itemSprite, Vector3 itemScale, int value) {
        itemIcon.sprite = itemSprite;
        itemIcon.transform.localScale = itemScale;
        itemNumber.text = "+"+ value.ToString ();
        this.gameObject.SetActive (true);
    }    
}
