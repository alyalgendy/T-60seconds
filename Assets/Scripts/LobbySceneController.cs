using UnityEngine;
using UnityEngine.UI;

public class LobbySceneController : MonoBehaviour
{
    [SerializeField] Text readinessText;
    [SerializeField] Text readyButtonText;

    bool isReady;

    public void Configure(Text readiness, Text readyLabel)
    {
        readinessText = readiness;
        readyButtonText = readyLabel;
    }

    public void ToggleReady()
    {
        isReady = !isReady;
        readinessText.text = isReady ? "READY" : "NOT READY";
        readyButtonText.text = isReady ? "CANCEL READY" : "READY UP";
        readinessText.color = isReady ? new Color(0.35f, 0.82f, 0.52f, 1f) : Color.white;
    }

    public void LeaveLobby()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
