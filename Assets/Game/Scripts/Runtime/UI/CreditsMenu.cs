using DG.Tweening;
using TMPro;
using UnityEngine;

public class CreditsMenu : MonoBehaviour
{
    [SerializeField] private TMP_Text versionTxt;

    private void Start()
    {
        versionTxt.SetText($"app ver {Application.version}");
    }

    public void CloseCreditsPanel()
    {
        Sequence seq = DOTween.Sequence();

        _ =seq.AppendCallback(() =>
        {
            transform.DOPunchScale(Vector3.one * 0.1f, 0.2f);
        });
        seq.AppendInterval(0.2f);
        seq.AppendCallback(() => { gameObject.SetActive(false); });
    }

    public void GoToDiscordLink() => Application.OpenURL("https://discord.gg/cpBTNMeU");
    public void GoToWebsiteLink() => Application.OpenURL("https://petalpals.com/");
    public void GoToTwitterLink() => Application.OpenURL("https://x.com/petalpalsid");
    public void GoToYoutubeLink() => Application.OpenURL("https://www.youtube.com/@ScriptSmelterS");
}
