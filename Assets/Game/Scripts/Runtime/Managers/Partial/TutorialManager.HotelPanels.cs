using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public partial class TutorialManager
{
    private ClickableObject _currentHotelClickableNextTarget;
    private Coroutine _hotelWaitBeforeNextRoutine;
    private MagicalGarden.Hotel.HotelController _hotelMarkedRoomForLoot;
    private GameObject _hotelMarkedLootTarget;

    public void ShowNextHotelPanel()
    {
        Debug.Log($"[HotelTutorial] ShowNextHotelPanel ENTER | index={_hotelPanelIndex} | total={hotelTutorials?.Count ?? 0} | mode={_currentMode}");

        // Hentikan coroutine tunggu sebelumnya jika masih jalan
        if (_hotelWaitBeforeNextRoutine != null)
        {
            StopCoroutine(_hotelWaitBeforeNextRoutine);
            _hotelWaitBeforeNextRoutine = null;
        }

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
            ApplyHotelPanelSortingOverride(nextStep.panelRoot, nextConfig.bringPanelRootToFront);
            PlayHotelPanelShowAnimation(nextStep.panelRoot);
            _hotelStepShownTime = Time.time;
            TrySpawnTutorialGiftForCurrentStep();
            TryApplyHotelGiftBoostForCurrentStep();

            if (nextConfig.waitBeforeNextStep)
            {
                float waitDuration = Mathf.Max(0f, nextConfig.waitBeforeNextDuration);

                if (nextStepButton != null)
                {
                    nextStepButton.interactable = false;
                }

                _hotelWaitBeforeNextRoutine = StartCoroutine(HotelWaitBeforeNextRoutine(waitDuration));
                return;
            }

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
            PlayHotelStepEffectForIndex(_hotelPanelIndex);
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

        if (HotelMainUI.instance != null)
        {
            HotelMainUI.instance.PlayHideForHotelTutorial();
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
            ApplyHotelPanelSortingOverride(firstStep.panelRoot, firstConfig.bringPanelRootToFront);
            PlayHotelPanelShowAnimation(firstStep.panelRoot);
            _hotelStepShownTime = Time.time;

            // Jika step ini mengaktifkan tutorial gift, spawn gift dari kamar terakhir
            // yang dipakai check-in sebelum memulai sub-tutorial/pointer.
            TrySpawnTutorialGiftForCurrentStep();
            TryApplyHotelGiftBoostForCurrentStep();

            if (firstConfig.waitBeforeNextStep)
            {
                float waitDuration = Mathf.Max(0f, firstConfig.waitBeforeNextDuration);
                _hotelWaitBeforeNextRoutine = StartCoroutine(HotelWaitBeforeNextRoutine(waitDuration));
                return;
            }

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

    private void ApplyHotelPanelSortingOverride(GameObject panelRoot, bool bringToFront)
    {
        if (panelRoot == null)
            return;

        var canvas = panelRoot.GetComponent<Canvas>();
        if (!bringToFront)
        {
            if (canvas != null && canvas.overrideSorting && canvas.sortingOrder == 5000)
            {
                canvas.overrideSorting = false;
            }
            return;
        }

        if (canvas == null)
        {
            canvas = panelRoot.AddComponent<Canvas>();
        }

        var rootCanvas = panelRoot.GetComponentInParent<Canvas>();
        if (rootCanvas != null)
        {
            canvas.sortingLayerID = rootCanvas.sortingLayerID;
        }

        canvas.overrideSorting = true;
        canvas.sortingOrder = 5000;
    }

    private System.Collections.IEnumerator HotelWaitBeforeNextRoutine(float duration)
    {
        if (duration > 0f)
        {
            yield return new WaitForSeconds(duration);
        }
        else
        {
            yield return null;
        }

        _hotelWaitBeforeNextRoutine = null;
        ShowNextHotelPanel();
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
        Debug.Log($"[HotelTutorial][Gift] TrySpawnTutorialGiftForCurrentStep ENTER | index={_hotelPanelIndex} | totalSteps={hotelTutorials?.Count ?? 0}");

        if (hotelTutorials == null || hotelTutorials.Count == 0)
        {
            Debug.LogWarning("[HotelTutorial][Gift] ABORT: hotelTutorials null / empty");
            return false;
        }

        if (_hotelPanelIndex < 0 || _hotelPanelIndex >= hotelTutorials.Count)
        {
            Debug.LogWarning($"[HotelTutorial][Gift] ABORT: _hotelPanelIndex out of range ({_hotelPanelIndex})");
            return false;
        }

        var currentStep = hotelTutorials[_hotelPanelIndex];
        var config = currentStep != null ? currentStep.config : null;

        if (config == null)
        {
            Debug.LogWarning("[HotelTutorial][Gift] ABORT: config is null for current hotel step");
            return false;
        }

        if (!config.spawnTutorialGiftFromLastAssignedHotelRoom)
        {
            Debug.Log("[HotelTutorial][Gift] SKIP: spawnTutorialGiftFromLastAssignedHotelRoom is FALSE for this step");
            return false;
        }

        var hotelManager = MagicalGarden.Manager.HotelManager.Instance;
        if (hotelManager == null)
        {
            Debug.LogWarning("[HotelTutorial][Gift] ABORT: HotelManager.Instance is null");
            return false;
        }

        var lastRoom = hotelManager.LastAssignedRoom;
        if (lastRoom == null)
        {
            Debug.LogWarning("[HotelTutorial][Gift] ABORT: LastAssignedRoom is null (belum ada guest yang check-in)");
            return false;
        }

        Debug.Log($"[HotelTutorial][Gift] Attempting SpawnTutorialGift on room '{lastRoom.gameObject.name}' (idHotel={lastRoom.idHotel}, guest='{lastRoom.nameGuest}', isOccupied={lastRoom.IsOccupied})");

        var gift = lastRoom.SpawnTutorialGift();
        if (gift == null)
        {
            Debug.LogWarning("[HotelTutorial][Gift] SpawnTutorialGift returned NULL (HotelGiftSpawner mungkin tidak ada atau gagal spawn gift)");
            return false;
        }

        Debug.Log($"[HotelTutorial][Gift] SUCCESS: Tutorial gift spawned at room '{lastRoom.gameObject.name}' for step index={_hotelPanelIndex}.");
        return true;
    }

    private void TryApplyHotelGiftBoostForCurrentStep()
    {
        if (hotelTutorials == null || hotelTutorials.Count == 0)
            return;

        if (_hotelPanelIndex < 0 || _hotelPanelIndex >= hotelTutorials.Count)
            return;

        var currentStep = hotelTutorials[_hotelPanelIndex];
        var config = currentStep != null ? currentStep.config : null;
        if (config == null)
            return;

        if (!config.accelerateGiftOnClean)
            return;

        float duration = Mathf.Max(0f, config.giftOnCleanBoostDuration);
        if (duration <= 0f)
            return;

        HotelGiftSpawner.EnableTutorialForceGift(duration);
        Debug.Log($"[HotelTutorial][GiftBoost] Enabled gift-on-clean boost for {duration:F1}s on step index={_hotelPanelIndex}.");
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
        if (config == null)
        {
            Debug.LogWarning("[HotelTutorial] TryStartHotelCameraFollowForCurrentStep: config is null");
            return false;
        }

        bool focusRoom = config.focusCameraOnLastCheckedInGuestRoom;
        bool focusGuest = config.focusCameraOnLastCheckedInGuest;
        bool focusShop = config.focusCameraOnHotelShop;
        bool focusLoot = config.focusCameraOnHotelLootFromMarkedRoom;

        if (!focusRoom && !focusGuest && !focusShop && !focusLoot)
        {
            return false;
        }

        MagicalGarden.Hotel.HotelController lastRoom = null;
        if (focusRoom || focusGuest)
        {
            var hotelManager = MagicalGarden.Manager.HotelManager.Instance;
            if (hotelManager == null)
            {
                Debug.LogWarning("[HotelTutorial] TryStartHotelCameraFollowForCurrentStep: HotelManager.Instance is null");
                return false;
            }

            lastRoom = hotelManager.LastAssignedRoom;
            if (lastRoom == null)
            {
                Debug.LogWarning("[HotelTutorial] TryStartHotelCameraFollowForCurrentStep: LastAssignedRoom is null");
                return false;
            }

            // If this step focuses on the guest room, mark it for future loot-focused steps.
            if (focusRoom)
            {
                _hotelMarkedRoomForLoot = lastRoom;
                Debug.Log($"[HotelTutorial] Marked room '{lastRoom.name}' for future loot focus steps.");
            }
        }

        if (_hotelMonsterCameraRoutine != null)
        {
            StopCoroutine(_hotelMonsterCameraRoutine);
        }

        if (focusGuest)
        {
            if (lastRoom.listPet != null && lastRoom.listPet.Count > 0)
            {
                var pet = lastRoom.listPet[lastRoom.listPet.Count - 1];
                if (pet != null)
                {
                    if (_hotelMonsterCameraRoutine != null)
                    {
                        StopCoroutine(_hotelMonsterCameraRoutine);
                    }

                    _hotelCameraFocusCompleted = false;
                    _hotelMonsterCameraRoutine = StartCoroutine(HotelCameraFollowGuestRoutine(lastRoom, pet.transform, config.guestCameraFocusOffset));
                    return true;
                }
            }

            Debug.LogWarning("[HotelTutorial] TryStartHotelCameraFollowForCurrentStep: No pet found in LastAssignedRoom.listPet, cannot focus on guest.");
            return false;
        }
        else if (focusRoom)
        {
            Transform targetTransform = lastRoom.transform;
            float duration = config.cameraFocusDuration;
            Vector2 offset = config.cameraFocusOffset;

            if (duration <= 0f)
            {
                return false;
            }

            _hotelCameraFocusCompleted = false;
            _hotelMonsterCameraRoutine = StartCoroutine(HotelCameraFollowRoomRoutine(targetTransform, duration, offset));
            return true;
        }

        else if (focusShop)
        {
            var shop = HandPointerTargetFinder.FindHotelShopClickable();
            if (shop == null)
            {
                Debug.LogWarning("[HotelTutorial] TryStartHotelCameraFollowForCurrentStep: ClickableShopHotel not found for shop focus");
                return false;
            }

            float duration = config.cameraFocusDuration;
            if (duration <= 0f)
            {
                return false;
            }

            _hotelCameraFocusCompleted = false;
            _hotelMonsterCameraRoutine = StartCoroutine(HotelCameraFollowRoomRoutine(shop.transform, duration, config.hotelShopCameraFocusOffset));
            return true;
        }

        else if (focusLoot)
        {
            var roomForLoot = _hotelMarkedRoomForLoot;

            // Fallback: if nothing marked, use the current LastAssignedRoom
            if (roomForLoot == null)
            {
                var hotelManager = MagicalGarden.Manager.HotelManager.Instance;
                if (hotelManager != null)
                {
                    roomForLoot = hotelManager.LastAssignedRoom;
                }
            }

            if (roomForLoot == null)
            {
                Debug.LogWarning("[HotelTutorial] TryStartHotelCameraFollowForCurrentStep: No room available for loot focus (marked/last assigned room is null)");
                return false;
            }

            var lootTarget = FindClosestHotelLootNearRoom(roomForLoot);
            if (lootTarget == null)
            {
                Debug.LogWarning("[HotelTutorial] TryStartHotelCameraFollowForCurrentStep: No loot decoration found near marked room for loot focus");
                return false;
            }

            _hotelMarkedRoomForLoot = roomForLoot;
            _hotelMarkedLootTarget = lootTarget;

            float duration = config.cameraFocusDuration;
            if (duration <= 0f)
            {
                return false;
            }

            _hotelCameraFocusCompleted = false;
            _hotelMonsterCameraRoutine = StartCoroutine(HotelCameraFollowRoomRoutine(lootTarget.transform, duration, config.hotelLootCameraFocusOffset));
            return true;
        }

        return false;
    }

    private GameObject FindClosestHotelLootNearRoom(MagicalGarden.Hotel.HotelController room)
    {
        if (room == null)
        {
            return null;
        }

        var randomLoot = Object.FindObjectOfType<HotelRandomLoot>(true);
        if (randomLoot == null)
        {
            Debug.LogWarning("[HotelTutorial] FindClosestHotelLootNearRoom: HotelRandomLoot not found in scene");
            return null;
        }

        var lootObjects = randomLoot.GetAllActiveLootDecorations();
        if (lootObjects == null || lootObjects.Count == 0)
        {
            Debug.LogWarning("[HotelTutorial] FindClosestHotelLootNearRoom: No active loot decorations found");
            return null;
        }

        var roomPos = room.transform.position;
        GameObject closest = null;
        float bestSqrDist = float.MaxValue;

        for (int i = 0; i < lootObjects.Count; i++)
        {
            var obj = lootObjects[i];
            if (obj == null || !obj.activeInHierarchy)
                continue;

            float sqrDist = (obj.transform.position - roomPos).sqrMagnitude;
            if (sqrDist < bestSqrDist)
            {
                bestSqrDist = sqrDist;
                closest = obj;
            }
        }

        if (closest != null)
        {
            Debug.Log($"[HotelTutorial] FindClosestHotelLootNearRoom: Found loot '{closest.name}' near room '{room.name}'");
        }

        return closest;
    }

    private System.Collections.IEnumerator HotelCameraFollowGuestRoutine(MagicalGarden.Hotel.HotelController room, Transform petTransform, Vector2 offset)
    {
        if (petTransform == null || room == null)
        {
            _hotelMonsterCameraRoutine = null;
            yield break;
        }

        if (cameraController == null)
        {
            cameraController = Object.FindObjectOfType<MagicalGarden.Farm.CameraDragMove>();
        }

        var cam = Camera.main;
        if (cameraController == null || cam == null)
        {
            Debug.LogWarning("[HotelTutorial] HotelCameraFollowGuestRoutine: CameraDragMove or Camera.main not found, cannot follow guest");
            _hotelMonsterCameraRoutine = null;
            yield break;
        }

        cameraController.LockForTutorial();

        float safetyTimer = 20f;
        while (petTransform != null && room != null && !room.isPetReachedTarget && safetyTimer > 0f)
        {
            safetyTimer -= Time.deltaTime;

            Vector3 targetPos = petTransform.position + (Vector3)offset;

            // Clamp to hotel boundary if available
            var boundary = cameraController.boundaryColliderHotel;
            if (boundary != null)
            {
                Bounds bounds = boundary.bounds;
                float camHeight = cam.orthographicSize;
                float camWidth = camHeight * cam.aspect;

                float minX = bounds.min.x + camWidth;
                float maxX = bounds.max.x - camWidth;
                float minY = bounds.min.y + camHeight;
                float maxY = bounds.max.y - camHeight;

                float clampedX = Mathf.Clamp(targetPos.x, minX, maxX);
                float clampedY = Mathf.Clamp(targetPos.y, minY, maxY);

                targetPos = new Vector3(clampedX, clampedY, cam.transform.position.z);
            }
            else
            {
                targetPos = new Vector3(targetPos.x, targetPos.y, cam.transform.position.z);
            }

            cameraController.transform.position = targetPos;
            yield return null;
        }

        cameraController.LockForTutorial();

        _hotelMonsterCameraRoutine = null;
        _hotelCameraFocusCompleted = true;
        if (cameraController != null)
        {
            cameraController.LockForTutorial();
        }

        if (_currentMode == TutorialMode.Hotel)
        {
            Debug.Log("[HotelTutorial] Guest reached room (or timeout), advancing to NEXT hotel tutorial step.");
            ShowNextHotelPanel();
        }
    }

    private System.Collections.IEnumerator HotelCameraFollowRoomRoutine(Transform targetTransform, float duration, Vector2 offset)
    {
        if (targetTransform == null)
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
            var targetPos = targetTransform.position + (Vector3)offset;
            cameraController.FocusOnTarget(targetPos, 4f, duration, isHotel: true);
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
        _hotelCameraFocusCompleted = true;

        if (_currentMode == TutorialMode.Hotel)
        {
            Debug.Log("[HotelTutorial] Camera focus duration ended, waiting for NEXT button click.");
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

        if (config.minNextClickDelay > 0f)
        {
            btn.interactable = false;
            StartCoroutine(EnableHotelNextButtonAfterDelay(config.minNextClickDelay, _hotelPanelIndex));
        }

        Debug.Log($"[HotelTutorial] Hooked next button '{btn.gameObject.name}' for panel '{step.panelRoot.name}'");
    }

    private System.Collections.IEnumerator EnableHotelNextButtonAfterDelay(float delay, int stepIndex)
    {
        if (delay <= 0f)
            yield break;

        yield return new WaitForSeconds(delay);

        if (_currentMode != TutorialMode.Hotel)
            yield break;

        if (hotelTutorials == null || hotelTutorials.Count == 0)
            yield break;

        if (stepIndex < 0 || stepIndex >= hotelTutorials.Count)
            yield break;

        if (_hotelPanelIndex != stepIndex)
            yield break;

        var step = hotelTutorials[stepIndex];
        var config = step != null ? step.config : null;
        if (step == null || config == null)
            yield break;

        if (config.useClickableObjectAsNext)
            yield break;

        var btn = GetHotelStepNextButton(step);
        if (btn == null)
            yield break;

        btn.interactable = true;
    }

    private void HandleHotelNextButtonClicked()
    {
        Debug.Log($"[HotelTutorial] Next button CLICKED | index={_hotelPanelIndex} | mode={_currentMode}");
        if (hotelTutorials != null &&
            _hotelPanelIndex >= 0 &&
            _hotelPanelIndex < hotelTutorials.Count)
        {
            var currentStep = hotelTutorials[_hotelPanelIndex];
            var config = currentStep != null ? currentStep.config : null;

            if (config != null &&
                (config.focusCameraOnLastCheckedInGuestRoom || config.focusCameraOnLastCheckedInGuest || config.focusCameraOnHotelShop) &&
                !_hotelCameraFocusCompleted)
            {
                Debug.LogWarning("[HotelTutorial] Next button ignored: camera focus duration not finished yet for this step.");
                return;
            }
        }

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
