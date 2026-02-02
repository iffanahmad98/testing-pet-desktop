using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
public enum DescriptionInfoType {
    Default,
    NPC,
}
public class DescriptionInformation : MonoBehaviour
{
   public static DescriptionInformation instance;
    [SerializeField] Transform tabs;
    [SerializeField] Button closeButton;
    [SerializeField] Image descriptionInformationUI;
    List <IDescriptionTab> descriptionTabs;
    Dictionary <DescriptionInfoType, IDescriptionTab> dictionaryDescriptionTabs = new ();
    Coroutine cnAutoHide;
    bool loadListener = false;
    
    /*
    [Header ("DoTween")]
    [SerializeField] Image parentTab;
    Vector3 parentTabStartPos;
    Sequence parentTabSequence;

    [SerializeField] float moveDuration = 0.25f;
    float stayDuration = 2f;
    [SerializeField] float moveOffsetY = 40f;
    */

    void Awake () {
        instance = this;
        // parentTabStartPos = parentTab.rectTransform.anchoredPosition;
    }
   
    void Show (DescriptionInfoType type, string description) {
        descriptionInformationUI.gameObject.SetActive (true);
        if (!loadListener) {
            loadListener = true;
            closeButton.onClick.AddListener (Hide);
            LoadDictionary ();
        }
        ShowAllDescription ();

    }

    void Hide () {
      HideDisplay ();
    }

    void HideDisplay () {
        descriptionInformationUI.gameObject.SetActive (false);
        cnAutoHide =null;
    }

    #region Show
    void LoadDictionary () {
        descriptionTabs = tabs.GetComponentsInChildren<IDescriptionTab>(true).ToList();
        /*
        foreach (IDescriptionTab tab in mouseInformationTabs) {
            dictionaryDescriptionTabs.Add (tab.idPlayerInputAction, tab);
        }
        */
    }

    void ShowAllDescription () {
        /*
        foreach (DescriptionEntry entry in descriptionEntries) {
            if (dictionaryDescriptionTabs.ContainsKey (entry.idPlayerInputAction)) {
                dictionaryDescriptionTabs[entry.idPlayerInputAction].ChangeText (entry.miniDescription);
            }
        }
        */
    }

    #endregion
 
    #region Dotween
    /*
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
    */
    #endregion
}
