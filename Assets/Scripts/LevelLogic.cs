using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.SocialPlatforms.Impl;
using System.Collections;
using static UnityEngine.Rendering.DebugUI;
using Button = UnityEngine.UI.Button;

public class LevelLogic : MonoBehaviour
{
    [Header("Knot Used in this Level")]
    public Knot knot;
    public Button nextLevelToUnlock;
    public Sprite keyUp;
    public Sprite keyDown;
    public Sprite keyLeft;
    public Sprite keyRight;

    [Header("Star Requirements")]
    private int highestScore;
    public int starScore;
    public bool finishedLoopQuickly;
    public bool madeMistake;
    public GameObject minigameStar1;
    public GameObject minigameStar2;
    public GameObject minigameStar3;
    public GameObject levelStar1;
    public GameObject levelStar2;
    public GameObject levelStar3;

    [Header("Countdown")]
    public TextMeshProUGUI countdownText;

    [Header("Minigame State Settings")]
    public GameObject gameStateWindow;
    public TextMeshProUGUI gameStateTitle;

    [Header("Timer Settings")]
    bool canStartTimer;
    public Image timer;
    public bool isTimeUp;
    [Tooltip("Time is measured in seconds")]
    public float timeToCompleteKnot = 30f;
    private float currentTime;

    [Header("Audio Settings")]
    public AudioSource audiosoure;
    public AudioClip minigameMistake;
    public AudioClip minigameWin;
    public AudioClip minigameFail;

    [Header("Knot Images")]
    public Image knotImage;
    public List<Sprite> knotImages;
    public Sprite knotDoneImage;
    private int currentKnotImageIndex = 0;

    [Header("Knot Tighten Settings")]
    public GameObject tightenLoopUIHolder;
    public bool readyToTightenLoop;
    public int requiredPullAttempts;
    private int currentPullAttempts;

    [Header("Button Combination")]
    public GameObject buttonsHolder;
    public List<GameObject> buttonsCombination;

