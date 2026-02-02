using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
public enum PlayerInputAction {
    RightClick,
    LeftClick
}
public class SmallInformationUI : MonoBehaviour
{
    public static SmallInformationUI instance;
    [SerializeField] Transform tabs;
    [SerializeField] Image smallInformationUI;
    List <MouseInformationTab> mouseInformationTabs;
    [SerializeField] List <DescriptionEntry> descriptionEntries = new ();
    Dictionary <PlayerInputAction, MouseInformationTab> dictionaryMouseInformationTabs = new ();
    Coroutine cnAutoHide;
    bool loadListener = false;
    
    [Header ("DoTween")]
    [SerializeField] Image parentTab;
    Vector3 parentTabStartPos;
    Sequence parentTabSequence;

    [SerializeField] float moveDuration = 0.25f;
    float stayDuration = 2f;
    [SerializeField] float moveOffsetY = 40f;

    public class DescriptionEntry {
        public PlayerInputAction idPlayerInputAction;
        public string miniDescription;
    }

    void Awake () {
        instance = this;
        parentTabStartPos = parentTab.rectTransform.anchoredPosition;
    }


    public void ShowWithAutoHide (float showTime) {
        smallInformationUI.gameObject.SetActive (true);
        stayDuration = showTime;
        if (!loadListener) {
            loadListener = true;
            LoadDictionary ();
        }
        ShowAllDescription ();
        if (cnAutoHide == null) {
            cnAutoHide = GameManager.instance.StartCoroutine (nHide (showTime));
        }

        PlayParentTabAnimation ();
    }

    public void Hide () {
        if (cnAutoHide != null) {
            StopCoroutine (cnAutoHide);
            HideDisplay ();
        }
    }

    IEnumerator nHide (float delayHideTime) {
        yield return new WaitForSeconds (delayHideTime);
        yield return new WaitForSeconds (stayDuration);
       HideDisplay ();
    }

    void HideDisplay () {
        smallInformationUI.gameObject.SetActive (false);
        RemoveAllDescriptionEntry ();
        cnAutoHide =null;
    }

    public void AddDescriptionEntry (PlayerInputAction playerInputAction, string miniDescriptionVal) {
        DescriptionEntry newEntry = new DescriptionEntry {
            idPlayerInputAction = playerInputAction,
            miniDescription = miniDescriptionVal
        };

        descriptionEntries.Add (newEntry);
    }

    #region Show
    void LoadDictionary () {
        mouseInformationTabs = tabs.GetComponentsInChildren<MouseInformationTab>(true).ToList();
        foreach (MouseInformationTab tab in mouseInformationTabs) {
            dictionaryMouseInformationTabs.Add (tab.idPlayerInputAction, tab);
        }
    }

    void ShowAllDescription () {
        foreach (DescriptionEntry entry in descriptionEntries) {
            if (dictionaryMouseInformationTabs.ContainsKey (entry.idPlayerInputAction)) {
                dictionaryMouseInformationTabs[entry.idPlayerInputAction].ChangeText (entry.miniDescription);
            }
        }
    }

    #endregion
    #region Hide
    void RemoveAllDescriptionEntry () {
        
    }
    #endregion
    #region Dotween
    void PlayParentTabAnimation ()
    {
        // Hentikan animasi sebelumnya (kalau ada)
        parentTabSequence?.Kill();

        RectTransform rt = parentTab.rectTransform;

        Vector3 upPos = parentTabStartPos + Vector3.up * moveOffsetY;

        parentTabSequence = DOTween.Sequence();

        parentTabSequence
            .Append(rt.DOAnchorPos(upPos, moveDuration).SetEase(Ease.OutCubic))
            .AppendInterval(stayDuration)
            .Append(rt.DOAnchorPos(parentTabStartPos, moveDuration).SetEase(Ease.InCubic));
    }

    #endregion
    
}
