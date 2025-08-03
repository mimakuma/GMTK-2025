using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;

public class BossLevelLogic : MonoBehaviour
{
    public List<Knot> allKnots;
    public List<Knot> randomlyPickedKnots;

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
    public Image levelMapIcon;
    public Sprite levelStar1;
    public Sprite levelStar2;
    public Sprite levelStar3;

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

    [Header("Fade Settings")]
    public List<Image> keyCovers;
    public float fadeDuration = 1f;
    public Ease fadeEase = Ease.InOutQuad;

    [Header("Audio Settings")]
    public AudioSource audiosoure;
    public AudioClip minigameMistake;
    public AudioClip minigameWin;
    public AudioClip minigameFail;

    [Header("Knot Images")]
    public Image knotImage;
    private int currentKnotImageIndex = 0;

    [Header("Knot Tighten Settings")]
    public GameObject tightenLoopUIHolder;
    public bool readyToTightenLoop;
    public int requiredPullAttempts;
    private int currentPullAttempts;

    [Header("Button Combination")]
    public GameObject buttonsHolder;
    public List<GameObject> buttonsCombination;

    private bool countdownShown = false;

    void FadeOutImages()
    {
        ResetKeyCoversAlpha();

        for (int i = 0; i < keyCovers.Count; i++)
        {
            Image img = keyCovers[i];
            img.DOFade(1f, fadeDuration).SetEase(fadeEase).SetDelay(0f);
        }
    }

    void ResetKeyCoversAlpha()
    {
        foreach (Image img in keyCovers)
        {
            Color c = img.color;
            c.a = 0f;
            img.color = c;
        }
    }

    void PickRandomKnotsFromPool()
    {
        randomlyPickedKnots.Clear();

        if (allKnots.Count < 5)
        {
            Debug.LogWarning("Not enough objects in source list to pick 5 unique ones.");
            return;
        }

        List<Knot> tempList = new List<Knot>(allKnots);

        for (int i = 0; i < 5; i++)
        {
            int index = Random.Range(0, tempList.Count);
            randomlyPickedKnots.Add(tempList[index]);
            tempList.RemoveAt(index);
        }
    }

    void LoadNextKnot()
    {
        if (randomlyPickedKnots.Count == 0)
        {
            FinalizeVictory();
            return;
        }

        knot = randomlyPickedKnots[0];
        randomlyPickedKnots.RemoveAt(0);

        currentKnotImageIndex = 0;
        knotImage.sprite = knot.knotImages[0];

        SetAmountOfKeys();
        buttonsHolder.SetActive(true);
        tightenLoopUIHolder.SetActive(false);
        readyToTightenLoop = false;
        currentPullAttempts = 0;

        // Only freeze timer if we're about to show countdown
        if (!countdownShown)
        {
            canStartTimer = false;
            StartLevelCountdown();
            countdownShown = true;
        }
        else
        {
            // Continue running timer between knots without countdown
            canStartTimer = true;
            FadeOutImages();
        }
    }

