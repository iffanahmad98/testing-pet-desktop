using UnityEngine;
using UnityEngine.UI;
using MagicalGarden.Hotel;
using MagicalGarden.Manager;

public partial class TutorialManager
{
    private class HandPointerTargetingContext
    {
        public ITutorialPointer Pointer { get; private set; }
        public Button CurrentButton { get; set; }
        public RectTransform CurrentRect { get; set; }
        public ClickableObject CurrentClickable { get; set; }
        public System.Func<(Button, RectTransform)> TargetResolver { get; set; }
        public System.Action PostClickAction { get; set; }
        public System.Collections.Generic.List<ClickableObject> DisabledClickableObjects { get; set; }
        public System.Collections.Generic.List<Collider> DisabledColliders { get; set; }
        public System.Collections.Generic.List<Collider2D> DisabledColliders2D { get; set; }

        public HandPointerTargetingContext()
        {
            InitializePointer();
        }

        private void InitializePointer()
        {
            Pointer = ServiceLocator.Get<ITutorialPointer>();
            if (Pointer == null)
            {
                var fallbackPointer = Object.FindObjectOfType<TutorialHandPointer>(true);
                if (fallbackPointer != null)
                {
                    Debug.Log("[HotelTutorial] HandPointerSub: ITutorialPointer tidak ada di ServiceLocator, menggunakan fallback TutorialHandPointer dari scene.");
                    Pointer = fallbackPointer;
                }
                else
                {
                    Debug.LogWarning("[HotelTutorial] HandPointerSub: ITutorialPointer tidak ditemukan, tidak bisa menampilkan hand pointer.");
                }
            }
        }

        public void ClearCurrentTargets()
        {
            if (CurrentButton != null)
            {
                // Remove from protected buttons list
                TutorialManager.RemoveProtectedButton(CurrentButton);
                CurrentButton = null;
            }

            CurrentRect = null;

            if (CurrentClickable != null)
            {
                CurrentClickable = null;
            }

            // Re-enable disabled ClickableObjects
            if (DisabledClickableObjects != null && DisabledClickableObjects.Count > 0)
            {
                Debug.Log($"[ClearCurrentTargets] Re-enabling {DisabledClickableObjects.Count} ClickableObjects");
                foreach (var clickable in DisabledClickableObjects)
                {
                    if (clickable != null)
                    {
                        clickable.enabled = true;
                    }
                }
                DisabledClickableObjects.Clear();
            }

            // Re-enable disabled Colliders
            if (DisabledColliders != null && DisabledColliders.Count > 0)
            {
                Debug.Log($"[ClearCurrentTargets] Re-enabling {DisabledColliders.Count} Colliders");
                foreach (var collider in DisabledColliders)
                {
                    if (collider != null)
                    {
                        collider.enabled = true;
                    }
                }
                DisabledColliders.Clear();
            }

            // Re-enable disabled Collider2Ds
            if (DisabledColliders2D != null && DisabledColliders2D.Count > 0)
            {
                Debug.Log($"[ClearCurrentTargets] Re-enabling {DisabledColliders2D.Count} Collider2Ds");
                foreach (var collider2D in DisabledColliders2D)
                {
                    if (collider2D != null)
                    {
                        collider2D.enabled = true;
                    }
                }
                DisabledColliders2D.Clear();
            }

            TargetResolver = null;
            PostClickAction = null;
        }

        public void HidePointer()
        {
            Pointer?.Hide();
        }
    }

    private abstract class HandPointerTargetHandler
    {
        protected TutorialManager Manager { get; }
        protected HandPointerTargetingContext Context { get; }

        protected HandPointerTargetHandler(TutorialManager manager, HandPointerTargetingContext context)
        {
            Manager = manager;
            Context = context;
        }

        public abstract bool CanHandle(HandPointerSubStep step);
        public abstract bool Apply(HandPointerSubStep step, System.Action onClickCallback);

        protected void SetupClickableTarget(ClickableObject clickable, System.Action onClickCallback)
        {
            if (Context.CurrentClickable != null)
            {
                Context.CurrentClickable.OnClicked -= Manager.OnHandPointerClickableTargetClicked;
            }

            Context.CurrentClickable = clickable;
            Context.CurrentClickable.OnClicked += Manager.OnHandPointerClickableTargetClicked;

            Context.CurrentButton = null;
            Context.CurrentRect = null;
        }

