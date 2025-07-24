using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace MagicalGarden.Farm
{
    public class CropInformation : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI titleCrop;
        public Image cropImage;
        public Image statusLifeImage;
        public TextMeshProUGUI statusLifeText;
        public Image fertiTypeImage;
        public TextMeshProUGUI fertiGrowthSpeedText;
        public TextMeshProUGUI waterTimeText;
        public TextMeshProUGUI lifeTimeText;
        [Header("Ferti Info")]
        public GameObject fertiAvailable;
        public GameObject fertiNone;

        public void SetFertiInfo(bool value)
        {
            fertiAvailable.SetActive(value);
            fertiNone.SetActive(!value);
        }
    }
}
