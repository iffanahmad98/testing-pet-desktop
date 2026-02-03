using UnityEngine;
using MagicalGarden.Farm;

[RequireComponent(typeof(Collider2D))]
public class BridgeTeleport : MonoBehaviour
{
    public enum TeleportTarget
    {
        ToFarm,
        ToHotel
    }

    public TeleportTarget targetArea;
    public CameraDragMove camMove;
    public GameObject menuBarFarm;

    [Header("Spawn Points")]
    public Transform farmSpawnPoint;
    public Transform hotelSpawnPoint;

    [Header("Camera Settings")]
    public float transitionDuration = 1f;
    public float targetZoom = 5f;

    [Header("Hover Effect")]
    public float hoverScaleMultiplier = 1.1f;
    public float scaleSpeed = 5f;

    private Vector3 originalScale;
    private bool isHovered = false;

    [SerializeField] private ChickenAI[] animals;
    [SerializeField] private HotelFountain[] hotelFountains;

    private void Start()
    {
        originalScale = transform.localScale;
    }

    private void Update()
    {
        Vector3 targetScale = isHovered ? originalScale * hoverScaleMultiplier : originalScale;
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
    }

    private void OnMouseEnter()
    {
        isHovered = true;
    }

    private void OnMouseExit()
    {
        isHovered = false;
    }

    private void OnMouseDown()
    {
        
        if (camMove == null)
        {
            Debug.LogWarning("❌ CameraDragMove not found.");
            return;
        }

        switch (targetArea)
        {
            case TeleportTarget.ToHotel:
                if (hotelSpawnPoint != null)
                {
                    foreach (var t in animals) t.StopAnimalSoundCoroutine();
                    
                    MonsterManager.instance.audio.PlayHotelAmbiance();
                    foreach (var f in hotelFountains) f.PlayFountainSound();

                    // farm game intro SFX is at index 0
                    MonsterManager.instance.audio.PlayFarmSFX(0);

                    camMove.FocusOnTarget(hotelSpawnPoint.position, targetZoom, transitionDuration, isHotel: true);
                    FarmMainUI.instance.Hide ();
                    HotelMainUI.instance.Show ();
                    menuBarFarm.SetActive(false);
                    if (CursorIconManager.Instance) {CursorIconManager.Instance.HideSeedIcon ();}
                }
                break;

            case TeleportTarget.ToFarm:
                if (farmSpawnPoint != null)
                {
                    foreach (var t in animals) t.StartAnimalSoundCoroutine();
                    MonsterManager.instance.audio.playFarmAmbiance();
                    // farm game intro SFX is at index 0
                    MonsterManager.instance.audio.PlayFarmSFX(0);

                    camMove.FocusOnTarget(farmSpawnPoint.position, targetZoom, transitionDuration, isHotel: false);
                    FarmMainUI.instance.Show ();
                    HotelMainUI.instance.Hide ();
                    menuBarFarm.SetActive(true);
                }
                break;
        }
    }
}
