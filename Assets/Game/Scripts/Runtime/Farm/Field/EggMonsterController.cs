using UnityEngine;
using System.Collections;
using MagicalGarden.Manager;

namespace MagicalGarden.Farm
{
    public class EggMonsterController : MonoBehaviour
    {
        [Header("Reference Object")]
        public GameObject vfxShinePrefab;
        public GameObject menu;
        public GameObject monsterGatcha;

        [Header("Setting Zoom")]
        public float zoomInSize = 3f;
        public float zoomDuration = 1f;
        public float hatchDelay = 1.2f;
        public float cameraReturnDelay = 2f;
        private bool isHatching = false;
        public void SendMonsterToLand()
        {
            TileManager.Instance.disableTileSelect = false;
            UIManager.Instance.ToggleMenuBar();
            GameManager.Instance.EnableCameraRig();
            Destroy(monsterGatcha);
            Destroy(gameObject);
        }
        public void SellMonster()
        {
            TileManager.Instance.disableTileSelect = false;
            CoinManager.Instance.AddCoins(200);
            GameManager.Instance.EnableCameraRig();
            UIManager.Instance.ToggleMenuBar();
            Destroy(monsterGatcha);
            Destroy(gameObject);
        }
    }
}