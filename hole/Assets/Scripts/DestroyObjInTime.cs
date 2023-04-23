using System.Collections;
using UnityEngine;

public class DestroyObjInTime : MonoBehaviour
{
    public float timeToDestroy;

    // Start is called before the first frame update
    void Awake()
    {
        StartCoroutine(DestroyInTime());
    }

    IEnumerator DestroyInTime()
    {
        yield return new WaitForSecondsRealtime(timeToDestroy);
        Destroy(gameObject);
    }
}
