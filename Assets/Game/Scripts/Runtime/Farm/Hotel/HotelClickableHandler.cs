using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using System;
public class ObjectClickableData {
    public GameObject clickObject;
    public Vector3 originalPosition;
}

public class HotelClickableHandler : MonoBehaviour
{
    [Header ("Main")]
    Camera cam;
    GameObject currentHover;

    public List<GameObject> enterList = new List<GameObject>();
    public List<GameObject> exitList = new List<GameObject>();
    public List<ObjectClickableData> shakeList = new List <ObjectClickableData> ();
    public List<ObjectClickableData> dataShakeList = new List <ObjectClickableData> ();
    public HotelClickableDatabaseSO hotelClickableDatabaseSO;

    [Header("Raycast")]
    public LayerMask clickableMask;
    
    [Header("Shake Settings")]
    public float duration = 0.15f;
    public float strength = 0.5f;
    public int vibrato = 3;

    public event Action <GameObject> OnShakedObject; // HotelRandomLoot
    void Awake()
    {
        cam = Camera.main;
    }

    void Update()
    {
        enterList.Clear();
        exitList.Clear();

        DetectHover();
        DetectClick();

        ProcessHoverAnimation(); // kalau mau langsung disini
        ProcessShakeAnimation ();
    }

    void DetectHover()
    {
        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, 100f, clickableMask);

        if(hit.collider != null)
        {
            GameObject obj = hit.collider.gameObject;

            if(obj != currentHover)
            {
                if(currentHover != null && currentHover.CompareTag("ClickableDecoration"))
                    exitList.Add(currentHover);
              //  Debug.Log ("Hover " + obj.name);
                currentHover = obj;

                if(obj.CompareTag("ClickableDecoration"))
                    enterList.Add(obj);
            }
        }
        else
        {
            if(currentHover != null && currentHover.CompareTag("ClickableDecoration"))
                exitList.Add(currentHover);

            currentHover = null;
        }
    }

     void DetectClick()
    {
        if(!Input.GetMouseButtonDown(0)) return;

        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, 100f, clickableMask);
        
        if(hit.collider != null && hit.collider.CompareTag("ClickableDecoration"))
        {
            GameObject obj = hit.collider.gameObject;

            // ⛔ PENTING
            // kalau objek ini SUDAH masuk queue shake, abaikan klik
            if(shakeList.Exists(x => x.clickObject == obj))
            {
                return;
            }

            // baru boleh tambah
            var data = new ObjectClickableData();
            data.clickObject = obj;
            data.originalPosition = obj.transform.position;
            shakeList.Add(data);
            if(!dataShakeList.Exists(x => x.clickObject == obj))
            {
                dataShakeList.Add (data);
            }
            OnShakedObject?.Invoke(obj);
        }
    }


    void ProcessHoverAnimation()
    {
        foreach(var obj in enterList)
        {
            // nanti diganti tween scale up
         //   Debug.Log("Hover Enter Anim: " + obj.name);
         obj.transform.DOScale(hotelClickableDatabaseSO.GetHotelClickableDataSO(obj.name).objectHoverScale, 0.15f);
        }

        foreach(var obj in exitList)
        {
            // nanti diganti tween scale down
          //  Debug.Log("Hover Exit Anim: " + obj.name);
          obj.transform.DOScale(hotelClickableDatabaseSO.GetHotelClickableDataSO(obj.name).objectNormalScale, 0.15f);
        }
    }

    void ProcessShakeAnimation ()
    {
        var temp = shakeList.ToArray();

        foreach(var data in temp)
        {
            data.clickObject.transform
                .DOShakePosition(duration, strength, vibrato)
                .OnComplete(() =>
                {
                    data.clickObject.transform.DOMove(GetElementDataShakeList(data.clickObject).originalPosition, 0.1f)
                        .OnComplete(() =>
                        {
                            shakeList.Remove(data);
                        });
                });
        }
    }

    ObjectClickableData GetElementDataShakeList (GameObject targetObject) {
        return dataShakeList.Find(x => x.clickObject == targetObject);
    }

}
