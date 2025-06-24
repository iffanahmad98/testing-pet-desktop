using UnityEngine;
using System.Collections.Generic;
using MagicalGarden.Manager;
using System;
using UnityEngine.Tilemaps;
using MagicalGarden.Farm;

namespace MagicalGarden.Hotel
{
    public class HotelRoom : MonoBehaviour
    {
        [Header("Room Info")]
        public string roomId;
        public GuestController guest;
        public Vector3Int hotelPosition;
        public int offsetDistance = 3;
        public List<Vector3Int> wanderingTiles = new List<Vector3Int>();

        [Header("Guest Requests")]
        public List<HotelRoomRequest> roomRequests = new();

        public bool IsOccupied;

        public void AssignGuest(GuestController newGuest)
        {
            guest = newGuest;
            roomRequests.Clear();
            int multiplier = 1;
            if (guest.rarity == GuestRarity.Rare || guest.rarity == GuestRarity.Mythic || guest.rarity == GuestRarity.Legend)
            {
                multiplier = 2;
            }

            int totalRequest = Mathf.Clamp(guest.stayDurationDays * multiplier, 1, 6);
            for (int i = 0; i < totalRequest; i++)
            {
                var type = (GuestRequestType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(GuestRequestType)).Length);
                AddRequest(new HotelRoomRequest(type, guest.stayDurationDays));
            }
            guest.checkInDate = DateTime.Now.Date;

            HotelManager.Instance.SaveGuestRequests();
        }
        public void AssignGuestLoad(GuestController newGuest)
        {
            guest = newGuest;
            roomRequests.Clear();
            int totalRequest = Mathf.Clamp(guest.stayDurationDays, 1, 3); // max 3
            for (int i = 0; i < totalRequest; i++)
            {
                var type = (GuestRequestType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(GuestRequestType)).Length);
                AddRequest(new HotelRoomRequest(type, guest.stayDurationDays));
            }
        }

        public void AddRequest(HotelRoomRequest request)
        {
            roomRequests.Add(request);
        }

        public void FulfillRequest(GuestRequestType type)
        {
            var request = roomRequests.Find(r => r.requestType == type && !r.isFulfilled);
            if (request != null)
            {
                request.isFulfilled = true;
                guest.happiness = Mathf.Min(guest.happiness + 20, 100);
                roomRequests.Remove(request);

                Debug.Log($"✅ Permintaan {type} dipenuhi di {roomId} untuk {guest.guestName}");
            }
        }
        public void SetHotelTileDirty(bool isDirty)
        {
            var tilemap = TileManager.Instance.tilemapHotel;
            if (tilemap == null) return;
            var dirtyTile = HotelManager.Instance.dirtyTile;
            var cleanTile = HotelManager.Instance.cleanTile;
            tilemap.SetTile(hotelPosition, isDirty ? dirtyTile : cleanTile);
        }
        public Vector2Int? GetRandomWanderingTile2D()
        {
            if (wanderingTiles == null || wanderingTiles.Count == 0)
                return null;

            var tile = wanderingTiles[UnityEngine.Random.Range(0, wanderingTiles.Count)];
            return new Vector2Int(tile.x, tile.y);
        }
        public void CheckOut()
        {
            if (guest == null) return;

            if (guest.happiness >= 50)
            {
                Debug.Log($"💰 {guest.guestName} membayar karena puas!");
                // Tambahkan coin atau sistem pembayaran
            }
            else
            {
                Debug.Log($"😡 {guest.guestName} kecewa dan tidak membayar.");
            }

            guest = null;
            roomRequests.Clear();
        }
        public void CalculateWanderingArea()
        {
            Tilemap mapHotel = TileManager.Instance.tilemapHotel;
            if (mapHotel == null)
            {
                Debug.LogWarning("❌ Tilemap Hotel belum di-set!");
                return;
            }
            TileBase tile = mapHotel.GetTile(hotelPosition);
            HotelTile hotelTile = tile as HotelTile;
            if (hotelTile == null)
            {
                Debug.LogWarning($"❌ Tile di {hotelPosition} bukan HotelTile!");
                return;
            }
            AreaOffsetDirection offsetDirection = hotelTile.offsetDirection;
            Vector3Int globalOffset = Vector3Int.zero;
            wanderingTiles.Clear();

            switch (offsetDirection)
            {
                case AreaOffsetDirection.Right:
                    globalOffset = new Vector3Int(offsetDistance, 0, 0);
                    break;
                case AreaOffsetDirection.Left:
                    globalOffset = new Vector3Int(-offsetDistance, 0, 0);
                    break;
                case AreaOffsetDirection.Up:
                    globalOffset = new Vector3Int(0, offsetDistance, 0);
                    break;
                case AreaOffsetDirection.Down:
                    globalOffset = new Vector3Int(0, -offsetDistance, 0);
                    break;
            }

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    Vector3Int localOffset = new Vector3Int(x, y, 0);
                    Vector3Int pos = hotelPosition + globalOffset + localOffset;

                    if (TileManager.Instance.tilemapSoil.HasTile(pos))
                    {
                        wanderingTiles.Add(pos);
                    }
                }
            }
        }
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0, 1, 0, 0.5f);
            if (TileManager.Instance.tilemapSoil == null || wanderingTiles == null) return;

            foreach (var pos in wanderingTiles)
            {
                Vector3 worldPos = TileManager.Instance.tilemapSoil.GetCellCenterWorld(pos);
                Gizmos.DrawCube(worldPos, Vector3.one * 0.9f);
            }
        }
    }
    
}