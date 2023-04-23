using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static bool levelStarted, levelFailed, levelPassed, shouldRestart, isRestarting, cheatOn, reloadLevel, inLoading;

    [SerializeField] Transform levelObjectsFolder, layoutTransform;

    [SerializeField] Camera myCam;

    [SerializeField] TextMeshPro levelScoreTMP;
    [SerializeField] SpriteRenderer pressText, pressBG, dropText, dropBGIMG, fullSquare, retryText, retryBG;
    [SerializeField] GameObject passedObj, fillObj, holeObj, finishLineObj;

    bool restartFadedIn, startedLevel, shouldPass, fasterLoad;

    [SerializeField] Animator cameraShakeAnim;

    GameObject tempObj;
    Vector3 spawnLocation = new Vector3(-2, -3, -0.265f);
    float fillZ = -0.265f;
    float holeZ = 0.2399999f;

    [SerializeField] Rigidbody myRB;
    [SerializeField] GameObject confettiObj;

    [SerializeField] SpriteRenderer[] soundIcons;

    [SerializeField] GameObject catObj, pigObj;

    // Sounds: ControlsLogic.cs, PlayerController.cs

    [SerializeField] AudioSource mainMenuMusic;

    [SerializeField] Material plankMat, wallMat;

    private void Awake()
    {
        Application.targetFrameRate = 60;

        StartCoroutine(FadeImageOut(fullSquare, 30));

        restartFadedIn = false;
        shouldRestart = false;
        levelStarted = false;
        levelFailed = false;
        startedLevel = false;
        shouldPass = false;
        levelPassed = false;
        passedObj.SetActive(false);
        isRestarting = false;

        fasterLoad = false;
        reloadLevel = false;
        inLoading = false;

        cheatOn = false;

        if (PlayerPrefs.GetInt("eggiesEnabled", 0) == 0) // is off
        {
            catObj.SetActive(false);
            pigObj.SetActive(true);
            cheatOn = false;
        }
        else if (PlayerPrefs.GetInt("eggiesEnabled", 0) == 1) // is on
        {
            catObj.SetActive(true);
            pigObj.SetActive(false);
            cheatOn = true;
        }

        int adder = PlayerPrefs.GetInt("levelScore", 1);

        levelScoreTMP.text = "Level " + adder;

        if (PlayerPrefs.GetInt("NewLevel", 1) == 1)
        {
            // change mat colors
            if (PlayerPrefs.GetInt("levelScore", 1) % 5 == 0)
            {
                PlayerPrefs.SetInt("ColorTracker", PlayerPrefs.GetInt("ColorTracker", 0)+1);

                // plankMat, wallMat;
                Color plankColor = new Color(0.6117647f, 0.5464052f, 0.2196078f, 1),
                    wallColor = new Color(0.4077743f, 0.6402845f, 0.772f, 1);

                switch (PlayerPrefs.GetInt("ColorTracker", 0)%6)
                {
                    case 0:
                        plankColor = new Color(0.6117647f, 0.5464052f, 0.2196078f, 1);
                        wallColor = new Color(0.4077743f, 0.6402845f, 0.772f, 1);
                        break;
                    case 1:
                        plankColor = new Color(0.2196078f, 0.2849672f, 0.6117647f, 1);
                        wallColor = new Color(0.772549f, 0.5415686f, 0.4078431f, 1);
                        break;
                    case 2:
                        plankColor = new Color(0.2849673f, 0.6117647f, 0.2196078f, 1);
                        wallColor = new Color(0.5415686f, 0.4078431f, 0.772549f, 1);
                        break;
                    case 3:
                        plankColor = new Color(0.5464053f, 0.2196078f, 0.6117647f, 1);
                        wallColor = new Color(0.6388235f, 0.772549f, 0.4078431f, 1);
                        break;
                    case 4:
                        plankColor = new Color(0.5464053f, 0.2196078f, 0.6117647f, 1);
                        wallColor = new Color(0.772549f, 0.5415686f, 0.4078431f, 1);
                        break;
                    case 5:
                        plankColor = new Color(0.6117647f, 0.2196078f, 0.2849674f, 1);
                        wallColor = new Color(0.6388235f, 0.772549f, 0.4078431f, 1);
                        break;
                    default:
                        plankColor = new Color(0.6117647f, 0.5464052f, 0.2196078f, 1);
                        wallColor = new Color(0.4077743f, 0.6402845f, 0.772f, 1);
                        break;
                }
                plankMat.color = plankColor;
                wallMat.color = wallColor;
            }

            SpawnNewLevel();
            PlayerPrefs.SetInt("NewLevel", 0);
        }
        else
        {
            RespawnNewLevel();
        }

        myRB.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezePositionX;
    }

    IEnumerator FadeOutAudio(AudioSource myAudio)
    {
        float timer = 0, totalTime = 24;
        float startingLevel = myAudio.volume;
        while (timer <= totalTime)
        {
            myAudio.volume = Mathf.Lerp(startingLevel, 0, timer / totalTime);
            yield return new WaitForFixedUpdate();
            timer++;
        }
    }

    private void Update()
    {
        if (reloadLevel)
        {
            fasterLoad = true;
            inLoading = true;
            StartCoroutine(RestartLevel(fullSquare));
            reloadLevel = false;
        }

        if (cheatOn && PlayerPrefs.GetInt("eggiesEnabled", 0) == 0) // turn on
        {
            catObj.SetActive(true);
            pigObj.SetActive(false);
            PlayerPrefs.SetInt("eggiesEnabled", 1);
        }
        else if (!cheatOn && PlayerPrefs.GetInt("eggiesEnabled", 0) == 1) // turn off
        {
            catObj.SetActive(false);
            pigObj.SetActive(true);
            PlayerPrefs.SetInt("eggiesEnabled", 0);
        }

        if (levelStarted && !startedLevel)
        {
            foreach (SpriteRenderer sprite in soundIcons)
            {
                StartCoroutine(FadeImageOut(sprite, 24));
            }

            myRB.constraints = RigidbodyConstraints.None;

            StartCoroutine(FadeOutAudio(mainMenuMusic));

            StartCoroutine(FadeImageOut(pressText, 24));
            StartCoroutine(FadeImageOut(pressBG, 24));
            StartCoroutine(FadeImageOut(dropText, 24));
            StartCoroutine(FadeImageOut(dropBGIMG, 24));
            startedLevel = true;
        }

        if (levelFailed && !restartFadedIn)
        {
            StartCoroutine(RestartWait());
            restartFadedIn = true;
        }

        if (levelFailed && shouldRestart)
        {
            PlayerPrefs.SetInt("FailedInARow", PlayerPrefs.GetInt("FailedInARow", 0) + 1); //
            StartCoroutine(RestartLevel(fullSquare));
            shouldRestart = false;
        }

        if (levelPassed && !shouldPass)
        {
            PlayerPrefs.SetInt("FailedInARow", 0); //
            confettiObj.SetActive(true);
            myRB.AddForce(new Vector3(0, 60, 0), ForceMode.Impulse);
            StartCoroutine(PassLogic());
            shouldPass = true;         
        }
    }

    void SpawnNewLevel()
    {        
        int obstacleCount = 0;

        // determine finish line height
        float finishHeight = Random.Range(0, Mathf.Min(5, PlayerPrefs.GetInt("levelScore", 1)));
        int maxObstacleCount = (int)Mathf.Min((finishHeight+1)*2, PlayerPrefs.GetInt("levelScore", 1) * 1.5f);
        PlayerPrefs.SetInt("FinishHeight", (int)finishHeight);
        float heightY = 3 + finishHeight;

        finishHeight *= 2;
        finishHeight += 0.5f;
        finishLineObj.transform.localPosition = new Vector3(0, finishHeight, -0.27f);

        for (int i = 1; i < heightY; i++) // height
        {
            int obsPerRow = 0, maxPerRow = 1;
            if (PlayerPrefs.GetInt("levelScore", 1) >= 5 && PlayerPrefs.GetInt("levelScore", 1) < 15)
                maxPerRow = 2;
            else if (Random.Range(0, 3) == 0)
                maxPerRow = 3;
            else
                maxPerRow = 2;

            for (int j = 0; j < 3; j++) // width 
            {
                int obstacleType = 0;
                if (obstacleCount < maxObstacleCount)
                {
                    if (Random.Range(0, 2) == 0 || obsPerRow >= maxPerRow) // spawn fill
                    {
                        tempObj = Instantiate(fillObj, spawnLocation, Quaternion.Euler(-90, 0, 0), layoutTransform);
                        tempObj.transform.localPosition = new Vector3(spawnLocation.x, spawnLocation.y, fillZ);
                    }
                    else // spawn hole
                    {
                        tempObj = Instantiate(holeObj, spawnLocation, Quaternion.Euler(180, 0, 270), layoutTransform);
                        tempObj.transform.localPosition = new Vector3(spawnLocation.x, spawnLocation.y, holeZ);
                        obstacleType = 1;
                        maxObstacleCount++;
                        obsPerRow++;
                    }
                }
                else
                {
                    tempObj = Instantiate(fillObj, spawnLocation, Quaternion.Euler(-90, 0, 0), layoutTransform);
                    tempObj.transform.localPosition = new Vector3(spawnLocation.x, spawnLocation.y, fillZ);
                }

                // store obstacle
                PlayerPrefs.SetInt("ObstacleInt" + PlayerPrefs.GetInt("ObstacleCount", 0), obstacleType); // 0: fill, 1: hole
                PlayerPrefs.SetInt("ObstacleCount", PlayerPrefs.GetInt("ObstacleCount", 0) + 1); // increment obstacle count

                // update spawn width location
                if (j < 2)
                    spawnLocation += new Vector3(2, 0, 0);
                else
                    spawnLocation -= new Vector3(4, 0, 0);
            }
            // update spawn height location
            spawnLocation += new Vector3(0, 2, 0);
        }
        // spawn final height
        for (int k = 0; k < 3; k++)
        {
            tempObj = Instantiate(fillObj, spawnLocation, Quaternion.Euler(-90, 0, 0), layoutTransform);
            tempObj.transform.localPosition = new Vector3(spawnLocation.x, spawnLocation.y, fillZ);
            spawnLocation += new Vector3(2, 0, 0);
        }   
    }

    void RespawnNewLevel()
    {
        // determine finish line height
        float finishHeight = PlayerPrefs.GetInt("FinishHeight", 1);
        float heightY = 3 + finishHeight;

        finishHeight *= 2;
        finishHeight += 0.5f;
        finishLineObj.transform.localPosition = new Vector3(0, finishHeight, -0.27f);
        int obstaclesSpawned = 0;

        for (int i = 1; i < heightY; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                int obstacleToSpawn = PlayerPrefs.GetInt("ObstacleInt" + obstaclesSpawned, 0);
                obstaclesSpawned++;

                if (obstacleToSpawn == 0) // spawn fill
                {
                    tempObj = Instantiate(fillObj, spawnLocation, Quaternion.Euler(-90, 0, 0), layoutTransform);
                    tempObj.transform.localPosition = new Vector3(spawnLocation.x, spawnLocation.y, fillZ);
                }
                else // spawn hole
                {
                    tempObj = Instantiate(holeObj, spawnLocation, Quaternion.Euler(180, 0, 270), layoutTransform);
                    tempObj.transform.localPosition = new Vector3(spawnLocation.x, spawnLocation.y, holeZ);
                }

                // update spawn width location
                if (j < 2)
                    spawnLocation += new Vector3(2, 0, 0);
                else
                    spawnLocation -= new Vector3(4, 0, 0);
            }
            // update spawn height location
            spawnLocation += new Vector3(0, 2, 0);
        }
        // spawn final height
        for (int k = 0; k < 3; k++)
        {
            tempObj = Instantiate(fillObj, spawnLocation, Quaternion.Euler(-90, 0, 0), layoutTransform);
            tempObj.transform.localPosition = new Vector3(spawnLocation.x, spawnLocation.y, fillZ);
            spawnLocation += new Vector3(2, 0, 0);
        }
    }

    IEnumerator PassLogic()
    {
        passedObj.SetActive(true);
        PlayerPrefs.SetInt("NewLevel", 1);
        PlayerPrefs.SetInt("ObstacleCount", 0);
        PlayerPrefs.SetInt("levelScore", PlayerPrefs.GetInt("levelScore", 1) + 1);
        yield return new WaitForSeconds(1.4f);
        StartCoroutine(NextLevel(fullSquare));
    }

    IEnumerator NextLevel(SpriteRenderer myImage)
    {
        float timer = 0, totalTime = 24;
        Color startingColor = myImage.color;
        myImage.enabled = true;
        while (timer <= totalTime)
        {
            myImage.color = Color.Lerp(new Color(startingColor.r, startingColor.g, startingColor.b, 0), new Color(startingColor.r, startingColor.g, startingColor.b, 1), timer / totalTime);
            yield return new WaitForFixedUpdate();
            timer++;
        }
        yield return new WaitForSeconds(0.1f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
    }

    IEnumerator ShakeCamera()
    {
        yield return new WaitForSeconds(7f/60f);
        cameraShakeAnim.SetTrigger("shake");
    }

    IEnumerator RestartWait()
    {
        yield return new WaitForSeconds(0.1f);
        StartCoroutine(FadeImageIn(retryText, 48));
        StartCoroutine(FadeImageIn(retryBG, 47));
    }

    IEnumerator RestartLevel(SpriteRenderer myImage)
    {
        float timer = 0, totalTime = 24;
        Color startingColor = myImage.color;
        myImage.enabled = true;
        while (timer <= totalTime)
        {
            myImage.color = Color.Lerp(new Color(startingColor.r, startingColor.g, startingColor.b, 0), new Color(startingColor.r, startingColor.g, startingColor.b, 1), timer / totalTime);
            yield return new WaitForFixedUpdate();
            timer++;
        }
        if (fasterLoad)
        {
            fasterLoad = false;
            yield return new WaitForSecondsRealtime(0.7f);
            inLoading = false;
        }
        else
            yield return new WaitForSecondsRealtime(0.3f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
    }

    IEnumerator FadeImageOut(SpriteRenderer myImage, float totalTime)
    {
        float timer = 0;
        Color startingColor = myImage.color;
        myImage.enabled = true;
        while (timer <= totalTime)
        {
            myImage.color = Color.Lerp(new Color(startingColor.r, startingColor.g, startingColor.b, 1), new Color(startingColor.r, startingColor.g, startingColor.b, 0), timer / totalTime);
            yield return new WaitForFixedUpdate();
            timer++;
        }
        myImage.enabled = false;
    }

    IEnumerator FadeImageIn(SpriteRenderer myImage, float totalTime)
    {
        float timer = 0;
        Color startingColor = myImage.color;
        myImage.enabled = true;
        while (timer <= totalTime)
        {
            myImage.color = Color.Lerp(new Color(startingColor.r, startingColor.g, startingColor.b, 0), new Color(startingColor.r, startingColor.g, startingColor.b, 1), timer / totalTime);
            yield return new WaitForFixedUpdate();
            timer++;
        }
    }

    IEnumerator FadeTextOut(TextMeshPro myTtext)
    {
        float timer = 0, totalTime = 24;
        Color startingColor = myTtext.color;
        while (timer <= totalTime)
        {
            myTtext.color = Color.Lerp(new Color(startingColor.r, startingColor.g, startingColor.b, 1), new Color(startingColor.r, startingColor.g, startingColor.b, 0), timer / totalTime);
            yield return new WaitForFixedUpdate();
            timer++;
        }
    }

    IEnumerator FadeTextIn(TextMeshPro myTtext)
    {
        float timer = 0, totalTime = 24;
        Color startingColor = myTtext.color;
        while (timer <= totalTime)
        {
            myTtext.color = Color.Lerp(new Color(startingColor.r, startingColor.g, startingColor.b, 0), new Color(startingColor.r, startingColor.g, startingColor.b, 1), timer / totalTime);
            yield return new WaitForFixedUpdate();
            timer++;
        }
    }
}
