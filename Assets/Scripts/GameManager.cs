using UnityEngine;
using UnityEngine.UI;

// 생존시간을 재고 화면 좌상단 UI에 표시한다. Game Over 이후에는 시간을 멈춘다.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Tooltip("생존시간을 표시할 Text")]
    public Text survivalTimeText;

    public bool IsGameOver { get; private set; }

    float survivalTime;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (IsGameOver) return;

        survivalTime += Time.deltaTime;
        UpdateText();
    }

    public void GameOver()
    {
        IsGameOver = true;
        UpdateText();
    }

    void UpdateText()
    {
        if (survivalTimeText != null)
            survivalTimeText.text = "Survival Time: " + survivalTime.ToString("F1") + "s";
    }
}
