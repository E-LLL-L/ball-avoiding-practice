using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 생존시간을 재고 화면 좌상단 UI에 표시한다. Game Over 이후에는 시간을 멈추고 결과 화면을 보여준다.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Tooltip("생존시간을 표시할 Text")]
    public Text survivalTimeText;

    [Header("Game Over 화면")]
    [Tooltip("Game Over 시 켜질 패널")]
    public GameObject gameOverPanel;

    [Tooltip("버틴 시간을 표시할 Text")]
    public Text resultTimeText;

    public bool IsGameOver { get; private set; }

    public float SurvivalTime { get; private set; }

    void Awake()
    {
        Instance = this;
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    void Update()
    {
        if (IsGameOver) return;

        SurvivalTime += Time.deltaTime;
        UpdateText();
    }

    public void GameOver()
    {
        if (IsGameOver) return;

        IsGameOver = true;
        UpdateText();
        ObstacleSphere.FreezeAll();

        if (resultTimeText != null)
            resultTimeText.text = "You survived " + SurvivalTime.ToString("F1") + " seconds";
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    // 재도전 버튼에서 호출
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void UpdateText()
    {
        if (survivalTimeText != null)
            survivalTimeText.text = "Survival Time: " + SurvivalTime.ToString("F1") + "s";
    }
}
