using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public partial class TutorialManager
{
    private ClickableObject _currentHotelClickableNextTarget;

    public void ShowNextHotelPanel()
    {
        Debug.Log($"[HotelTutorial] ShowNextHotelPanel ENTER | index={_hotelPanelIndex} | total={hotelTutorials?.Count ?? 0} | mode={_currentMode}");

        if (hotelTutorials == null || hotelTutorials.Count == 0)
        {
            Debug.LogWarning("[HotelTutorial] ABORT: hotelTutorials null / empty");
            return;
        }

        if (_hotelPanelIndex < 0)
        {
            StartHotelTutorialSequence();
            return;
        }

        Button previousNextButton = null;
        if (_hotelPanelIndex < hotelTutorials.Count)
        {
            var currentStep = hotelTutorials[_hotelPanelIndex];
            var currentConfig = currentStep != null ? currentStep.config : null;
            if (currentStep != null && currentStep.panelRoot != null)
                currentStep.panelRoot.SetActive(false);

            if (currentConfig != null &&
                !currentConfig.useClickableObjectAsNext &&
                !string.IsNullOrEmpty(currentConfig.nextButtonName))
            {
                previousNextButton = GetHotelStepNextButton(currentStep);
            }
        }

        if (_currentHotelClickableNextTarget != null)
        {
            _currentHotelClickableNextTarget.OnClicked -= HandleHotelClickableNextClicked;
            _currentHotelClickableNextTarget = null;
        }

        _hotelPanelIndex++;
        Debug.Log($"TutorialManager: moving to hotel panel index {_hotelPanelIndex}");

        if (_hotelPanelIndex >= hotelTutorials.Count)
        {
            Debug.Log("[HotelTutorial] FINISHED: no more hotelTutorials steps");
            MarkHotelTutorialCompleted();
            RestoreUIManagerButtonsInteractable();
            gameObject.SetActive(false);
            return;
        }

        var nextStep = hotelTutorials[_hotelPanelIndex];
        var nextConfig = nextStep != null ? nextStep.config : null;

        Button nextStepButton = null;
        if (nextConfig != null &&
            !nextConfig.useClickableObjectAsNext &&
            !string.IsNullOrEmpty(nextConfig.nextButtonName))
        {
            nextStepButton = GetHotelStepNextButton(nextStep);
        }

        if (previousNextButton != null)
        {
            previousNextButton.interactable = false;
            previousNextButton.gameObject.SetActive(false);
        }

        if (nextStepButton != null)
        {
            nextStepButton.gameObject.SetActive(true);
            nextStepButton.interactable = true;
        }

        if (nextStep != null && nextStep.panelRoot != null && nextConfig != null)
        {
            Debug.Log($"[HotelTutorial] Showing NEXT panel | index={_hotelPanelIndex} | panel={nextStep.panelRoot.name} | handPointer={(nextConfig.handPointerSequence != null)}");

            PlayHotelPanelShowAnimation(nextStep.panelRoot);
            _hotelStepShownTime = Time.time;

            // Jika step ini mengaktifkan tutorial gift, spawn gift dari kamar terakhir
            // yang dipakai check-in sebelum memulai sub-tutorial/pointer.
            TrySpawnTutorialGiftForCurrentStep();

            if (nextConfig.handPointerSequence != null)
            {
                StartHandPointerHotelSubTutorial(nextStep);
            }
            else
            {
                UpdateHotelStepNextButtonsInteractable();
                UpdatePointerForHotelStep(nextStep);
                TryStartHotelCameraFollowForCurrentStep();
            }

            EnsureHotelNextButtonListenerForStep(nextStep);
            SetupHotelClickableNextForStep(nextStep);
        }
    }

    private void StartHotelTutorialSequence()
    {
        if (hotelTutorials == null || hotelTutorials.Count == 0)
        {
            Debug.LogWarning("TutorialManager: hotelTutorials kosong, tidak ada tutorial hotel yang ditampilkan.");
            return;
        }

        for (int i = 0; i < hotelTutorials.Count; i++)
        {
            var step = hotelTutorials[i];
            if (step == null)
                continue;

            if (step.panelRoot != null)
                step.panelRoot.SetActive(false);
        }

        _hotelPanelIndex = 0;
        var firstStep = hotelTutorials[_hotelPanelIndex];
        var firstConfig = firstStep != null ? firstStep.config : null;
        if (firstStep != null && firstStep.panelRoot != null && firstConfig != null)
        {
            Debug.Log($"[HotelTutorial] Showing FIRST panel | index={_hotelPanelIndex} | panel={firstStep.panelRoot.name} | handPointer={(firstConfig.handPointerSequence != null)}");
            PlayHotelPanelShowAnimation(firstStep.panelRoot);
            _hotelStepShownTime = Time.time;

            // Jika step ini mengaktifkan tutorial gift, spawn gift dari kamar terakhir
            // yang dipakai check-in sebelum memulai sub-tutorial/pointer.
            TrySpawnTutorialGiftForCurrentStep();

            if (firstConfig.handPointerSequence != null)
            {
                StartHandPointerHotelSubTutorial(firstStep);
            }
            else
            {
                UpdateHotelStepNextButtonsInteractable();
                UpdatePointerForHotelStep(firstStep);

                // Untuk step hotel pertama tanpa hand pointer sequence,
                // cek apakah perlu fokus kamera (dan lanjut otomatis setelah durasi).
                TryStartHotelCameraFollowForCurrentStep();
            }
            EnsureHotelNextButtonListenerForStep(firstStep);
            SetupHotelClickableNextForStep(firstStep);
        }
    }

    private void UpdatePointerForHotelStep(HotelTutorialPanelStep step)
    {
        var pointer = ServiceLocator.Get<ITutorialPointer>();
        if (pointer == null)
        {
            Debug.LogWarning("[HotelTutorial] UpdatePointerForHotelStep: ITutorialPointer tidak ditemukan di ServiceLocator.");
            return;
        }
        var config = step != null ? step.config : null;

        bool wantsPointer = config != null && config.usePointer;
        if (!wantsPointer)
        {
            pointer.Hide();
            return;
        }

        if (config != null && config.useClickableObjectAsPointerTarget)
        {
            var clickable = ResolveHotelClickableObject(config);
            if (clickable != null)
            {
                var t = clickable.transform;
                Debug.Log($"[HotelTutorial] UpdatePointerForHotelStep: PointToWorld ke ClickableObject '{clickable.gameObject.name}' dengan id='{config.clickableObjectId}'");
                pointer.PointToWorld(t, config.pointerOffset);
                return;
            }
            else
            {
                Debug.LogWarning($"[HotelTutorial] UpdatePointerForHotelStep: ClickableObject dengan id='{config.clickableObjectId}' tidak ditemukan untuk pointer.");
            }
        }

        if (config != null && config.useNextButtonAsPointerTarget)
        {
            var btn = GetHotelStepNextButton(step);
            if (btn != null)
            {
                var rect = btn.transform as RectTransform;
                if (rect != null)
                {
                    pointer.PointTo(rect, config.pointerOffset);
                    return;
                }
            }
        }

        pointer.Hide();
    }

    private ClickableObject ResolveHotelClickableObject(HotelTutorialStepConfig config)
    {
        if (config == null)
            return null;

        return ResolveHotelClickableObjectById(config.clickableObjectId);
    }

    private ClickableObject ResolveHotelClickableObjectById(string clickableObjectId)
    {
        Debug.Log($"[HotelTutorial] Resolving ClickableObject for id '{clickableObjectId}'");
        if (string.IsNullOrEmpty(clickableObjectId))
            return null;

        Debug.Log($"[HotelTutorial] Resolving ClickableObject: searching all ClickableObjects in scene for id '{clickableObjectId}'");
        var all = Object.FindObjectsOfType<ClickableObject>(true);
        for (int i = 0; i < all.Length; i++)
        {
            var c = all[i];
            if (c != null && string.Equals(c.tutorialId, clickableObjectId, System.StringComparison.Ordinal))
                return c;
        }

        return null;
    }

    private bool TrySpawnTutorialGiftForCurrentStep()
    {
        if (hotelTutorials == null || hotelTutorials.Count == 0)
            return false;

        if (_hotelPanelIndex < 0 || _hotelPanelIndex >= hotelTutorials.Count)
            return false;

        var currentStep = hotelTutorials[_hotelPanelIndex];
        var config = currentStep != null ? currentStep.config : null;

        if (config == null || !config.spawnTutorialGiftFromLastAssignedHotelRoom)
            return false;

        var hotelManager = MagicalGarden.Manager.HotelManager.Instance;
        if (hotelManager == null)
        {
            Debug.LogWarning("[HotelTutorial] TrySpawnTutorialGiftForCurrentStep: HotelManager.Instance is null");
            return false;
        }

        var lastRoom = hotelManager.LastAssignedRoom;
        if (lastRoom == null)
        {
            Debug.LogWarning("[HotelTutorial] TrySpawnTutorialGiftForCurrentStep: LastAssignedRoom is null (belum ada guest yang check-in)");
            return false;
        }

        var gift = lastRoom.SpawnTutorialGift();
        if (gift == null)
            return false;

        Debug.Log($"[HotelTutorial] Tutorial gift spawned at room '{lastRoom.gameObject.name}' for step index={_hotelPanelIndex}.");
        return true;
    }
    private bool TryStartHotelCameraFollowForCurrentStep()
    {
        Debug.Log("=== CHECK HOTEL CAMERA FOLLOW ===");
        if (hotelTutorials == null || hotelTutorials.Count == 0)
        {
            Debug.LogWarning("HotelTutorial ❌ NULL");
            return false;
        }


        if (_hotelPanelIndex < 0 || _hotelPanelIndex >= hotelTutorials.Count)
            return false;

        var currentStep = hotelTutorials[_hotelPanelIndex];
        var config = currentStep != null ? currentStep.config : null;
        if (config == null || !config.focusCameraOnLastCheckedInGuestRoom)
        {
            Debug.LogWarning("Config ❌ NULL");
            Debug.LogWarning("focus camera on last : " + config.focusCameraOnLastCheckedInGuestRoom);
            Debug.LogWarning("config " + config);
            return false;
        }

        var hotelManager = MagicalGarden.Manager.HotelManager.Instance;
        if (hotelManager == null)
        {
            Debug.LogWarning("[HotelTutorial] TryStartHotelCameraFollowForCurrentStep: HotelManager.Instance is null");
            return false;
        }

        var lastRoom = hotelManager.LastAssignedRoom;
        if (lastRoom == null)
        {
            Debug.LogWarning("[HotelTutorial] TryStartHotelCameraFollowForCurrentStep: LastAssignedRoom is null");
            return false;
        }

        if (_hotelMonsterCameraRoutine != null)
        {
            Debug.LogWarning("LastAssignedRoom ❌ NULL");
            StopCoroutine(_hotelMonsterCameraRoutine);
        }

        float duration = config.cameraFocusDuration;
        if (duration <= 0f)
        {
            return false;
        }

        _hotelMonsterCameraRoutine = StartCoroutine(HotelCameraFollowRoomRoutine(lastRoom.transform, duration));
        return true;
    }

    private System.Collections.IEnumerator HotelCameraFollowRoomRoutine(Transform roomTransform, float duration)
    {
        if (roomTransform == null)
        {
            _hotelMonsterCameraRoutine = null;
            yield break;
        }

        if (cameraController == null)
        {
            cameraController = Object.FindObjectOfType<MagicalGarden.Farm.CameraDragMove>();
        }

        if (cameraController != null)
        {
            cameraController.FocusOnTarget(roomTransform.position, 4f, duration, isHotel: true);
        }
        else
        {
            Debug.LogWarning("[HotelTutorial] HotelCameraFollowRoomRoutine: CameraDragMove not found, cannot focus camera");
        }

        if (duration > 0f)
        {
            yield return new WaitForSeconds(duration);
        }
        else
        {
            yield return null;
        }

        _hotelMonsterCameraRoutine = null;

        // Setelah durasi fokus kamera selesai, otomatis lanjut ke step hotel berikutnya,
        // selama kita masih berada di mode tutorial hotel.
        if (_currentMode == TutorialMode.Hotel)
        {
            Debug.Log("[HotelTutorial] Camera focus duration ended, moving to next hotel panel.");
            ShowNextHotelPanel();
        }
    }

    private void LockGuestScrollForTutorial()
    {
        var hotelManager = MagicalGarden.Manager.HotelManager.Instance;
        if (hotelManager != null)
        {
            hotelManager.LockGuestListScroll();
        }
    }

    private void UnlockGuestScrollForTutorial()
    {
        var hotelManager = MagicalGarden.Manager.HotelManager.Instance;
        if (hotelManager != null)
        {
            hotelManager.UnlockGuestListScroll();
        }
    }

    private void PlayHotelPanelShowAnimation(GameObject panel)
    {
        if (panel == null)
            return;

        panel.SetActive(true);

        var rect = panel.GetComponent<RectTransform>();
        var canvasGroup = panel.GetComponent<CanvasGroup>();

        if (rect == null || canvasGroup == null)
            return;

        rect.DOKill();
        canvasGroup.DOKill();

        var targetPos = rect.anchoredPosition;
        var startPos = targetPos;
        startPos.y -= rect.rect.height;
        rect.anchoredPosition = startPos;

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        rect.DOAnchorPos(targetPos, plainPanelShowDuration).SetEase(plainPanelShowEase);
        canvasGroup.DOFade(1f, plainPanelShowDuration).SetEase(Ease.OutQuad);
    }

    private void UpdateHotelStepNextButtonsInteractable()
    {
        if (hotelTutorials == null || hotelTutorials.Count == 0)
            return;

        if (_hotelPanelIndex < 0 || _hotelPanelIndex >= hotelTutorials.Count)
            return;

        var currentStep = hotelTutorials[_hotelPanelIndex];
        var currentConfig = currentStep != null ? currentStep.config : null;
        if (currentStep == null || currentConfig == null)
            return;

        if (currentConfig.useClickableObjectAsNext)
            return;

        var currentBtn = ResolveHotelButtonByName(currentConfig.nextButtonName);
        if (currentBtn == null)
            return;

        currentBtn.interactable = true;
    }

    private void HideCurrentHotelNextButtonForLastAssignedRoomStep()
    {
        if (hotelTutorials == null || hotelTutorials.Count == 0)
            return;

        if (_hotelPanelIndex < 0 || _hotelPanelIndex >= hotelTutorials.Count)
            return;

        var currentStep = hotelTutorials[_hotelPanelIndex];
        var currentConfig = currentStep != null ? currentStep.config : null;
        if (currentStep == null || currentConfig == null)
            return;
        if (currentConfig.useClickableObjectAsNext)
            return;

        var btn = GetHotelStepNextButton(currentStep);
        if (btn == null)
            return;

        btn.interactable = false;
        btn.gameObject.SetActive(false);
    }

    private Button GetHotelStepNextButton(HotelTutorialPanelStep step)
    {
        if (step == null)
            return null;

        var config = step.config;
        if (config == null)
            return null;

        if (step.panelRoot != null)
        {
            var buttons = step.panelRoot.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                var btn = buttons[i];
                if (btn != null && btn.gameObject.name == config.nextButtonName)
                    return btn;
            }
        }

        return GetButtonByName(config.nextButtonName);
    }
    public Button GetButtonByName(string buttonName)
    {
        if (string.IsNullOrEmpty(buttonName))
        {
            Debug.LogWarning("[HandPointerTutorial] GetButtonByName: buttonName is null or empty");
            return null;
        }

        if (_uiButtonsCache == null)
        {
            Debug.LogWarning("[HandPointerTutorial] GetButtonByName: _uiButtonsCache is null");
            return null;
        }

        Debug.Log($"[HandPointerTutorial] GetButtonByName: Searching for '{buttonName}' in cache of {_uiButtonsCache.Length} buttons");

        for (int i = 0; i < _uiButtonsCache.Length; i++)
        {
            var btn = _uiButtonsCache[i];
            if (btn != null && btn.gameObject.name == buttonName)
            {
                Debug.Log($"[HandPointerTutorial] GetButtonByName: Button FOUND at index {i} - name='{btn.name}', active={btn.gameObject.activeSelf}, interactable={btn.interactable}");
                return btn;
            }
        }

        Debug.LogWarning($"[HandPointerTutorial] GetButtonByName: Button '{buttonName}' NOT FOUND in cache of {_uiButtonsCache.Length} buttons");
        return null;
    }

    private void EnsureHotelNextButtonListenerForStep(HotelTutorialPanelStep step)
    {
        if (step == null)
            return;

        var config = step.config;
        if (config == null || string.IsNullOrEmpty(config.nextButtonName))
            return;

        if (config.useClickableObjectAsNext)
            return;

        var btn = GetHotelStepNextButton(step);
        if (btn == null)
        {
            Debug.LogWarning($"[HotelTutorial] EnsureHotelNextButtonListenerForStep: next button '{config.nextButtonName}' NOT FOUND for panel '{step.panelRoot?.name ?? "NULL"}'");
            return;
        }

        btn.onClick.RemoveListener(HandleHotelNextButtonClicked);
        btn.onClick.RemoveListener(ShowNextHotelPanel);
        btn.onClick.AddListener(HandleHotelNextButtonClicked);

        Debug.Log($"[HotelTutorial] Hooked next button '{btn.gameObject.name}' for panel '{step.panelRoot.name}'");
    }

    private void HandleHotelNextButtonClicked()
    {
        Debug.Log($"[HotelTutorial] Next button CLICKED | index={_hotelPanelIndex} | mode={_currentMode}");
        ShowNextHotelPanel();
    }

    private void SetupHotelClickableNextForStep(HotelTutorialPanelStep step)
    {
        if (step == null)
            return;

        var config = step.config;
        if (config == null || !config.useClickableObjectAsNext)
            return;

        var clickable = ResolveHotelClickableObject(config);
        if (clickable == null)
        {
            Debug.LogWarning($"[HotelTutorial] SetupHotelClickableNextForStep: ClickableObject with id '{config.clickableObjectId}' not found.");
            return;
        }

        _currentHotelClickableNextTarget = clickable;
        _currentHotelClickableNextTarget.OnClicked -= HandleHotelClickableNextClicked;
        _currentHotelClickableNextTarget.OnClicked += HandleHotelClickableNextClicked;

        Debug.Log($"[HotelTutorial] Hooked ClickableObject '{clickable.gameObject.name}' (id={config.clickableObjectId}) as NEXT trigger.");
    }

    private void HandleHotelClickableNextClicked(ClickableObject clickable)
    {
        if (_currentMode != TutorialMode.Hotel)
            return;

        if (hotelTutorials == null || hotelTutorials.Count == 0)
            return;

        if (_hotelPanelIndex < 0 || _hotelPanelIndex >= hotelTutorials.Count)
            return;

        var step = hotelTutorials[_hotelPanelIndex];
        var config = step != null ? step.config : null;
        if (config == null || !config.useClickableObjectAsNext)
            return;

        if (string.IsNullOrEmpty(config.clickableObjectId) ||
            !string.Equals(clickable.tutorialId, config.clickableObjectId, System.StringComparison.Ordinal))
            return;

        Debug.Log($"[HotelTutorial] ClickableObject NEXT CLICKED | id={clickable.tutorialId} | index={_hotelPanelIndex}");
        ShowNextHotelPanel();
    }
}
