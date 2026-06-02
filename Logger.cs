using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Logger : MonoBehaviour
{
    public static Logger Instance;

    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Text logText;
    private Image accentBar;
    private RectTransform toastRect;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        CreateUI();
    }

    void CreateUI()
    {
        // ── Canvas ──────────────────────────────────────────────
        GameObject canvasObj = new GameObject("LoggerCanvas");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();
        DontDestroyOnLoad(canvasObj);

        // ── Toast container ──────────────────────────────────────
        GameObject toastObj = new GameObject("Toast");
        toastObj.transform.SetParent(canvasObj.transform, false);

        Image bg = toastObj.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.1f, 0.1f, 1f);

        toastRect = toastObj.GetComponent<RectTransform>();
        toastRect.anchorMin = new Vector2(0.5f, 0f);
        toastRect.anchorMax = new Vector2(0.5f, 0f);
        toastRect.pivot     = new Vector2(0.5f, 0f);
        toastRect.anchoredPosition = new Vector2(0f, -180f); // hidden start pos
        toastRect.sizeDelta = new Vector2(1400f, 140f);

        canvasGroup = toastObj.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;

        // ── Accent bar (left colour strip) ───────────────────────
        GameObject accentObj = new GameObject("AccentBar");
        accentObj.transform.SetParent(toastObj.transform, false);
        accentBar = accentObj.AddComponent<Image>();
        accentBar.color = Color.white;
        RectTransform accentRect = accentBar.GetComponent<RectTransform>();
        accentRect.anchorMin = Vector2.zero;
        accentRect.anchorMax = new Vector2(0f, 1f);
        accentRect.pivot     = new Vector2(0f, 0.5f);
        accentRect.offsetMin = Vector2.zero;
        accentRect.offsetMax = Vector2.zero;
        accentRect.sizeDelta = new Vector2(6f, 0f);
        accentRect.anchoredPosition = Vector2.zero;

        // ── Text — stretches the full toast ──────────────────────
        GameObject textObj = new GameObject("LogText");
        textObj.transform.SetParent(toastObj.transform, false);

        logText = textObj.AddComponent<Text>();

        // Robust font loading: try built-in first, then OS fallback
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (font == null) font = Font.CreateDynamicFontFromOSFont("Arial", 26);
        if (font == null) font = Font.CreateDynamicFontFromOSFont("Helvetica", 26);
        if (font == null)
        {
            string[] osFonts = Font.GetOSInstalledFontNames();
            if (osFonts.Length > 0)
                font = Font.CreateDynamicFontFromOSFont(osFonts[0], 26);
        }
        logText.font = font;

        logText.fontSize  = 64;
        logText.resizeTextForBestFit = true;
        logText.resizeTextMinSize = 24;
        logText.resizeTextMaxSize = 64;
        logText.color     = Color.white;
        logText.alignment = TextAnchor.MiddleCenter;
        logText.horizontalOverflow = HorizontalWrapMode.Wrap;
        logText.verticalOverflow   = VerticalWrapMode.Overflow;
        logText.supportRichText    = true;

        // Stretch to fill the full toast, with padding
        RectTransform textRect = logText.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(18f,  8f);  // left + bottom padding
        textRect.offsetMax = new Vector2(-14f, -8f); // right + top padding
    }

    void Start()
    {
    }

    // ──────────────── PUBLIC API ────────────────────────────────

    public void Log(string message)
    {
        Show(message, Color.white, new Color(0.4f, 0.9f, 1f));
        Debug.Log(message);
    }

    public void Error(string message)
    {
        Show(message, new Color(1f, 0.45f, 0.45f), new Color(1f, 0.2f, 0.2f));
        Debug.LogError(message);
    }

    public void Warning(string message)
    {
        Show(message, new Color(1f, 0.85f, 0.3f), new Color(1f, 0.6f, 0f));
        Debug.LogWarning(message);
    }

    // ──────────────── CORE ──────────────────────────────────────

    void Show(string message, Color textColor, Color accentColor)
    {
        StopAllCoroutines();
        logText.text  = message;
        logText.color = textColor;
        if (accentBar != null) accentBar.color = accentColor;
        StartCoroutine(ShowRoutine());
    }

    IEnumerator ShowRoutine()
    {
        Vector2 hiddenPos = new Vector2(0f, -180f);
        Vector2 shownPos  = new Vector2(0f, 140f);

        // Slide up + fade in
        float slideTime = 0.2f, t = 0f;
        while (t < slideTime)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / slideTime);
            canvasGroup.alpha = p;
            toastRect.anchoredPosition = Vector2.Lerp(hiddenPos, shownPos, p);
            yield return null;
        }
        canvasGroup.alpha = 1f;
        toastRect.anchoredPosition = shownPos;

        // Hold
        yield return new WaitForSeconds(3f);

        // Fade out
        float fadeTime = 0.4f; t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = 1f - Mathf.SmoothStep(0f, 1f, t / fadeTime);
            yield return null;
        }
        canvasGroup.alpha = 0f;
        toastRect.anchoredPosition = hiddenPos;
    }

    // ──────────────── AUTO INIT ─────────────────────────────────

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        if (Instance == null)
        {
            new GameObject("Logger").AddComponent<Logger>();
        }
    }
}