using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using MagicalGarden.Manager;
using MagicalGarden.AI;

namespace MagicalGarden.Hotel
{
    public class GuestItem : MonoBehaviour
    {
        [Header("UI References")]
        public Image guestIcon;
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI descPartyText;
        public TextMeshProUGUI descPriceText;
        public TextMeshProUGUI descTimeText;
        public GuestRequest guest;

        [Header("Type")]
        public GameObject vipObject;
        public GameObject regObject;
        public GameObject checkInBtn;
        public GameObject confirmBtn;
        public GameObject declineBtn;

        public string GuestType => guest?.type;
        public string GuestName => guest?.guestName;

        public void Setup(GuestRequest guest)
        {
            Debug.Log($"[GuestItem] Setup called for guest '{guest.guestName}' - StackTrace: {UnityEngine.StackTraceUtility.ExtractStackTrace()}");

            this.guest = guest;
            guestIcon.sprite = guest.icon;
            titleText.text = guest.guestName;
            descPartyText.text = guest.party.ToString();
            descPriceText.text = guest.price.ToString();
            descTimeText.text = guest.GetStayDurationString();

            var checkInButton = checkInBtn.GetComponent<Button>();
            var confirmButton = confirmBtn.GetComponent<Button>();
            var declineButton = declineBtn.GetComponent<Button>();

            // Check if buttons are protected by tutorial system
            bool checkInProtected = TutorialManager.IsButtonProtectedByTutorial(checkInButton);
            bool confirmProtected = TutorialManager.IsButtonProtectedByTutorial(confirmButton);
            bool declineProtected = TutorialManager.IsButtonProtectedByTutorial(declineButton);

            Debug.Log($"[GuestItem] Setup: Button protection status - checkIn={checkInProtected}, confirm={confirmProtected}, decline={declineProtected} | checkInBtnID={checkInButton.GetInstanceID()}");

            // Only modify listeners if not protected by tutorial
            if (!checkInProtected)
            {
                checkInButton.onClick.RemoveAllListeners();
                checkInButton.onClick.AddListener(() =>
                {
                    HandleGuestCheckIn();
                });
            }
            else
            {
                Debug.Log("[GuestItem] Setup: checkInBtn is protected by tutorial, skipping listener setup");
            }

            if (!confirmProtected)
            {
                confirmButton.onClick.RemoveAllListeners();
                confirmButton.onClick.AddListener(() =>
                {
                    HandleGuestCheckIn();
                });
            }
            else
            {
                Debug.Log("[GuestItem] Setup: confirmBtn is protected by tutorial, skipping listener setup");
            }

            if (!declineProtected)
            {
                declineButton.onClick.RemoveAllListeners();
                declineButton.onClick.AddListener(() =>
                {
                    HandleGuestDecline();
                });
            }
            else
            {
                Debug.Log("[GuestItem] Setup: declineBtn is protected by tutorial, skipping listener setup");
            }
            if (guest.IsVIPGuest())
            {
                checkInBtn.SetActive(false);
                confirmBtn.SetActive(true);
                vipObject.SetActive(true);
                regObject.SetActive(false);
            }
            else
            {
                checkInBtn.SetActive(true);
                confirmBtn.SetActive(false);
                vipObject.SetActive(false);
                regObject.SetActive(true);
            }
        }


        private void HandleGuestCheckIn()
        {
            Debug.Log($"[GuestItem] HandleGuestCheckIn invoked for guest '{guest?.guestName}'");

            var hotelManager = HotelManager.Instance;
            if (hotelManager == null)
            {
                Debug.LogError("[GuestItem] HandleGuestCheckIn aborted: HotelManager.Instance is null");
                return;
            }

            if (hotelManager.IsCanAssign())
            {
                Debug.Log("[GuestItem] IsCanAssign == true, assigning guest to room");

                // Confirm guest is at index 15
                MonsterManager.instance.audio.PlayFarmSFX(15);
                hotelManager.AssignGuestToAvailableRoom(guest);
                Debug.Log("[GuestItem] Guest assigned, destroying GuestItem gameObject");
                Destroy(gameObject);
                var tutorialManager = UnityEngine.Object.FindObjectOfType<TutorialManager>();
                if (tutorialManager != null)
                {
                    tutorialManager.NotifyGuestItemCheckInClicked();
                }
            }
            else
            {
                Debug.LogWarning("[GuestItem] IsCanAssign == false, cannot assign guest now");
            }
        }

        private void HandleGuestDecline()
        {
            // Decline guest ias at index 16
            MonsterManager.instance.audio.PlayFarmSFX(16);
            HotelManager.Instance.DeclineGuest(guest);
            Destroy(gameObject);
        }
    }
}
