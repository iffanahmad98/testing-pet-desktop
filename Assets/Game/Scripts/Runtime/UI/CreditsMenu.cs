using DG.Tweening;
using TMPro;
using UnityEngine;

public class CreditsMenu : MonoBehaviour
{
    [SerializeField] private CanvasGroup cGroup;
    [SerializeField] private TMP_Text versionTxt;

    private void Start()
    {
        versionTxt.SetText($"app ver {Application.version}");
    }

    public void CloseCreditsPanel()
    {
        MonsterManager.instance.audio.PlaySFX("menu_close");
        cGroup.DOFade(0f, 0.2f).SetEase(Ease.InQuad).OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }

    public void GoToDiscordLink() => Application.OpenURL("https://discord.gg/cpBTNMeU");
    public void GoToWebsiteLink() => Application.OpenURL("https://petalpals.com/");
    public void GoToTwitterLink() => Application.OpenURL("https://x.com/petalpalsid");
    public void GoToYoutubeLink() => Application.OpenURL("https://www.youtube.com/@ScriptSmelterS");
    public void PlayClickAudio()=> MonsterManager.instance.audio.PlaySFX("button_click");
}
