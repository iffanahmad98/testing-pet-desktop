using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;

    [Header("Settings Properties")]
    public float hoverShowTime = 0.60f;
    public float hoverHideTime = 0.15f;
    public float hoverTolerance = 8f;
    public float clampX = 40f;
    public float clampY = 10f;

    [Header("Offset Settings")]
    public float offsetX = 10f;
    public float offsetY = 20f;

    [Header("UI Components")]
    [SerializeField] private GameObject tooltipWindow;
    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private CanvasGroup canvasGroup;

    private Coroutine currentCoroutine;
    private Vector2 initialMousePos;

    List <TooltipTriggerWorld> listToolTipTriggerWorld = new ();

    private void Awake()
    {
        if (Instance != this && Instance != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
       // Debug.LogError ("Start Tooltip");
        tooltipWindow.SetActive(false);
    }

    private void Update()
    {
        if (tooltipWindow.activeSelf)
        {
            UpdatePosition();
        }
    }

    public void StartHoverForDuration(string info, float duration)
    {
        StartHover(info);
        StartCoroutine(EndHoverAfterDelay(duration));
    }

    private IEnumerator EndHoverAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideInstant();
    }

    public void StartHover(string info)
    {
        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);

        currentCoroutine = StartCoroutine(ShowTimer(info));
    }

    public void EndHover()
    {
        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);

        currentCoroutine = StartCoroutine(HideTimer());
    }

    private IEnumerator ShowTimer(string info)
    {
        initialMousePos = Mouse.current.position.ReadValue();
        float timer = 0f;
        rectTransform.SetAsLastSibling();

        while (timer < hoverShowTime)
        {
            timer += Time.deltaTime;
            
            // float distance = Vector2.Distance(Mouse.current.position.ReadValue(), initialMousePos);

            // if (distance > hoverTolerance)
            // {
            //     yield break;
            // }

            yield return null;
        }

        Show(info);
    }

    private IEnumerator HideTimer()
    {
        float timer = 0f;

        while (timer < hoverHideTime)
        {
            Vector2 currentMousePos = Mouse.current.position.ReadValue();
            float distance = Vector2.Distance(currentMousePos, initialMousePos);

            if (distance < hoverTolerance)
            {
                timer = 0f;
                //yield break;
            }
            else
            {
                timer += Time.deltaTime;
            }

            yield return null;
        }

        HideInstant();
    }

    private void Show(string info)
    {

        if (string.IsNullOrEmpty(info))
            infoText.gameObject.SetActive(false);
        else
        {
            infoText.gameObject.SetActive(true);
            infoText.text = info;
        }

        UpdatePosition();

        tooltipWindow.SetActive(true);
        if (canvasGroup != null)
            canvasGroup.alpha = 1f;
    }

    public void HideInstant()
    { // this, SceneLoadManager
        tooltipWindow.SetActive(false);
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }

    private void UpdatePosition()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        mousePos += new Vector2(offsetX, offsetY);

        var canvas = rectTransform.GetComponentInParent<Canvas>();
        float scale = canvas ? canvas.scaleFactor : 1f;

        // element size in *screen pixels*
        float w = rectTransform.rect.width * scale;
        float h = rectTransform.rect.height * scale;

        // clamp so the whole rect stays on-screen (accounts for pivot)
        float minX = w * rectTransform.pivot.x;
        float maxX = Screen.width - w * (1f - rectTransform.pivot.x);
        float minY = h * rectTransform.pivot.y;
        float maxY = Screen.height - h * (1f - rectTransform.pivot.y);

        mousePos.x = Mathf.Clamp(mousePos.x, minX, maxX);
        mousePos.y = Mathf.Clamp(mousePos.y, minY, maxY);

        rectTransform.position = mousePos;
    }

    #region RequirementTipClick2d
    public void AddToolTipClick2d (TooltipTriggerWorld requirement) {
        listToolTipTriggerWorld.Add (requirement);
    }

    public void RemoveToolTipClick2d (TooltipTriggerWorld requirement) {
        listToolTipTriggerWorld.Remove (requirement);
    }

    public void ShowAllRequirementTipClick2d () { // HotelShop.cs
        Debug.Log ("Enabled True");
        foreach (TooltipTriggerWorld triggerTip in listToolTipTriggerWorld) {
            Debug.Log ("Enabled True 2");
            triggerTip.enabled = true;
        }
    }
    
    public void HideAllRequirementTipClick2d () { // HotelShop.cs
         foreach (TooltipTriggerWorld triggerTip in listToolTipTriggerWorld) {
            triggerTip.enabled = false;
        }
        HideInstant ();
    }
    #endregion
}
