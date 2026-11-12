using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectionMenuController : MonoBehaviour
{
    [System.Serializable]
    public class MenuOption
    {
        public string title;
        [TextArea(2, 4)] public string description;
    }

    [SerializeField] MenuOption[] options;
    [SerializeField] GameObject[] optionObjects;
    [SerializeField] Image[] rowBackgrounds;
    [SerializeField] Image[] selectionBars;
    [SerializeField] Text detailTitle;
    [SerializeField] Text detailBody;
    [SerializeField] Text statusText;
    [SerializeField] Text countdownText;

    float remainingSeconds = 60f;

    [SerializeField] Color rowColor = new Color(0.07f, 0.1f, 0.16f, 1f);
    [SerializeField] Color rowSelectedColor = new Color(0.12f, 0.2f, 0.28f, 1f);
    [SerializeField] Color barColor = new Color(0.96f, 0.65f, 0.14f, 1f);

    int selectedIndex = -1;

    void Start()
    {
        Select(0);
        if (optionObjects != null && optionObjects.Length > 0 && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(optionObjects[0]);
    }

    void Update()
    {
        if (countdownText == null || remainingSeconds <= 0f)
            return;

        remainingSeconds -= Time.deltaTime;
        if (remainingSeconds < 0f)
            remainingSeconds = 0f;

        int seconds = Mathf.CeilToInt(remainingSeconds);
        countdownText.text = "T− 00:00:" + seconds.ToString("00");
    }

    public int SelectedIndex => selectedIndex;
    public string SelectedTitle => options != null && selectedIndex >= 0 && selectedIndex < options.Length
        ? options[selectedIndex].title
        : string.Empty;

    public void Select(int index)
    {
        if (options == null || index < 0 || index >= options.Length || index == selectedIndex)
            return;

        selectedIndex = index;

        for (int i = 0; i < options.Length; i++)
        {
            bool selected = i == index;
            if (rowBackgrounds != null && i < rowBackgrounds.Length && rowBackgrounds[i] != null)
                rowBackgrounds[i].color = selected ? rowSelectedColor : rowColor;
            if (selectionBars != null && i < selectionBars.Length && selectionBars[i] != null)
                selectionBars[i].color = selected ? barColor : new Color(barColor.r, barColor.g, barColor.b, 0f);
        }

        if (detailTitle != null)
            detailTitle.text = options[index].title;
        if (detailBody != null)
            detailBody.text = options[index].description;
        if (statusText != null && !statusText.text.StartsWith("LOCKED"))
            statusText.text = "SELECTED  ·  " + options[index].title;
    }

    public void Confirm()
    {
        if (string.IsNullOrEmpty(SelectedTitle))
            return;

        PlayerPrefs.SetString("SelectedConsole", SelectedTitle);
        PlayerPrefs.Save();

        if (statusText != null)
            statusText.text = "LOCKED  ·  " + SelectedTitle;

        Debug.Log("Console locked: " + SelectedTitle);
    }

    public void ExitMenu()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
