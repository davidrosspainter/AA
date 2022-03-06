using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class InformationOverlay : MonoBehaviour
{
    public TextMeshProUGUI FPS_text;

    public TextMeshProUGUI observerIDText;
    public TextMeshProUGUI gameStartTimeText;
    public TextMeshProUGUI gameTimeRemainingText;

    public TextMeshProUGUI levelStartTimeText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI levelTimeRemaining;
    public TextMeshProUGUI trialText;
    public TextMeshProUGUI targetPositionText;

    public TextMeshProUGUI dateTimeNowText;
    public TextMeshProUGUI pausedText;

    public TextMeshProUGUI audioInstructionsText;
    public TextMeshProUGUI affirmationsText;
    public TextMeshProUGUI isShowLevelResultsText;

    GetOrigin getOrigin;

    int frameCount = 0;
    float dt = 0.0f;
    float fps = 0.0f;
    float updateRate = 4.0f;  // 4 updates per sec.

    // Start is called before the first frame update
    void Start()
    {
        getOrigin = GetComponent<GetOrigin>();
    }

    // Update is called once per frame

    void Update()
    {
        frameCount++;
        dt += Time.deltaTime;
        if (dt > 1.0 / updateRate)
        {
            fps = frameCount / dt;
            frameCount = 0;
            dt -= 1.0f / updateRate;
            FPS_text.text = fps.ToString("0.00") + " Hz";
        }
  
        observerIDText.text = "ID: " + CentralMemory.observer.ID;
        gameStartTimeText.text = "gameStartTime: " + GameManager.game.startTime;
        levelStartTimeText.text = "levelStartTime: " + CentralMemory.level.levelStartTime;
        levelText.text = "Current Level: " + (GameManager.currentLevel + 1).ToString() + " of " + (GameManager.game.listLevels.Count).ToString();

        if (getOrigin.enabled)
        {
            float gameTimeRemaining = 0;

            for (int i = GameManager.currentLevel; i < GameManager.game.listLevels.Count; i++)
            {
                gameTimeRemaining += GameManager.game.listLevels[i].timeLimitMinutes;
            }

            gameTimeRemainingText.text = "Game Time Remaining: " + gameTimeRemaining.ToString("0.00") + " minutes";

            levelTimeRemaining.text = "Level Time Remaining: " + CentralMemory.level.timeLimitMinutes.ToString("0.00") + " minutes";
            trialText.text = "";
            targetPositionText.text = "";
        }
        else
        {
            float levelTimeRemaining = CentralMemory.level.timeLimitMinutes - ((Time.time - GameRunner.levelStartTime) / 60);

            if (levelTimeRemaining < 0)
            {
                levelTimeRemaining = 0;
            }

            float gameTimeRemaining = levelTimeRemaining;

            for (int i = GameManager.currentLevel + 1; i < GameManager.game.listLevels.Count; i++)
            {
                gameTimeRemaining += GameManager.game.listLevels[i].timeLimitMinutes;
            }

            gameTimeRemainingText.text = "Game Time Remaining: " + gameTimeRemaining.ToString("0.00") + " minutes";

            this.levelTimeRemaining.text = "Level Time Remaining: " + levelTimeRemaining.ToString("0.00") + " minutes";
            trialText.text = "Trial: " + GameRunner.trial.ToString();
            targetPositionText.text = "Target Position: " + GameRunner.currentTrialData.targetPosition.ToString();
        }

        if (GameRunner.isPaused)
        {
            pausedText.enabled = true;
        }
        else
        {
            pausedText.enabled = false;
        }

        dateTimeNowText.text = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fffffff");

        if (DataStructures.GameOptions.isPlayAudioInstructions)
        {
            audioInstructionsText.text = "I: toggle audio instructions (on)";
        }
        else
        {
            audioInstructionsText.text = "I: toggle audio instructions (off)";
        }


        if (DataStructures.GameOptions.isPlayAffirmations)
        {
            affirmationsText.text = "A: toggle affirmations (on)";
        }
        else
        {
            affirmationsText.text = "A: toggle affirmations (off)";
        }

        if (DataStructures.GameOptions.isShowLevelResults)
        {
            isShowLevelResultsText.text = "R: toggle show level results (on)";
        }
        else
        {
            isShowLevelResultsText.text = "R: toggle show level results (off)";
        }

    }
}