        protected void SetupButtonTarget(Button button, RectTransform rect, System.Action onClickCallback)
        {
            Debug.Log($"[SetupButtonTarget] Setting up button target - button='{button.name}', rect='{rect.name}'");
            // Generic case: tutorial fully owns the button behaviour
            button.gameObject.SetActive(true);
            button.interactable = true;

            if (Context.CurrentButton != null && Context.CurrentButton == button)
            {
                Context.CurrentButton.onClick.RemoveListener(Manager.OnHandPointerTargetClicked);
            }

            Context.CurrentButton = button;

            // Disable components that might interfere with button clicks
            DisableClickInterference(button);

            // Setup button listeners with enhanced protection (clear existing)
            ConfigureButtonListeners(button);

            Context.CurrentRect = rect;
            Context.CurrentClickable = null;

            ValidateButtonSetup(button);
        }
        protected void SetupButtonTargetKeepExisting(Button button, RectTransform rect)
        {
            Debug.Log($"[SetupButtonTargetKeepExisting] Setting up button target - button='{button.name}', rect='{rect.name}'");
            button.gameObject.SetActive(true);
            button.interactable = true;

            if (Context.CurrentButton != null && Context.CurrentButton == button)
            {
                Debug.Log($"[SetupButtonTargetKeepExisting] Button '{button.name}' is already the current target, skipping listener reconfiguration");
                Context.CurrentButton.onClick.RemoveListener(Manager.OnHandPointerTargetClicked);
            }

            Context.CurrentButton = button;
            DisableClickInterference(button);

            // Mark as protected so GuestItem.Setup atau script lain tidak mengubah listener check-in
            TutorialManager.AddProtectedButton(button, Manager.OnHandPointerTargetClicked);

            Context.CurrentRect = rect;
            Context.CurrentClickable = null;

            ValidateButtonSetup(button);
            Debug.Log($"[SetupButtonTargetKeepExisting] Button '{button.name}' setup complete with existing listeners preserved.");
        }

        private void DisableClickInterference(Button button)
        {
            var clickableObj = button.GetComponent<ClickableObject>();
            if (clickableObj != null)
            {
                Debug.LogWarning($"[DisableClickInterference] Disabling ClickableObject on '{button.name}'");
                DisableClickableObject(clickableObj);
            }

            var parentClickable = button.GetComponentInParent<ClickableObject>();
            if (parentClickable != null && parentClickable.gameObject != button.gameObject)
            {
                Debug.LogWarning($"[DisableClickInterference] Disabling parent ClickableObject on '{parentClickable.name}'");
                DisableClickableObject(parentClickable);
            }
        }

        private void DisableClickableObject(ClickableObject clickable)
        {
            clickable.enabled = false;

            Context.DisabledClickableObjects = Context.DisabledClickableObjects ?? new System.Collections.Generic.List<ClickableObject>();
            if (!Context.DisabledClickableObjects.Contains(clickable))
            {
                Context.DisabledClickableObjects.Add(clickable);
            }

            DisableCollidersOnObject(clickable.gameObject);
        }

        private void DisableCollidersOnObject(GameObject obj)
        {
            var collider = obj.GetComponent<Collider>();
            if (collider != null && collider.enabled)
            {
                collider.enabled = false;
                Context.DisabledColliders = Context.DisabledColliders ?? new System.Collections.Generic.List<Collider>();
                Context.DisabledColliders.Add(collider);
            }

            var collider2D = obj.GetComponent<Collider2D>();
            if (collider2D != null && collider2D.enabled)
            {
                collider2D.enabled = false;
                Context.DisabledColliders2D = Context.DisabledColliders2D ?? new System.Collections.Generic.List<Collider2D>();
                Context.DisabledColliders2D.Add(collider2D);
            }
        }

        private void ConfigureButtonListeners(Button button)
        {
            button.onClick.RemoveAllListeners();

            // Add tutorial listener
            var tutorialListener = new UnityEngine.Events.UnityAction(Manager.OnHandPointerTargetClicked);
            button.onClick.AddListener(tutorialListener);

            // Add to enhanced protection system with automatic restoration
            TutorialManager.AddProtectedButton(button, Manager.OnHandPointerTargetClicked);
        }

