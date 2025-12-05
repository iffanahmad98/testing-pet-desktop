using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    public float timeDuration;
    void Start () {
        Invoke ("DestroyTime", timeDuration);
    }   

    void DestroyTime () {
        Destroy (this.gameObject);
    }
}