    void Update()
    {
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
        }
        else if (!isTimeUp && readyToTightenLoop && currentPullAttempts != requiredPullAttempts)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                currentPullAttempts++;
                CheckIfCurrentKnotComplete();
            }
        }
    }

    public void StartTimer()
    {
        isTimeUp = false;
        currentTime = timeToCompleteKnot;
        //timer.fillAmount = 1f;
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
            firstButton.SetActive(false);
            buttonsCombination.RemoveAt(0);

            currentKnotImageIndex++;
            if (currentKnotImageIndex < knot.knotImages.Count)
            {
                knotImage.sprite = knot.knotImages[currentKnotImageIndex];
            }
            else
            {
                knotImage.sprite = knot.knotDoneImage;
            }

            CheckIfKeyCombinationIsFinished();
        }
        else
        {
            audiosoure.PlayOneShot(minigameMistake);
            SetAmountOfKeys();
            currentKnotImageIndex = 0;
            knotImage.sprite = knot.knotImages[0];
            FadeOutImages();
            if (!madeMistake) madeMistake = true;
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

    void CheckIfCurrentKnotComplete()
    {
        if (currentPullAttempts >= requiredPullAttempts)
        {
            tightenLoopUIHolder.SetActive(false);
            LoadNextKnot();
        }
    }

    void FinalizeVictory()
    {
        isTimeUp = true;
        audiosoure.PlayOneShot(minigameWin);
        if (timer.fillAmount >= 0.5f) finishedLoopQuickly = true;

        if (finishedLoopQuickly && !madeMistake)
        {
            starScore = 3;
            AnimateStars(3);
        }
        else if ((!finishedLoopQuickly && !madeMistake) || (finishedLoopQuickly && madeMistake))
        {
            starScore = 2;
            AnimateStars(2);
        }
        else if (!finishedLoopQuickly && madeMistake)
        {
            starScore = 1;
            AnimateStars(1);
        }

        highestScore = starScore;

        if (highestScore == 1) levelMapIcon.sprite = levelStar1;

        if (highestScore == 2) levelMapIcon.sprite = levelStar2;

        if (highestScore == 3) levelMapIcon.sprite = levelStar3;

        if (nextLevelToUnlock != null) nextLevelToUnlock.interactable = true;

        gameStateWindow.SetActive(true);
        gameStateTitle.text = "Great work! You tied all loops!";
        Debug.Log("YOU WON!");
    }

    void AnimateStars(int count)
    {
        Vector3 targetScale = Vector3.one;
        float duration = 1f;
        float delay = 0.35f;

        minigameStar1.transform.localScale = Vector3.zero;
        minigameStar2.transform.localScale = Vector3.zero;
        minigameStar3.transform.localScale = Vector3.zero;

        if (count >= 1) minigameStar1.transform.DOScale(targetScale, duration).SetDelay(delay).SetEase(Ease.OutBounce);
        if (count >= 2) minigameStar2.transform.DOScale(targetScale, duration).SetDelay(delay * 2).SetEase(Ease.OutBounce);
        if (count == 3) minigameStar3.transform.DOScale(targetScale, duration).SetDelay(delay * 3).SetEase(Ease.OutBounce);
    }

    void PlayerFailedLevel()
    {
        isTimeUp = true;
        audiosoure.PlayOneShot(minigameFail);
        tightenLoopUIHolder.SetActive(false);
        starScore = 0;

        gameStateWindow.SetActive(true);
        minigameStar1.transform.localScale = Vector3.zero;
        minigameStar2.transform.localScale = Vector3.zero;
        minigameStar3.transform.localScale = Vector3.zero;
        gameStateTitle.text = "Oh no! You ran out of time!";
    }

    public void RestartLevel()
    {
        ResetKeyCoversAlpha();
        countdownShown = false;
        PickRandomKnotsFromPool();
        currentTime = timeToCompleteKnot;
        timer.fillAmount = 1;
        isTimeUp = false;
        madeMistake = false;
        finishedLoopQuickly = false;
        gameStateWindow.SetActive(false);
        buttonsHolder.transform.localScale = Vector3.zero;
        canStartTimer = false;
        LoadNextKnot();
        StartLevelCountdown();
    }

    public void StartLevelCountdown()
    {
        if (countdownShown) return;
        countdownShown = true;
        StartCoroutine(CountdownCoroutine());
    }

    private IEnumerator CountdownCoroutine()
    {
        string[] countdownValues = { "3", "2", "1", "GO!" };

        countdownText.gameObject.SetActive(true);
        countdownText.transform.localScale = Vector3.zero;

        foreach (string value in countdownValues)
        {
            countdownText.text = value;
            countdownText.transform.localScale = Vector3.zero;
            countdownText.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
            yield return new WaitForSeconds(1f);

            if (countdownText.text == "1")
            {
                buttonsHolder.transform.DOScale(Vector3.one, 1f).SetDelay(0.25f).SetEase(Ease.OutQuint);
            }
        }

        countdownText.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack);
        yield return new WaitForSeconds(0.25f);

        StartTimer();
        FadeOutImages();
    }

    void SetAmountOfKeys()
    {
        if (buttonsHolder == null)
        {
            Debug.LogError("buttonsHolder is not assigned");
            return;
        }

        int activateCount = Mathf.Min(knot.knotCombination.Count, buttonsHolder.transform.childCount);

        buttonsCombination.Clear();

        for (int i = 0; i < buttonsHolder.transform.childCount; i++)
        {
            GameObject child = buttonsHolder.transform.GetChild(i).gameObject;
            bool shouldBeActive = i < activateCount;
            child.SetActive(shouldBeActive);

            if (shouldBeActive)
            {
                buttonsCombination.Add(child);
                RequiredKey keyComponent = child.GetComponent<RequiredKey>();

                if (keyComponent != null)
                {
                    keyComponent.requiredKey = knot.knotCombination[i];

                    if (knot.knotCombination[i] == "up") keyComponent.GetComponent<Image>().sprite = keyUp;
                    else if (knot.knotCombination[i] == "down") keyComponent.GetComponent<Image>().sprite = keyDown;
                    else if (knot.knotCombination[i] == "left") keyComponent.GetComponent<Image>().sprite = keyLeft;
                    else if (knot.knotCombination[i] == "right") keyComponent.GetComponent<Image>().sprite = keyRight;
                }
                else
                {
                    Debug.LogWarning($"Child {child.name} is missing RequiredKey component.");
                }
            }
        }
    }
}
