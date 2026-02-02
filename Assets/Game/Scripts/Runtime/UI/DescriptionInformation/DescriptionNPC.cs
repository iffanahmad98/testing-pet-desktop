using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class DescriptionNPC : MonoBehaviour, IDescriptionTab
{
    [SerializeField] Image descriptionTab;
    [SerializeField] TMP_Text descriptionText;
    #region IDescriptionTab
    // DescriptionInformation.cs
    public void Show () {
        descriptionTab.gameObject.SetActive (true);
    }

    public void Hide () {
        descriptionTab.gameObject.SetActive (false);
    }

    public void SetDescriptionText (string desc) {
        descriptionText.text = desc;
    }
    #endregion
}
