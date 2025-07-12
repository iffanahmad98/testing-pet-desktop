using UnityEngine;

namespace MagicalGarden.Farm
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;
        public CameraDragMove cameraRig;
        private void Awake()
        {
            Instance = this;
        }

        public void DisableCameraRig()
        {
            cameraRig.canDrag = false;
            cameraRig.canZoom = false;
        }
        public void EnableCameraRig()
        {
            cameraRig.canDrag = true;
            cameraRig.canZoom = true;
        }

        public bool HasEnoughPetsInInventory(int requiredCount)
        {
            //change to get count pet
            return true;
        }
    }
}
