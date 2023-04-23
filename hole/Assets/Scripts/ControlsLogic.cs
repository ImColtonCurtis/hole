using System.Collections;
using Unity.Services.Mediation.Samples;
using UnityEngine;

public class ControlsLogic : MonoBehaviour
{
    public static bool leftTouch, rightTouch;

    [SerializeField] GameObject noIcon;

    [SerializeField] Animator soundAnim;

    [SerializeField] SoundManagerLogic mySoundManager;

    [SerializeField] AudioSource conveyerSound;

    bool fadingOut, startSound;

    int cheatCounter;

    void Awake()
    {
        leftTouch = false;
        rightTouch = false;

        if (PlayerPrefs.GetInt("SoundStatus", 1) == 1)
        {
            noIcon.SetActive(false);
            AudioListener.volume = 1;
        }
        else
        {
            noIcon.SetActive(true);
            AudioListener.volume = 0;
        }
        cheatCounter = 0;

        fadingOut = false;
        startSound = false;
    }

    private void Update()
    {
        if ((rightTouch || leftTouch) && !conveyerSound.isPlaying)
        {
            conveyerSound.Play();
            conveyerSound.volume = 0.25f;
            startSound = true;
        }
        if (conveyerSound.isPlaying && !leftTouch && !rightTouch && !fadingOut)
        {
            StartCoroutine(FadeSound(conveyerSound));
            fadingOut = true;
            startSound = false;
        }

    }

    IEnumerator FadeSound(AudioSource mySound)
    {
        float timer = 0, totalTime = 7;
        float startVolume = mySound.volume;

        while (timer <= totalTime)
        {
            mySound.volume = Mathf.Lerp(startVolume, 0, timer / totalTime);
            yield return new WaitForFixedUpdate();
            timer++;

            if (startSound)
                break;
        }
        if (!startSound)
            mySound.Stop();
        fadingOut = false;
    }

    void OnTouchDown(Vector3 point)
    {
        if (GameManager.levelPassed)
            return;

        if (!GameManager.inLoading)
        {
            if (ShowAds.poppedUp)
            {
                if (point.x <= 0)
                    ShowAds.shouldShowRewardedAd = true;
                else
                    ShowAds.dontShow = true;
            }
            else if (ShowAds.skipPoppedUp)
            {
                if (point.x <= 0)
                    ShowAds.shouldShowRewardedAd = true;
                else
                    ShowAds.dontShow = true;
            }
            else
            {
                // cheat: top-right, top-right, top-left, bottom-right
                // top right tap
                if (!GameManager.levelStarted && (cheatCounter == 0 || cheatCounter == 1) && point.x >= 0.03f && point.y >= -35.65f)
                {
                    cheatCounter++;
                }
                // top left tap
                else if (!GameManager.levelStarted && (cheatCounter == 2) && point.x <= -0.03f && point.y >= -35.65f)
                {
                    cheatCounter++;
                }
                // bottom right tap
                else if (!GameManager.levelStarted && (cheatCounter == 3) && point.x >= 0.03f && point.y <= -35.79f)
                {
                    cheatCounter = 0;
                    if (!GameManager.cheatOn)
                        GameManager.cheatOn = true;
                    else
                        GameManager.cheatOn = false;
                }

                else if (!GameManager.levelStarted && point.x <= -0.02f && point.y <= -35.79f) // bottom left button clicked
                {
                    if (PlayerPrefs.GetInt("SoundStatus", 1) == 1)
                    {
                        PlayerPrefs.SetInt("SoundStatus", 0);
                        noIcon.SetActive(true);
                        AudioListener.volume = 0;
                    }
                    else
                    {
                        PlayerPrefs.SetInt("SoundStatus", 1);
                        noIcon.SetActive(false);
                        AudioListener.volume = 1;
                    }
                    soundAnim.SetTrigger("Blob");
                }
                else
                {
                    if (!GameManager.levelFailed && !GameManager.levelStarted)
                        GameManager.levelStarted = true;

                    if (!GameManager.levelPassed && !GameManager.levelFailed)
                    {
                        if (point.x <= 0) // left
                        {
                            leftTouch = true;
                            rightTouch = false;
                        }
                        else if (point.x > 0)  // right
                        {
                            rightTouch = true;
                            leftTouch = false;
                        }
                    }
                }
            }

            if (GameManager.levelFailed && !GameManager.isRestarting)
            {
                GameManager.isRestarting = true;
                GameManager.shouldRestart = true;
            }
        }
    }

    void OnTouchUp()
    {
        leftTouch = false;
        rightTouch = false;
    }

    void OnTouchExit()
    {
        leftTouch = false;
        rightTouch = false;
    }
}