    void Update()
    {
        // Timer
        if (canStartTimer)
        {
            if (isTimeUp || currentTime <= 0f)
                return;

            currentTime -= Time.deltaTime;
            timer.fillAmount = currentTime / timeToCompleteKnot;

            if (currentTime <= 0f)
            {
                isTimeUp = true;
                timer.fillAmount = 0f;
                PlayerFailedLevel();
            }
        }

        // Mid-Mini Game
        if (!isTimeUp && !readyToTightenLoop)
        {
            if (buttonsCombination.Count == 0)
                return;

            string inputKey = null;

            if (Input.GetKeyDown(KeyCode.UpArrow))
                inputKey = "up";
            else if (Input.GetKeyDown(KeyCode.DownArrow))
                inputKey = "down";
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
                inputKey = "left";
            else if (Input.GetKeyDown(KeyCode.RightArrow))
                inputKey = "right";

            if (inputKey != null)
                CheckPlayerInput(inputKey);

            // timer logic goes here
        }
        else if (!isTimeUp && readyToTightenLoop && currentPullAttempts != requiredPullAttempts)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                currentPullAttempts = currentPullAttempts + 1;
                CheckIfPlayerWonLevel();
            }
        }
    }

    public void StartTimer()
    {
        isTimeUp = false;
        currentTime = timeToCompleteKnot;
        timer.fillAmount = 1f;
        canStartTimer = true;
    }

    void CheckPlayerInput(string inputKey)
    {
        if (buttonsCombination.Count == 0)
            return;

        GameObject firstButton = buttonsCombination[0];

        RequiredKey keyComponent = firstButton.GetComponent<RequiredKey>();
        if (keyComponent == null)
        {
            Debug.LogWarning("Missing RequiredKey component on button.");
            return;
        }

        if (keyComponent.requiredKey == inputKey)
        {
            // CORRECT INPUT
            firstButton.SetActive(false);
            buttonsCombination.RemoveAt(0);

            // Advance to next sprite if possible
            currentKnotImageIndex++;
            if (currentKnotImageIndex < knotImages.Count)
            {
                knotImage.sprite = knotImages[currentKnotImageIndex];
            }
            else
            {
                // Optional: Set to done image or keep last sprite
                knotImage.sprite = knotDoneImage;
            }

            CheckIfKeyCombinationIsFinished();
        }
        else
        {
            // INCORRECT INPUT
            audiosoure.PlayOneShot(minigameMistake);
            SetAmountOfKeys();
            currentKnotImageIndex = 0;
            knotImage.sprite = knotImages[0];
            if (madeMistake != true) madeMistake = true;
        }
    }

    void CheckIfKeyCombinationIsFinished()
    {
        if (buttonsCombination.Count == 0)
        {
            buttonsHolder.SetActive(false);
            tightenLoopUIHolder.SetActive(true);
            readyToTightenLoop = true;
        }
    }

    void CheckIfPlayerWonLevel()
    {
        if (currentPullAttempts >= requiredPullAttempts)
        {
            isTimeUp = true;

            audiosoure.PlayOneShot(minigameWin);
            currentPullAttempts = requiredPullAttempts;
            knotImage.sprite = knotDoneImage;
            tightenLoopUIHolder.SetActive(false);

            // Check Score
            if (timer.fillAmount >= 0.5) { finishedLoopQuickly = true; }
            // Calculate Score
            if (finishedLoopQuickly && !madeMistake) 
            { 
                starScore = 3;
                // Star Logic
                minigameStar1.transform.localScale = new Vector3( 0, 0, 0);
                minigameStar2.transform.localScale = new Vector3(0, 0, 0);
                minigameStar3.transform.localScale = new Vector3(0, 0, 0);

                Vector3 targetScale = new Vector3(1f, 1f, 1f);
                float duration = 1f;
                float delay = 0.35f;

                // Start the scaling tween with an ease type
                minigameStar1.transform.DOScale(targetScale, duration).SetDelay(delay).SetEase(Ease.OutBounce);
                minigameStar2.transform.DOScale(targetScale, duration).SetDelay(delay * 2).SetEase(Ease.OutBounce);
                minigameStar3.transform.DOScale(targetScale, duration).SetDelay(delay * 3).SetEase(Ease.OutBounce);
            }
            else if ((!finishedLoopQuickly && !madeMistake) || (finishedLoopQuickly && madeMistake)) 
            { 
                starScore = 2;
                // Star Logic
                minigameStar1.transform.localScale = new Vector3(0, 0, 0);
                minigameStar2.transform.localScale = new Vector3(0, 0, 0);
                minigameStar3.transform.localScale = new Vector3(0, 0, 0);

                Vector3 targetScale = new Vector3(1f, 1f, 1f);
                float duration = 1f;
                float delay = 0.35f;

                // Start the scaling tween with an ease type
                minigameStar1.transform.DOScale(targetScale, duration).SetDelay(delay).SetEase(Ease.OutBounce);
                minigameStar2.transform.DOScale(targetScale, duration).SetDelay(delay * 2).SetEase(Ease.OutBounce);
            }
            else if (!finishedLoopQuickly && madeMistake) 
            { 
                starScore = 1;
                // Star Logic
                minigameStar1.transform.localScale = new Vector3(0, 0, 0);
                minigameStar2.transform.localScale = new Vector3(0, 0, 0);
                minigameStar3.transform.localScale = new Vector3(0, 0, 0);

                Vector3 targetScale = new Vector3(1f, 1f, 1f);
                float duration = 1f;
                float delay = 0.35f;

                // Start the scaling tween with an ease type
                minigameStar1.transform.DOScale(targetScale, duration).SetDelay(delay).SetEase(Ease.OutBounce);
            }

            // Check if Player got a higher score than before
            if (starScore >= highestScore)
            {
                highestScore = starScore;

                if (highestScore == 1) 
                { 
                    levelStar1.SetActive(true);
                    levelStar2.SetActive(false);
                    levelStar3.SetActive(false);
                }
                if (highestScore == 2) 
                {
                    levelStar1.SetActive(true);
                    levelStar2.SetActive(true);
                    levelStar3.SetActive(false);
                }
                if (highestScore == 3) 
                {
                    levelStar1.SetActive(true);
                    levelStar2.SetActive(true);
                    levelStar3.SetActive(true);
                }
            }

            // Unlock next node level
            if (nextLevelToUnlock != null) nextLevelToUnlock.interactable = true;

            gameStateWindow.SetActive(true);
            gameStateTitle.text = "Great work! You tied the loop!";
            Debug.Log("YOU WON!");
        }
    }

    void PlayerFailedLevel()
    {
        isTimeUp = true;
        audiosoure.PlayOneShot(minigameFail);
        tightenLoopUIHolder.SetActive(false);
        starScore = 0;

        gameStateWindow.SetActive(true);
        minigameStar1.transform.localScale = new Vector3(0, 0, 0);
        minigameStar2.transform.localScale = new Vector3(0, 0, 0);
        minigameStar3.transform.localScale = new Vector3(0, 0, 0);
        gameStateTitle.text = "Oh no! You ran out of time!";
    }

    public void RestartLevel()
    {
        // restart UI
        knotImage.sprite = knotImages[0];
        SetAmountOfKeys();
        buttonsHolder.SetActive(true);
        tightenLoopUIHolder.SetActive(false);
        gameStateWindow.SetActive(false);
        buttonsHolder.transform.localScale = Vector3.zero;

        // restart pull attempts
        currentPullAttempts = 0;

        // restart time
        timer.fillAmount = 1;
        readyToTightenLoop = false;
        canStartTimer = false;

        // restart score bools
        finishedLoopQuickly = false;
        madeMistake = false;

        StartLevelCountdown();
    }

    public void StartLevelCountdown()
    {
        StartCoroutine(CountdownCoroutine());
    }

    private IEnumerator CountdownCoroutine()
    {
        string[] countdownValues = { "3", "2", "1", "GO!" };

        foreach (string value in countdownValues)
        {
            countdownText.transform.localScale = Vector3.zero;
            countdownText.text = value;

            // Animate the scale with DOTween
            countdownText.transform.localScale = Vector3.zero;
            countdownText.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBounce);

            yield return new WaitForSeconds(1f);

            if (countdownText.text == "1")
            {
                Vector3 targetScale = new Vector3(1f, 1f, 1f);
                float duration = 1f;
                float delay = 0.25f;
                buttonsHolder.transform.DOScale(targetScale, duration).SetDelay(delay).SetEase(Ease.OutQuint);
            }
        }

        countdownText.transform.localScale = Vector3.zero;
        StartTimer();
    }

    void SetAmountOfKeys()
    {
        if (buttonsHolder == null)
        {
            Debug.LogError("buttonsHolder is not assigned");
            return;
        }

        int activateCount = Mathf.Min(knot.knotCombination.Count, buttonsHolder.transform.childCount);

        // Clear and repopulate buttonsCombination
        buttonsCombination.Clear();

        for (int i = 0; i < buttonsHolder.transform.childCount; i++)
        {
            GameObject child = buttonsHolder.transform.GetChild(i).gameObject;
            bool shouldBeActive = i < activateCount;
            child.SetActive(shouldBeActive);

            if (shouldBeActive)
            {
                buttonsCombination.Add(child);

                // Set RequiredKey.requiredKey = knot.knotCombination[i]
                RequiredKey keyComponent = child.GetComponent<RequiredKey>();
                if (keyComponent != null)
                {
                    keyComponent.requiredKey = knot.knotCombination[i];

                    if (knot.knotCombination[i] == "up") { keyComponent.GetComponent<Image>().sprite = keyUp; }
                    else if (knot.knotCombination[i] == "down") { keyComponent.GetComponent<Image>().sprite = keyDown; }
                    else if (knot.knotCombination[i] == "left") { keyComponent.GetComponent<Image>().sprite = keyLeft; }
                    else if (knot.knotCombination[i] == "right") { keyComponent.GetComponent<Image>().sprite = keyRight; }
                }
                else
                {
                    Debug.LogWarning($"Child {child.name} is missing RequiredKey component.");
                }
            }
        }
    }
}
