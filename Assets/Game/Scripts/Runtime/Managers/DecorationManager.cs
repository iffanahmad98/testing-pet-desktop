using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;

public class DecorationManager : MonoBehaviour {
   //  [SerializeField] Transform decorations;
    [SerializeField] Transform gameArea;
    
    
    void Awake () {
        SaveSystem.DataLoaded += LoadDecorations;
    }
    

    void Start () {
        ServiceLocator.Register (this);
        
    }

    public void ApplyDecorationByID (string id) { // DecorationShopManager
        foreach (Transform decor in gameArea) {
            if (decor.gameObject.tag == "Decoration" && decor.gameObject.name == id) {
                
                decor.gameObject.SetActive (true);

                if (decor.GetComponent<DecorationMultipleObject>()) {
                    
                } else {
                    return;

                }
                
            }
        }
    }

    public void RemoveActiveDecoration (string id) {
        foreach (Transform decor in gameArea) {
            if (decor.gameObject.name == id) {
                decor.gameObject.SetActive (false);
                if (decor.GetComponent<DecorationMultipleObject>()) {
                    
                } else {
                    return;
                    
                }
            }
        }
    }
    
    #region Data
    // SaveSystem
    public void LoadDecorations (PlayerConfig playerConfig) {
        foreach (OwnedDecorationData ownedDecorationData in playerConfig.ownedDecorations) {
            if (ownedDecorationData.isActive) {
                ApplyDecorationByID (ownedDecorationData.decorationID);
                DecorationShopManager.instance.SetLastLoadTreeDecoration1 (ownedDecorationData.decorationID);
            }
        }
    }

    #endregion
}
