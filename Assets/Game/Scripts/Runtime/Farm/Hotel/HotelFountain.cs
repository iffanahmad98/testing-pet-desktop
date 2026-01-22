using UnityEngine;

public class HotelFountain : MonoBehaviour
{
    [SerializeField] private float volume = 0.3f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PlayFountainSound();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayFountainSound()
    {
        // Hotel fountain is at index 17
        MonsterManager.instance.audio.PlayFarmSFX(17, volume, true);
    }
}
