using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
public enum PlayerInputAction {
    RightClick,
    LeftClick
}
public class SmallInformationUI : MonoBehaviour
{
    [SerializeField] Image smallInformationUI;
    List <MouseInformationTab> mouseInformationTabs;
    [SerializeField] List <DescriptionEntry> descriptionEntries = new ();
    Dictionary <PlayerInputAction, MouseInformationTab> dictionaryMouseInformationTabs = new ();
    bool loadListener = false;
    
    public class DescriptionEntry {
        public PlayerInputAction idPlayerInputAction;
        public string miniDescription;
    }

    public void Show (int showTime) {
        smallInformationUI.gameObject.SetActive (true);
        if (!loadListener) {
            loadListener = true;
            LoadDictionary ();
        }
        ShowAllDescription ();
        Invoke ("Hide", showTime);
    }

    void Hide () {
        smallInformationUI.gameObject.SetActive (false);
        RemoveAllDescriptionEntry ();
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
        mouseInformationTabs = GetComponentsInChildren<MouseInformationTab>(true).ToList();
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

    
}
