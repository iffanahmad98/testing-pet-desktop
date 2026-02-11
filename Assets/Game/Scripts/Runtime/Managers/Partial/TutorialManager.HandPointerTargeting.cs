using UnityEngine;
using UnityEngine.UI;
using MagicalGarden.Hotel;

public partial class TutorialManager
{
    private class HandPointerTargetingContext
    {
        public ITutorialPointer Pointer { get; private set; }
        public Button CurrentButton { get; set; }
        public RectTransform CurrentRect { get; set; }
        public ClickableObject CurrentClickable { get; set; }

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
                CurrentButton = null;
            }

            CurrentRect = null;

            if (CurrentClickable != null)
            {
                CurrentClickable = null;
            }
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
            button.gameObject.SetActive(true);
            button.interactable = true;

            if (Context.CurrentButton != null)
            {
                Context.CurrentButton.onClick.RemoveListener(Manager.OnHandPointerTargetClicked);
            }

            Context.CurrentButton = button;
            Context.CurrentButton.onClick.AddListener(Manager.OnHandPointerTargetClicked);

            Context.CurrentRect = rect;
            Context.CurrentClickable = null;
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
            var guestItem = HandPointerTargetFinder.FindGuestItem(step.guestNameFilter, step.guestTypeFilter);
            if (guestItem == null)
            {
                Debug.LogWarning($"[HotelTutorial] HandPointerSub: GuestItem dengan guestName='{step.guestNameFilter}' tidak ditemukan.");
                return false;
            }

            var checkInBtn = guestItem.checkInBtn?.GetComponent<Button>();
            if (checkInBtn == null)
            {
                Debug.LogWarning($"[HotelTutorial] HandPointerSub: checkInBtn tidak ditemukan di GuestItem '{step.guestNameFilter}'.");
                return false;
            }

            var guestRect = checkInBtn.transform as RectTransform;
            if (guestRect == null)
                return false;

            SetupButtonTarget(checkInBtn, guestRect, onClickCallback);

            if (Context.Pointer != null)
            {
                Debug.Log($"[HotelTutorial] HandPointerSub: PointTo GuestItem checkInBtn untuk guest '{step.guestNameFilter}' dengan offset {step.pointerOffset}.");
                Context.Pointer.PointTo(guestRect, step.pointerOffset);
            }

            return true;
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
                return false;

            var targetButton = _buttonResolver.Resolve(Manager, step);
            if (targetButton == null)
                return false;

            var rect = targetButton.transform as RectTransform;
            if (rect == null)
                return false;

            SetupButtonTarget(targetButton, rect, onClickCallback);

            if (Context.Pointer != null)
            {
                Context.Pointer.PointTo(rect, step.pointerOffset);
            }

            return true;
        }
    }
}
