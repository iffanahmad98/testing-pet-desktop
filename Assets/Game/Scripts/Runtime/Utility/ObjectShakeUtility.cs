using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public static class ObjectShakeUtility
{
    /// <summary>
    /// Shake single object then return to original position.
    /// </summary>
    public static void ShakeSingle(GameObject target, Vector3 originalPosition, 
        float duration, float strength, int vibrato)
    {
        if (target == null) return;

        target.transform.DOShakePosition(duration, strength, vibrato)
            .OnComplete(() =>
            {
                target.transform.DOMove(originalPosition, 0.1f);
            });
    }

    /// <summary>
    /// Shake all objects inside list of ObjectClickableData.
    /// </summary>
    public static void ShakeList(List<ObjectClickableData> shakeDataList,
        float duration, float strength, int vibrato)
    {
        if (shakeDataList == null) return;

        foreach (var data in shakeDataList)
        {
            if (data.clickObject == null) continue;

            ShakeSingle(data.clickObject, data.originalPosition, duration, strength, vibrato);
        }
    }

    /// <summary>
    /// Reset all objects back to their stored original positions.
    /// </summary>
    public static void ResetToOriginal(List<ObjectClickableData> shakeDataList, float duration = 0.1f)
    {
        if (shakeDataList == null) return;

        foreach (var data in shakeDataList)
        {
            if (data.clickObject == null) continue;

            data.clickObject.transform.DOMove(data.originalPosition, duration);
        }
    }

    /// <summary>
    /// Handle hover enter + exit scale tween.
    /// </summary>
    public static void ShakeEnterExit(
        List<GameObject> enter,
        List<GameObject> exit,
        float enterScale,
        float exitScale,
        float tweenDuration = 0.15f)
    {
        if (enter != null)
        {
            foreach (var obj in enter)
            {
                if (obj != null)
                    obj.transform.DOScale(enterScale, tweenDuration);
            }
        }

        if (exit != null)
        {
            foreach (var obj in exit)
            {
                if (obj != null)
                    obj.transform.DOScale(exitScale, tweenDuration);
            }
        }
    }
}