        private void ValidateButtonSetup(Button button)
        {
            var eventSystem = UnityEngine.EventSystems.EventSystem.current;
            if (eventSystem == null)
            {
                Debug.LogError("[ValidateButtonSetup] No EventSystem found! Button clicks will not work.");
                return;
            }

            var canvasGroup = button.GetComponent<UnityEngine.CanvasGroup>();
            if (canvasGroup != null && (!canvasGroup.blocksRaycasts || !canvasGroup.interactable))
            {
                Debug.LogWarning($"[ValidateButtonSetup] CanvasGroup on '{button.name}' may block clicks!");
            }
            Debug.Log($"[ValidateButtonSetup] Button '{button.name}' is set up for tutorial interaction.");
        }

    }
    private class HotelRoomTargetHandler : HandPointerTargetHandler
    {
        public HotelRoomTargetHandler(TutorialManager manager, HandPointerTargetingContext context)
            : base(manager, context) { }

        public override bool CanHandle(HandPointerSubStep step) => step.useHotelRoomTarget;

        public override bool Apply(HandPointerSubStep step, System.Action onClickCallback)
        {
            var hotelRoom = HandPointerTargetFinder.FindRandomOccupiedHotelRoom(step.hotelRoomGuestTypeFilter);
            if (hotelRoom == null)
            {
                Debug.LogWarning($"[HotelTutorial] HandPointerSub: Tidak ada HotelController occupied dengan filter type='{step.hotelRoomGuestTypeFilter}'.");
                return false;
            }

            var clickable = hotelRoom.GetComponent<ClickableObject>();
            if (clickable != null)
            {
                SetupClickableTarget(clickable, onClickCallback);
            }
            else
            {
                Debug.LogWarning($"[HotelTutorial] HandPointerSub: HotelController '{hotelRoom.gameObject.name}' tidak memiliki ClickableObject component.");
            }

            if (Context.Pointer != null)
            {
                Debug.Log($"[HotelTutorial] HandPointerSub: PointToWorld ke HotelController '{hotelRoom.gameObject.name}' (guest={hotelRoom.nameGuest}, type={hotelRoom.typeGuest}) dengan offset {step.pointerOffset}.");
                Context.Pointer.PointToWorld(hotelRoom.transform, step.pointerOffset);
            }

            return true;
        }
    }
    private class GuestItemCheckInTargetHandler : HandPointerTargetHandler
    {
        public GuestItemCheckInTargetHandler(TutorialManager manager, HandPointerTargetingContext context)
            : base(manager, context) { }

        public override bool CanHandle(HandPointerSubStep step) => step.useGuestItemCheckInButton;

        public override bool Apply(HandPointerSubStep step, System.Action onClickCallback)
        {
            Context.TargetResolver = () => ResolveGuestItemCheckInButton();

            var resolved = Context.TargetResolver();
            if (resolved.Item1 == null || resolved.Item2 == null)
            {
                Debug.LogWarning("[GuestItemCheckIn] Failed to resolve GuestItem checkInBtn");
                return false;
            }
            SetupButtonTargetKeepExisting(resolved.Item1, resolved.Item2);

            if (Context.Pointer != null)
            {
                var pointer = Context.Pointer as TutorialHandPointer;
                if (pointer != null)
                {
                    pointer.PointTo(resolved.Item2, step.pointerOffset, () => ResolveGuestItemCheckInButton().Item2);
                }
                else
                {
                    Context.Pointer.PointTo(resolved.Item2, step.pointerOffset);
                }
            }
            else
            {
                Debug.LogWarning("[GuestItemCheckIn] Context.Pointer is null, cannot show hand pointer");
                return false;
            }

            return true;
        }

