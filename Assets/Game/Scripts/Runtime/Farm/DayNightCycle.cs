// using UnityEngine;
// using UnityEngine.Rendering.Universal;

// public class DayNightCycle : MonoBehaviour
// {
//     public Light2D globalLight;
//     public Color dayColor = Color.white;
//     public Color nightColor = new Color(0.1f, 0.1f, 0.3f);
//     public float transitionDuration = 1.5f;

//     public void SetNight(bool night)
//     {
//         StopAllCoroutines();
//         StartCoroutine(TransitionTo(night ? nightColor : dayColor));
//     }

//     IEnumerator TransitionTo(Color targetColor)
//     {
//         Color startColor = globalLight.color;
//         float t = 0f;
//         while (t < 1f)
//         {
//             t += Time.deltaTime / transitionDuration;
//             globalLight.color = Color.Lerp(startColor, targetColor, t);
//             yield return null;
//         }
//     }
// }