using TMPro;
using UnityEngine;

public class Waves : MonoBehaviour
{
    public TextMeshProUGUI wavesText;

    private int currentWave = 0;
    private int totalWaves = 0;

    void Start()
    {
        UpdateWavesText();
    }

    public void SetWave(int current, int total)
    {
        currentWave = current;
        totalWaves = total;
        UpdateWavesText();
    }

    public void ResetWave()
    {
        currentWave = 0;
        totalWaves = 0;
        UpdateWavesText();
    }

    private void UpdateWavesText()
    {
        if (wavesText != null)
        {
            wavesText.text = $"敵部隊: {currentWave}/{totalWaves}";
        }
        else
        {
            Debug.LogWarning("Waves: wavesText が設定されていません。Inspector で wavesText フィールドに TextMeshProUGUI を割り当ててください。", this);
        }
    }
}