        private (Button, RectTransform) ResolveGuestItemCheckInButton()
        {
            var guestItem = HandPointerTargetFinder.FindGuestItem();
            if (guestItem == null)
            {
                Debug.LogWarning("[HotelTutorial] ResolveGuestItemCheckInButton: GuestItem is null");
                return (null, null);
            }

            if (guestItem.checkInBtn == null)
            {
                Debug.LogWarning("[HotelTutorial] ResolveGuestItemCheckInButton: checkInBtn GameObject is null");
                return (null, null);
            }

            guestItem.checkInBtn.SetActive(true);
            Debug.Log($"[HotelTutorial] ResolveGuestItemCheckInButton: checkInBtn SetActive, active={guestItem.checkInBtn.activeInHierarchy}");

            var checkInBtn = guestItem.checkInBtn.GetComponent<Button>();
            if (checkInBtn == null)
            {
                Debug.LogWarning("[HotelTutorial] ResolveGuestItemCheckInButton: Button component is null");
                return (null, null);
            }

            var rect = checkInBtn.GetComponent<RectTransform>();
            if (rect == null)
            {
                return (null, null);
            }

            return (checkInBtn, rect);
        }
    }
    private class ClickableObjectTargetHandler : HandPointerTargetHandler
    {
        public ClickableObjectTargetHandler(TutorialManager manager, HandPointerTargetingContext context)
            : base(manager, context) { }

        public override bool CanHandle(HandPointerSubStep step) => step.useClickableObjectTarget;

        public override bool Apply(HandPointerSubStep step, System.Action onClickCallback)
        {
            var clickable = HandPointerTargetFinder.FindClickableObjectById(step.clickableObjectId);
            if (clickable == null)
            {
                Debug.LogWarning($"[HotelTutorial] HandPointerSub: ClickableObject dengan id='{step.clickableObjectId}' tidak ditemukan.");
                return false;
            }

            SetupClickableTarget(clickable, onClickCallback);

            if (Context.Pointer != null)
            {
                Debug.Log($"[HotelTutorial] HandPointerSub: PointToWorld ke ClickableObject '{clickable.gameObject.name}' (id={step.clickableObjectId}) dengan offset {step.pointerOffset}.");
                Context.Pointer.PointToWorld(clickable.transform, step.pointerOffset);
            }

            return true;
        }
    }
    private class UIButtonTargetHandler : HandPointerTargetHandler
    {
        private IUIButtonResolver _buttonResolver;

        public UIButtonTargetHandler(TutorialManager manager, HandPointerTargetingContext context, IUIButtonResolver buttonResolver)
            : base(manager, context)
        {
            _buttonResolver = buttonResolver;
        }

        public override bool CanHandle(HandPointerSubStep step) => true; // Default fallback

        public override bool Apply(HandPointerSubStep step, System.Action onClickCallback)
        {
            if (_buttonResolver == null)
            {
                Debug.LogWarning("[HandPointerTutorial] UIButtonTargetHandler: buttonResolver is null");
                return false;
            }

            string buttonIdentifier = !string.IsNullOrEmpty(step.ButtonKey) ? $"ButtonKey='{step.ButtonKey}'" : $"uiButtonIndex={step.uiButtonIndex}";
            Debug.Log($"[HandPointerTutorial] UIButtonTargetHandler: Attempting to resolve button with {buttonIdentifier}");

            var targetButton = _buttonResolver.Resolve(Manager, step);
            if (targetButton == null)
            {
                Debug.LogWarning($"[HandPointerTutorial] UIButtonTargetHandler: Failed to resolve button with {buttonIdentifier}");
                return false;
            }

            Debug.Log($"[HandPointerTutorial] UIButtonTargetHandler: Button resolved successfully - name='{targetButton.name}', {buttonIdentifier}");

            var rect = targetButton.transform as RectTransform;
            if (rect == null)
            {
                Debug.LogWarning($"[HandPointerTutorial] UIButtonTargetHandler: RectTransform not found for button '{targetButton.name}'");
                return false;
            }

            SetupButtonTarget(targetButton, rect, onClickCallback);

            if (Context.Pointer != null)
            {
                Debug.Log($"[HandPointerTutorial] UIButtonTargetHandler: Pointing to button '{targetButton.name}' with offset {step.pointerOffset}");
                Context.Pointer.PointTo(rect, step.pointerOffset);
            }
            else
            {
                Debug.LogWarning("[HandPointerTutorial] UIButtonTargetHandler: Context.Pointer is null");
            }

            return true;
        }
    }
}
