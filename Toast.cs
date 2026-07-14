using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Toast : MonoBehaviour
{
    public static Toast Instance;

    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Text logText;
    private Image accentBar;
    private RectTransform toastRect;

    private const string CanvasName = "ToastCanvas";
    private const string ToastObjName = "Toast";
    private const string AccentObjName = "AccentBar";
    private const string TextObjName = "LogText";

    void Awake()
    {
        // Debug.Log("[Toast] Awake called on " + gameObject.name + " InstanceWasNull=" + (Instance == null));
        if (Instance != null && Instance != this)
        {
            Debug.Log("[Toast] Duplicate instance detected — destroying " + gameObject.name);
            Destroy(gameObject);

            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        // Debug.Log("[Toast] Instance assigned, calling CreateUI");
        try
        {
            CreateUI();
            // Debug.Log("[Toast] CreateUI completed successfully");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[Toast] CreateUI FAILED: " + ex.GetType().Name + " — " + ex.Message + "\n" + ex.StackTrace);
        }
    }

    void CreateUI()
    {
        // ── Canvas ──────────────────────────────────────────────
        GameObject canvasObj = new GameObject(CanvasName);
        // Debug.Log("[Toast] Created canvas GameObject: " + canvasObj.name);
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();
        DontDestroyOnLoad(canvasObj);
        // Debug.Log("[Toast] Canvas configured, sortingOrder=" + canvas.sortingOrder);

        // ── Toast container ──────────────────────────────────────
        GameObject toastObj = new GameObject(ToastObjName);
        toastObj.transform.SetParent(canvasObj.transform, false);
        // Debug.Log("[Toast] Created toast container: " + toastObj.name);

        Image bg = toastObj.AddComponent<Image>();
        bg.color = new Color(0.9f, 0.6f, 0.2f, 1f);
        // Debug.Log("[Toast] Toast background image added");

        toastRect = toastObj.GetComponent<RectTransform>();
        toastRect.anchorMin = new Vector2(0.5f, 0f);
        toastRect.anchorMax = new Vector2(0.5f, 0f);
        toastRect.pivot = new Vector2(0.5f, 0f);
        toastRect.anchoredPosition = new Vector2(0f, -180f); // hidden start pos
        toastRect.sizeDelta = new Vector2(1400f, 140f);
        // Debug.Log("[Toast] Toast RectTransform set, sizeDelta=" + toastRect.sizeDelta);

        canvasGroup = toastObj.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        // Debug.Log("[Toast] CanvasGroup added, initial alpha=" + canvasGroup.alpha);

        // ── Accent bar (left colour strip) ───────────────────────
        GameObject accentObj = new GameObject(AccentObjName);
        accentObj.transform.SetParent(toastObj.transform, false);
        accentBar = accentObj.AddComponent<Image>();
        accentBar.color = Color.white;
        RectTransform accentRect = accentBar.GetComponent<RectTransform>();
        accentRect.anchorMin = Vector2.zero;
        accentRect.anchorMax = new Vector2(0f, 1f);
        accentRect.pivot = new Vector2(0f, 0.5f);
        accentRect.offsetMin = Vector2.zero;
        accentRect.offsetMax = Vector2.zero;
        accentRect.sizeDelta = new Vector2(6f, 0f);
        accentRect.anchoredPosition = Vector2.zero;
        // Debug.Log("[Toast] Accent bar created");

        // ── Text — stretches the full toast ──────────────────────
        GameObject textObj = new GameObject(TextObjName);
        textObj.transform.SetParent(toastObj.transform, false);
        // Debug.Log("[Toast] Text GameObject created: " + textObj.name);

        logText = textObj.AddComponent<Text>();
        // Debug.Log("[Toast] Text component added, logText=" + (logText != null));

        // Font loading with diagnostics
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        // Debug.Log("[Toast] LegacyRuntime.ttf: " + (font != null));
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        // Debug.Log("[Toast] Arial.ttf: " + (font != null));
        if (font == null) font = Font.CreateDynamicFontFromOSFont("Arial", 26);
        // Debug.Log("[Toast] OS Arial: " + (font != null));
        if (font == null) font = Font.CreateDynamicFontFromOSFont("Helvetica", 26);
        // Debug.Log("[Toast] OS Helvetica: " + (font != null));
        if (font == null)
        {
            string[] osFonts = Font.GetOSInstalledFontNames();
            // Debug.Log("[Toast] OS installed fonts count: " + (osFonts != null ? osFonts.Length : 0));
            if (osFonts != null && osFonts.Length > 0)
            {
                font = Font.CreateDynamicFontFromOSFont(osFonts[0], 26);
                // Debug.Log("[Toast] Fallback to first OS font: " + osFonts[0] + " success=" + (font != null));
            }
        }
        if (font == null)
        {
            Debug.LogError("[Toast] ALL font loading paths FAILED — text will not render!");
            return;
        }
        logText.font = font;
        // Debug.Log("[Toast] Font assigned: " + font.name);

        logText.fontSize = 64;
        logText.resizeTextForBestFit = true;
        logText.resizeTextMinSize = 24;
        logText.resizeTextMaxSize = 64;
        logText.color = Color.white;
        logText.alignment = TextAnchor.MiddleCenter;
        logText.horizontalOverflow = HorizontalWrapMode.Wrap;
        logText.verticalOverflow = VerticalWrapMode.Overflow;
        logText.supportRichText = true;
        // Debug.Log("[Toast] Text properties set");

        RectTransform textRect = logText.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(18f, 8f);
        textRect.offsetMax = new Vector2(-14f, -8f);
        // Debug.Log("[Toast] Text RectTransform set");

        // Force initial state
        canvasGroup.alpha = 0f;
        toastRect.anchoredPosition = new Vector2(0f, -180f);
        // Debug.Log("[Toast] Initial state: alpha=0, position=(0,-180)");

        // Debug.Log("[Toast] CreateUI COMPLETE — toast object hierarchy: " + canvasObj.name +"/" + toastObj.name + "/" + accentObj.name + ", " + textObj.name);
    }

    void Start()
    {
        // Debug.Log("[Toast] Start called — instanceCheck=" + (Instance == this) +" canvas=" + (canvas != null) + " logText=" + (logText != null) + " canvasGroup=" + (canvasGroup != null) + " toastRect=" + (toastRect != null) + " accentBar=" + (accentBar != null));

        Invoke("ShowDummyLog", 3f);
    }

    void Update()
    {
        // Uncomment only for deep debugging — can spam console
        // if (canvasGroup != null && canvasGroup.alpha > 0f)
        //     Debug.Log($"[Toast] Update: alpha={canvasGroup.alpha:F3} pos={toastRect.anchoredPosition}");
    }

    // ──────────────── PUBLIC API ────────────────────────────────

    public void ShowLog(string message)
    {
        Ensure();
        if (Instance == null)
        {
            Debug.LogWarning("[Toast] ShowLog skipped — Instance is null after Ensure()");
            return;
        }
        // Debug.Log("[Toast] ShowLog called: \"" + message + "\"");
        Show(message, Color.white, new Color(0.4f, 0.9f, 1f));
        // Debug.Log("[Toast] ShowLog delegated to Show()");
    }

    public void ShowSuccess(string message)
    {
        Ensure();
        if (Instance == null)
        {
            Debug.LogWarning("[Toast] ShowSuccess skipped — Instance is null after Ensure()");
            return;
        }
        // Debug.Log("[Toast] ShowSuccess called: \"" + message + "\"");
        Show(message, Color.white, new Color(0.4f, 0.9f, 1f));
        // Debug.Log("[Toast] ShowSuccess delegated to Show()");
    }

    public void ShowError(string message)
    {
        Ensure();
        if (Instance == null)
        {
            Debug.LogWarning("[Toast] ShowError skipped — Instance is null after Ensure()");
            return;
        }
        // Debug.Log("[Toast] ShowError called: \"" + message + "\"");
        Show(message, Color.black, new Color(1f, 0.2f, 0.2f));
        // Debug.Log("[Toast] ShowError delegated to Show()");
    }

    public void ShowWarning(string message)
    {
        Ensure();
        if (Instance == null)
        {
            Debug.LogWarning("[Toast] ShowWarning skipped — Instance is null after Ensure()");
            return;
        }
        // Debug.Log("[Toast] ShowWarning called: \"" + message + "\"");
        Show(message, Color.blue, new Color(1f, 0.6f, 0f));
        // Debug.Log("[Toast] ShowWarning delegated to Show()");
    }

    // ──────────────── CORE ──────────────────────────────────────

    void Show(string message, Color textColor, Color accentColor)
    {
        if (logText == null)
        {
            Debug.LogError("[Toast] Show() ABORTED — logText is null (CreateUI probably failed)");
            return;
        }
        if (canvasGroup == null)
        {
            Debug.LogError("[Toast] Show() ABORTED — canvasGroup is null");
            return;
        }
        if (toastRect == null)
        {
            Debug.LogError("[Toast] Show() ABORTED — toastRect is null");
            return;
        }

        StopAllCoroutines();
        logText.text = message;
        logText.color = textColor;
        if (accentBar != null) accentBar.color = accentColor;
        // Debug.Log("[Toast] Show() starting coroutine — text=\"" + message + "\"");
        StartCoroutine(ShowRoutine());
    }

    IEnumerator ShowRoutine()
    {
        Vector2 hiddenPos = new Vector2(0f, -180f);
        Vector2 shownPos = new Vector2(0f, 140f);

        // Debug.Log("[Toast] ShowRoutine: slide-in starting");
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
        // Debug.Log("[Toast] ShowRoutine: slide-in COMPLETE — alpha=1 pos=" + shownPos);

        // Hold
        // Debug.Log("[Toast] ShowRoutine: holding for 3s");
        yield return new WaitForSeconds(3f);

        // Fade out
        // Debug.Log("[Toast] ShowRoutine: fade-out starting");
        float fadeTime = 0.4f; t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = 1f - Mathf.SmoothStep(0f, 1f, t / fadeTime);
            toastRect.anchoredPosition = Vector2.Lerp(shownPos, hiddenPos, Mathf.SmoothStep(0f, 1f, t / fadeTime));
            yield return null;
        }
        canvasGroup.alpha = 0f;
        toastRect.anchoredPosition = hiddenPos;
        // Debug.Log("[Toast] ShowRoutine: fade-out COMPLETE — alpha=0 pos=" + hiddenPos);
    }

    // ──────────────── AUTO INIT ─────────────────────────────────
    // Fallback only — skipped when a scene-placed Toast GameObject
    // (with the Toast component attached) already set Instance in Awake.

    public static void EnsureInitialized()
    {
        if (Instance == null)
        {
            Debug.LogWarning("[Toast] Auto-creating Toast — Instance was null at call-time");
            GameObject go = new GameObject("Toast");
            go.AddComponent<Toast>();
        }
    }

    public static void Ensure()
    {
        EnsureInitialized();
    }
}
