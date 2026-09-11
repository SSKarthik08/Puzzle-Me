using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WinPanelUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject winPanel;

    private TMP_Text winText;
    private Button restartButton;

    void Awake()
    {
        if (winPanel == null)
        {
            Debug.LogError(
                "WinPanelUI: Win Panel is NOT assigned!"
            );

            return;
        }

        winText =
            winPanel.GetComponentInChildren<TMP_Text>(true);

        restartButton =
            winPanel.GetComponentInChildren<Button>(true);

        HideWinPanel();
    }

    public void ShowWinPanel()
    {
        if (winPanel == null)
        {
            Debug.LogError(
                "WinPanelUI: WinPanel reference is missing!"
            );

            return;
        }

        winPanel.SetActive(true);

        winPanel.transform.SetAsLastSibling();

        if (winText == null)
        {
            winText =
                winPanel.GetComponentInChildren<TMP_Text>(true);
        }

        if (winText != null)
        {
            winText.gameObject.SetActive(true);

            winText.text = "PUZZLE COMPLETE!";

            winText.fontSize = 42;

            winText.color = Color.black;

            winText.alignment =
                TextAlignmentOptions.Center;

            RectTransform textRect =
                winText.GetComponent<RectTransform>();

            textRect.anchorMin =
                new Vector2(0.5f, 0.5f);

            textRect.anchorMax =
                new Vector2(0.5f, 0.5f);

            textRect.pivot =
                new Vector2(0.5f, 0.5f);

            textRect.anchoredPosition =
                new Vector2(0f, 80f);

            textRect.sizeDelta =
                new Vector2(600f, 100f);
        }
        else
        {
            Debug.LogError(
                "WinPanelUI: WinText TMP component not found!"
            );
        }

        if (restartButton == null)
        {
            restartButton =
                winPanel.GetComponentInChildren<Button>(true);
        }

        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(true);

            TMP_Text buttonText =
                restartButton.GetComponentInChildren<TMP_Text>(true);

            if (buttonText != null)
            {
                buttonText.text = "PLAY AGAIN";

                buttonText.fontSize = 28;

                buttonText.alignment =
                    TextAlignmentOptions.Center;
            }
        }

        Debug.Log("WIN UI DISPLAYED AUTOMATICALLY!");
    }

    public void HideWinPanel()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
    }
}