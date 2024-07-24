using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;
    public TMP_Text scoreText;
    public TMP_Text highScoreText;
    private int score = 0;
    [SerializeField] private int highScore = 0;
    [SerializeField] private int scoreCheck = 0;
    private int scoreThreshold = 10; //inital threshold
    private int zombiePerIteration = 1;
    private ZombieSpawner zombieSpawner;
    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        zombieSpawner = FindObjectOfType<ZombieSpawner>();
        //UpdateScoreText();
    }
    public void AddScore(int amount)
    {
        score += amount;
        scoreCheck += amount;
        UpdateScoreText();
        CheckScoreThreshold();
        UpdateCurrentScoreOnServer();
        CheckAndUpdateHighScore();
    }
    private void UpdateScoreText()
    {
        scoreText.text = "Score: " + score.ToString();
    }
    private void CheckScoreThreshold()
    {
        if (scoreCheck >= scoreThreshold)
        {
            if (zombiePerIteration < zombieSpawner.maxZombiePerSpawn)
            {
                zombiePerIteration++;
                scoreThreshold += 10; // increase threshold for next loop (step 10 20 30 etc)
                scoreCheck = 0;
            }
            Debug.Log($"you have killed all, ready for phase {zombiePerIteration}");
            zombieSpawner.StartNextSpawn(zombiePerIteration);
        }
    }
    public void ResetScore()
    {
        score = 0;
        scoreCheck = 0;
        zombiePerIteration = 1;
        scoreThreshold = 10;
        UpdateScoreText();
        zombieSpawner.ResetSpawner(zombiePerIteration);
        //UpdateCurrentScoreOnServer();
    }
    private void CheckAndUpdateHighScore()
    {
        if (score > highScore)
        {
            highScore = score;
            UpdateHighScoreUI();
            UpdateHighScoreOnServer();
        }
    }
    private void UpdateCurrentScoreOnServer()
    {
        ServerTalker.Instance.PostCurrentScore(score);
    }
    private void UpdateHighScoreOnServer()
    {
        ServerTalker.Instance.PostHighScore(highScore);
    }
    public void SetScores(int current, int high)
    {
        score = current;
        highScore = high;
        UpdateScoreText();
        UpdateHighScoreUI();
        if (scoreCheck != score)
        {
            scoreCheck = score % 10;
        }
    }
    private void UpdateHighScoreUI()
    {
        highScoreText.text = "High score " + highScore.ToString();
    }
}
