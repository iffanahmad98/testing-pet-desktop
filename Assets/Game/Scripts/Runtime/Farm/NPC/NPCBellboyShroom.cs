using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MagicalGarden.Hotel;
using MagicalGarden.Manager;
using System;

namespace MagicalGarden.AI
{
    public class NPCBellboyShroom : NPCService, INPCCheckAreaPoisition
    {
        public override void Awake()
        {
            service = this; // 🔥 INI KUNCI UTAMANYA
             Debug.Log ("THIS");
        }

       public override IEnumerator nCheckAreaPosition () {
            if (stateLoopCoroutine != null) {
                StopCoroutine (stateLoopCoroutine);
                stateLoopCoroutine = null;
                isOverridingState = false;
            }

            if (hotelRequestDetector.IsHasHotelRequest () || IsHasHotelReward ()) {
                List<HotelController> listHotelController = hotelRequestDetector.GetListHotelController();
                List<HotelController> listHotelReward = HotelManager.Instance.GetListHotelControllerHasReward ();
                Transform origin = transform; // posisi NPC / player

                float nearestDistance = float.MaxValue;
                Transform nearestTarget = null;
                NearestTargetType nearestTargetType = NearestTargetType.None;

                // Cek Hotel
                foreach (HotelController hotel in listHotelController)
                {
                    if (hotel == null) continue;

                    float dist = Vector3.Distance(origin.position, hotel.transform.position);
                    if (dist < nearestDistance)
                    {
                        nearestDistance = dist;
                        nearestTarget = hotel.transform;
                        nearestTargetType = NearestTargetType.Hotel;
                    }
                }

                foreach (HotelController hotel in listHotelReward)
                {
                    if (hotel == null) continue;

                    float dist = Vector3.Distance(origin.position, hotel.transform.position);
                    if (dist < nearestDistance)
                    {
                        nearestDistance = dist;
                        nearestTarget = hotel.transform;
                        nearestTargetType = NearestTargetType.Reward;
                    }
                }

                if (nearestTargetType == NearestTargetType.Hotel) {
                    HotelController hotelController = nearestTarget.GetComponent <HotelController> ();
                    hotelControlRef = hotelController;
                    isServingRoom = true;
                    // Debug.Log ("Hotel Robo Target Position : " + hotelController.gameObject.name);
                    isOverridingState = true; // di overridingState duluan, karena default dari NPCCleaning ada jeda waktu untuk move target.
                    hotelRequestDetector.RemoveSpecificHotelControllerHasRequest (hotelController);
                    hotelController.NPCAutoService (this);
                    
                } else if (nearestTargetType == NearestTargetType.Reward) {
                    HotelController hotelController = nearestTarget.GetComponent <HotelController> ();
                    hotelControlRef = hotelController;
                    isServingRoom = true;
                    // Debug.Log ("Hotel Robo Target Position : " + hotelController.gameObject.name);
                    isOverridingState = true; // di overridingState duluan, karena default dari NPCCleaning ada jeda waktu untuk move target.
                    HotelManager.Instance.RemoveListHotelControllerHasReward (hotelController);
                    hotelController.NPCAutoClaimReward (this);
                }
            } else {
                StartNewCoroutine (MoveToTarget (currentNPCAreaPointsSO.areaPositions[UnityEngine.Random.Range (0, currentNPCAreaPointsSO.areaPositions.Length)]));
            }
           
          //  isOverridingState = true;
             yield return new WaitUntil (() => !isOverridingState);
            yield return new WaitForSeconds (UnityEngine.Random.Range (checkAreaPositionMinSeconds, checkAreaPositionMaxSeconds));
            StartCoroutine (nCheckAreaPosition ());
        }

        public override IEnumerator nChangeAreaPoints () {
            if (!hotelRequestDetector.IsHasHotelRequest () && !IsHasHotelReward ()) {
                currentNPCAreaPointsSO = npcAreaPointsDatabase.GetRandomNPCAreaPointsSO ();
            // Debug.Log ("Current NPC Area Point : " + currentNPCAreaPointsSO);
            }
            yield return new WaitUntil (() => !isOverridingState);
            yield return new WaitForSeconds (UnityEngine.Random.Range (changeAreaPointsMinSeconds, changeAreaPointsMaxSeconds));
            
            StartCoroutine (nChangeAreaPoints ());
        }

        bool IsHasHotelReward () {
            return HotelManager.Instance.IsHasHotelReward ();
        } 
    }
}
