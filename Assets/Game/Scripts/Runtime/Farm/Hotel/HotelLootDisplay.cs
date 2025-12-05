using UnityEngine;
using DG.Tweening;
using System;
public class HotelLootDisplay : MonoBehaviour
{
    Camera cam;
    GameObject moveObject;
    Transform uiTarget;
    bool isPlay = false;
    Vector3 uiWorldPos;

    // HotelRandomLoot Reference
    public event Action <HotelRandomLootConfig, HotelRandomLootObject> OnTransitionFinished;
    public event Action OnClearTransitionFinished;
    HotelRandomLootConfig config; 
    HotelRandomLootObject configObject;
    public void StartPlay(Transform target, HotelRandomLootConfig configValue, HotelRandomLootObject configObjectValue)
    {
        
        uiTarget = target;
        config = configValue; 
        configObject = configObjectValue;
        Spawn();
    }

    public void StartPlay (Transform target)
    {
        uiTarget = target;
        Spawn ();
    }

    void Spawn()
    {
        cam = Camera.main;
        moveObject = this.gameObject;
        moveObject.transform.localScale = new Vector3 (0,0,0);

        var canvas = uiTarget.GetComponentInParent<Canvas>();
        var canvasRect = canvas.transform as RectTransform;

        var obj = moveObject;
        var rect = obj.GetComponent<RectTransform>();

        Vector2 anchoredPos;
        Vector2 screenPos = Input.mousePosition;

        //  CONVERT SCREEN POS INTO LOCAL UI SPACE
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            null,        // IMPORTANT FIX!
            out anchoredPos
        );

        rect.anchoredPosition = anchoredPos;

        Animate(rect);
    }


    void Animate(RectTransform rect)
    {
        var targetRect = uiTarget.GetComponent<RectTransform>();

        Sequence seq = DOTween.Sequence();

        // first pop
        seq.Append(rect.DOAnchorPos(rect.anchoredPosition + Vector2.up * 120f, 1.0f));
        seq.Join(rect.DOScale(0.5f, 0.7f));

        seq.AppendInterval(0.1f); // time wait

        // curved path section
        Vector3 start = rect.anchoredPosition;
        Vector3 end = targetRect.anchoredPosition;
        Vector3 control = start + new Vector3(100f, -300f);

        Vector3[] points = new Vector3[]
        {
            start,
            control,
            end
        };

        // motion tween object
        var curvedMove = rect.DOLocalPath(points, 0.6f, PathType.CatmullRom)
                            .SetEase(Ease.InOutCubic);

        seq.Append(curvedMove);

        // shrink while moving
        seq.Join(rect.DOScale(0.1f, 0.4f));

        // event fired EXACTLY when movement ends
        curvedMove.OnComplete(() =>
        {
           // Debug.Log ("Finished");
            OnTransitionFinished?.Invoke(config, configObject);
             OnClearTransitionFinished?.Invoke();
        });

        // then final destroy after all animation
        seq.OnComplete(() =>
        {
            Destroy(rect.gameObject);
        });
    }




}
