using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFolllow : MonoBehaviour
{
    [SerializeField] Transform targetTransform;
    [SerializeField] Vector3 offset;

    float smoothTime = 0.05f;
    Vector3 velocity = Vector3.zero;

    Transform myTransform;
    float peakHeight;

    private void Awake()
    {
        myTransform = transform;
        peakHeight = 0;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Define a target position above and behind the target transform
        Vector3 targetPosition = targetTransform.position + offset;

        if (!GameManager.levelFailed && !GameManager.levelPassed)
        {
            targetPosition = new Vector3(0, Mathf.Max(targetPosition.y, peakHeight), 0);

            peakHeight = myTransform.localPosition.y;

            // Smoothly move the camera towards that target position
            myTransform.localPosition = Vector3.SmoothDamp(myTransform.localPosition, targetPosition, ref velocity, smoothTime);
        }
    }
}
