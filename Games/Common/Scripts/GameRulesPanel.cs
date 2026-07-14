using UnityEngine;

public class GameRulesPanel : MonoBehaviour
{
    [HideInInspector] private GameObject rulesPanel;
    void Start()
    {
        rulesPanel = transform.GetChild(0).gameObject;
    }
    public void Show()
    {
        if (rulesPanel != null)
            rulesPanel.gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (rulesPanel != null)
            rulesPanel.gameObject.SetActive(false);
    }

    public void Toggle()
    {
        if (rulesPanel == null) return;
        rulesPanel.gameObject.SetActive(!rulesPanel.gameObject.activeSelf);
    }
}
