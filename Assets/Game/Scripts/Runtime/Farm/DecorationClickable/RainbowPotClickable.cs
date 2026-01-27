using UnityEngine;
using UnityEngine.UI;

public class RainbowPotClickable : DecorationClickable
{
    [SerializeField] private Button potButton;
    [SerializeField] private Image potImage;
    [SerializeField] private bool isActive;
    [SerializeField] private Sprite onImage;
    [SerializeField] private Sprite offImage;

    private void Start()
    {
        potButton.onClick.AddListener(OnClick);
    }

    private void Awake()
    {
        if (isActive)
        {
            OnEvent();
        }
        else
        {
            OffEvent();
        }
    }

    public override void DecorationTurnedOff()
    {
        isActive = false;
        OffEvent();
    }

    public override void OnClick()
    {
        if (isActive)
        {
            isActive = false;
            OffEvent();
        }
        else
        {
            isActive = true;
            OnEvent();
        }
    }

    private void OnEvent()
    {
        potImage.sprite = onImage;
        MonsterManager.instance.audio.PlaySFX("rainbow_pot");
    }

    private void OffEvent()
    {
        potImage.sprite = offImage;
    }
}
