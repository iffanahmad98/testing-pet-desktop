using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class MouseInformationTab : MonoBehaviour {
        public PlayerInputAction idPlayerInputAction;
        [SerializeField] TMP_Text panelText;

        public void ChangeText (string value) {
            panelText.text = value;
        }

        public void HideTab () {
            this.gameObject.SetActive (false);
        }
}
