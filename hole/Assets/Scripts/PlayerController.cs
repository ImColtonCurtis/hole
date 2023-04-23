using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{

    [SerializeField] Transform ballObj, leftHolder, rightHolder, trackTransform
        ,leftStrings, rightStrings;

    [SerializeField] SoundManagerLogic mySoundManager;

    [SerializeField] Rigidbody myRB;

    bool isGRounded, audioStopped;

    [SerializeField] AudioSource ballLoopingSource;

    float pitchAdjuster = 1.8f, volumeAdjuster = 13.5f;

    private void Awake()
    {
        isGRounded = false;
        audioStopped = false;
    }

    private void Update()
    {
        float moveSpeed = 4.8f;
        float deltaLimit = 5f;

        // left movement
        if (ControlsLogic.leftTouch && (leftHolder.transform.position.y - rightHolder.transform.position.y) <= deltaLimit)
        {
            leftHolder.transform.position += new Vector3(0, moveSpeed * Time.fixedDeltaTime, 0);
        }

        // right movement
        if (ControlsLogic.rightTouch && (rightHolder.transform.position.y - leftHolder.transform.position.y) <= deltaLimit)
        {
            rightHolder.transform.position += new Vector3(0, moveSpeed * Time.fixedDeltaTime, 0);
        }

        // ball rolling sound
        if (GameManager.levelStarted && !GameManager.levelFailed && isGRounded && !GameManager.levelPassed && myRB.velocity.magnitude >= 0.65f)
        {
            if (!ballLoopingSource.isPlaying)
                ballLoopingSource.Play();
            ballLoopingSource.pitch = Mathf.Clamp(myRB.velocity.magnitude / pitchAdjuster, 0.95f, 1.35f);
            ballLoopingSource.volume = Mathf.Min(myRB.velocity.magnitude / volumeAdjuster, 0.5f);
            if (audioStopped)
                audioStopped = false;
        }
        else if (!audioStopped)
        {
            StartCoroutine(FadeSound());
            audioStopped = true;
        }       
    }     

    private void LateUpdate()
    {
        float trackDelta = trackTransform.position.x*-1;
                
        // keep track centered at 0
        leftHolder.transform.position = new Vector3(leftHolder.transform.position.x+ trackDelta, leftHolder.transform.position.y, leftHolder.transform.position.z);
        rightHolder.transform.position = new Vector3(rightHolder.transform.position.x+ trackDelta, rightHolder.transform.position.y, rightHolder.transform.position.z);

        // keep strings pointed upward
        float rotPerc; // (-30.79f, 31.968f)
        if (trackTransform.eulerAngles.z < 180)
            rotPerc = trackTransform.eulerAngles.z; // something like 12
        else
            rotPerc = trackTransform.eulerAngles.z - 360f; // something like -12
        rotPerc = (rotPerc + 30.79f) / (30.79f + 31.968f);
        rotPerc = (rotPerc * -1) + 1; // invert perc
        rightStrings.transform.localEulerAngles = new Vector3(rightStrings.transform.localEulerAngles.x, rightStrings.transform.localEulerAngles.y, (rotPerc * 2.73f) - 1.3f); // (-1.3f, 1.43f) -> for right
        leftStrings.transform.localEulerAngles = new Vector3(leftStrings.transform.localEulerAngles.x, leftStrings.transform.localEulerAngles.y, (rotPerc * 2.73f) - 1.43f); // (-1.43f, 1.3f) -> for left

        // ensure ball remains in allowed z locations
        ballObj.transform.localPosition = new Vector3(ballObj.transform.localPosition.x, ballObj.transform.localPosition.y, Mathf.Max(ballObj.transform.localPosition.z, -0.81f));
    }

    IEnumerator FadeSound()
    {
        float timer = 0, totalTime = 7;
        float startVolume = ballLoopingSource.volume;

        while (timer <= totalTime)
        {
            ballLoopingSource.volume = Mathf.Lerp(startVolume, 0, timer / totalTime);
            yield return new WaitForFixedUpdate();
            timer++;

            if (!audioStopped)
                break;
        }
        if (audioStopped)
            ballLoopingSource.Stop();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Lose" && !GameManager.levelFailed && !GameManager.levelPassed)
        {
            mySoundManager.Play("loseJingle"); // losing jingle
            GameManager.levelFailed = true;
        }

        if (other.tag == "Win" && !GameManager.levelPassed)
        {
            mySoundManager.Play("winJingle"); // wining jingle sound
            GameManager.levelPassed = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Track")
        {
            if (!isGRounded)
                isGRounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Track")
        {
            if (isGRounded)
                isGRounded = false;
        }
    }
}
