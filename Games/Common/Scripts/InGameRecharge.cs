using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Core.Bootstrap;
using Core.Managers;
using Core.Models;
using Core.Utils;
using Features.Lobby.Integration;
using Features.Recharge.Controllers;
using Features.Recharge.Models;
using Features.Recharge.Utils;
using Features.Recharge.UI;

public class InGameRecharge : MonoBehaviour
{
    public static InGameRecharge instance;
    public TextMeshProUGUI totalGetTMP;
    public TextMeshProUGUI cashTMP;
    public TextMeshProUGUI bonusTMP;
    public Text walletTxt;
    int rechargeAmount;
    int bonusAmount;
    public Text rechargeAmountTxt;
    public Text managerWalletTxt;
    public Transform rechargeBtns_Parent;
    public GameObject amountBtnPrefab;
    ScreenOrientation gameOrientation;
    GameObject rechargePanel;
    public Button[] allRechargeBtn_Array;
    public Color primaryColor;

    [Header("Custom Amount")]
    public TMP_InputField customAmountInput;

    [Header("Gateway Selection")]
    public Transform gatewayContainer;
    public GameObject gatewayBtnPrefab;

    private RechargeController controller;
    private RechargeAmountPreset selectedPreset;
    private readonly List<Button> presetButtons = new List<Button>();
    private int activeRechargeBtn;
    private bool isLoading;
    private long customAmountPaisa;
    private long customBonusPaisa;
    private bool _suppressCustomAmountEvent;
    private GatewayInfo selectedGateway;
    private readonly List<RechargeGatewayButton> gatewayButtons = new List<RechargeGatewayButton>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        if (managerWalletTxt != null)
        {
            walletTxt.text = managerWalletTxt.text;
        }
        else
        {
            walletTxt.text = (BootstrapLobbyAdapter.GetWalletBalanceTotal() / 100f).ToString("F2");
        }
    }

    private void Start()
    {
        if (rechargePanel == null)
            rechargePanel = transform.GetChild(0).gameObject;
        rechargePanel.SetActive(false);

        controller = GetComponent<RechargeController>();
        if (controller == null)
            controller = gameObject.AddComponent<RechargeController>();

        if (customAmountInput != null)
        {
            customAmountInput.contentType = TMP_InputField.ContentType.IntegerNumber;
            customAmountInput.onValueChanged.AddListener(OnCustomAmountChanged);
        }
    }

    private void OnDestroy()
    {
        Unsubscribe();
        if (customAmountInput != null)
            customAmountInput.onValueChanged.RemoveListener(OnCustomAmountChanged);
    }

    private void Subscribe()
    {
        if (controller != null)
        {
            controller.OnAmountsLoaded += OnAmountsLoaded;
            controller.OnGatewaysLoaded += OnGatewaysLoaded;
            controller.OnRechargeCreated += OnRechargeCreated;
            controller.OnError += OnRechargeError;
        }
        if (BootstrapService.Instance != null)
            BootstrapService.Instance.OnBootstrapUpdated += OnBootstrapUpdated;
    }

    private void Unsubscribe()
    {
        if (controller != null)
        {
            controller.OnAmountsLoaded -= OnAmountsLoaded;
            controller.OnGatewaysLoaded -= OnGatewaysLoaded;
            controller.OnRechargeCreated -= OnRechargeCreated;
            controller.OnError -= OnRechargeError;
        }
        if (BootstrapService.Instance != null)
            BootstrapService.Instance.OnBootstrapUpdated -= OnBootstrapUpdated;
    }

    public void OpenPanel()
    {
        gameOrientation = Screen.orientation;
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        if (rechargePanel == null)
            rechargePanel = transform.GetChild(0).gameObject;
        rechargePanel.SetActive(true);

        Subscribe();
        RefreshAndRender();
    }

    public void ClosePanel()
    {
        if (rechargePanel != null)
            rechargePanel.SetActive(false);
        Screen.orientation = gameOrientation;
        Unsubscribe();
        ClearGateways();
    }

    public void BuyCoin()
    {
        if (isLoading) return;
        if (selectedPreset == null && customAmountPaisa <= 0)
        {
            PopupManager.Instance?.ShowError("Please select an amount first");
            return;
        }
        _ = PayNowAsync();
    }

    private async Task PayNowAsync()
    {
        if (controller == null)
        {
            PopupManager.Instance?.ShowError("Payment system unavailable. Please try again later.");
            return;
        }

        var gateways = controller.GetGateways();
        var filteredGateways = gateways != null 
            ? gateways.Where(g => g.status == "active" && g.enabled_ingame).ToList() 
            : new List<GatewayInfo>();

        if (filteredGateways.Count == 0)
        {
            PopupManager.Instance?.ShowError("No payment methods available for in-game deposits. Please try again.");
            return;
        }

        // Verify that selectedGateway is valid and in the filtered list
        if (selectedGateway == null || !filteredGateways.Any(g => g.id == selectedGateway.id))
        {
            selectedGateway = filteredGateways[0];
        }

        isLoading = true;
        LoadingManager.Instance?.Show("Creating order...");

        try
        {
            long amountPaisa = selectedPreset != null ? selectedPreset.amount_paisa : customAmountPaisa;
            Debug.Log($"[PaymentGateway] Initiating recharge using selected gateway - id: {selectedGateway.id}, enabled_ingame: {selectedGateway.enabled_ingame}, amount: {amountPaisa}");
            string checkoutUrl = await controller.CreateRecharge(amountPaisa, selectedGateway.id);
            if (!string.IsNullOrEmpty(checkoutUrl))
            {
                Application.OpenURL(checkoutUrl);
                ClosePanel();
            }
            else
            {
                PopupManager.Instance?.ShowError("Failed to create recharge. Please try again.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"InGameRecharge.PayNow failed: {e.Message}");
            PopupManager.Instance?.ShowError("Something went wrong. Please try again.");
        }
        finally
        {
            isLoading = false;
            LoadingManager.Instance?.Hide();
        }
    }

    private void RefreshAndRender()
    {
        if (controller == null) return;
        controller.GetDefaultAmounts();
        LoadPresetsUI();

        // Always force load / refresh gateways for ingame source
        _ = RefreshGatewaysAsync();
    }

    private async Task RefreshGatewaysAsync()
    {
        if (controller == null) return;
        try
        {
            // Call LoadGateways with source = "ingame" and silent = false to block interaction via LoadingManager
            await controller.LoadGateways("ingame", silent: false);
        }
        catch (Exception e)
        {
            Debug.LogError($"[PaymentGateway] RefreshGatewaysAsync failed: {e.Message}");
        }
    }

    private void OnAmountsLoaded(List<RechargeAmountPreset> amounts)
    {
        if (rechargePanel != null && rechargePanel.activeSelf)
            LoadPresetsUI();
    }

    private void OnGatewaysLoaded(List<GatewayInfo> gateways)
    {
        PopulateGatewaysUI(gateways);
    }

    private void PopulateGatewaysUI(List<GatewayInfo> gateways)
    {
        ClearGateways();

        if (gatewayContainer == null || gatewayBtnPrefab == null)
        {
            // If there's no UI for gateway selection, we still filter them and choose the default.
            var filtered = gateways != null 
                ? gateways.Where(g => g.status == "active" && g.enabled_ingame).ToList() 
                : new List<GatewayInfo>();

            Debug.Log("[PaymentGateway] Filtering In-Game gateways (UI containers not assigned)...");
            foreach (var gw in filtered)
            {
                Debug.Log($"[PaymentGateway] Filtered InGame Gateway - id: {gw.id}, enabled_lobby: {gw.enabled_lobby}, enabled_ingame: {gw.enabled_ingame}");
            }

            if (filtered.Count > 0)
            {
                if (selectedGateway == null || !filtered.Any(g => g.id == selectedGateway.id))
                {
                    SelectGateway(filtered[0]);
                }
            }
            return;
        }

        var filteredGateways = gateways != null 
            ? gateways.Where(g => g.status == "active" && g.enabled_ingame).ToList() 
            : new List<GatewayInfo>();

        Debug.Log("[PaymentGateway] Filtering In-Game gateways...");
        foreach (var gw in filteredGateways)
        {
            Debug.Log($"[PaymentGateway] Filtered InGame Gateway - id: {gw.id}, enabled_lobby: {gw.enabled_lobby}, enabled_ingame: {gw.enabled_ingame}");
        }

        int gatewayCounter = 0;
        foreach (var gateway in filteredGateways)
        {
            gatewayCounter++;
            var go = Instantiate(gatewayBtnPrefab, gatewayContainer);
            var btn = go.GetComponent<RechargeGatewayButton>();
            if (btn == null) btn = go.AddComponent<RechargeGatewayButton>();
            btn.Setup(gateway, OnGatewayClicked, gatewayCounter);
            gatewayButtons.Add(btn);
        }

        // Selection restoration and fallback logic
        if (selectedGateway != null)
        {
            var match = filteredGateways.FirstOrDefault(g => g.id == selectedGateway.id);
            if (match != null)
            {
                SelectGateway(match);
            }
            else
            {
                selectedGateway = null;
            }
        }

        if (selectedGateway == null && filteredGateways.Count > 0)
        {
            SelectGateway(filteredGateways[0]);
        }
    }

    private void OnGatewayClicked(GatewayInfo gateway)
    {
        SelectGateway(gateway);
    }

    private void SelectGateway(GatewayInfo gateway)
    {
        selectedGateway = gateway;
        string sourceInfo = controller != null ? $"cache source: {controller.LoadedSource}" : "unknown cache";
        Debug.Log($"[PaymentGateway] Selected gateway in In-Game: {gateway?.id} ({sourceInfo})");

        foreach (var btn in gatewayButtons)
        {
            if (btn != null)
            {
                btn.SetSelected(btn.Gateway?.id == gateway?.id);
            }
        }
    }

    private void ClearGateways()
    {
        foreach (var btn in gatewayButtons)
        {
            if (btn != null) Destroy(btn.gameObject);
        }
        gatewayButtons.Clear();
        selectedGateway = null;
    }

    private void OnRechargeCreated(CreateRechargeResponse response)
    {
    }

    private void OnRechargeError(string code, string message)
    {
        PopupManager.Instance?.ShowError(message);
    }

    private void OnBootstrapUpdated(BootstrapResponse resp)
    {
        if (rechargePanel != null && rechargePanel.activeSelf)
            RefreshAndRender();
    }

    private void OnCustomAmountChanged(string value)
    {
        if (_suppressCustomAmountEvent) return;

        string cleaned = value?.Replace(",", "") ?? "";
        if (long.TryParse(cleaned, out long rupees) && rupees > 0)
        {
            customAmountPaisa = rupees * 100;
        }
        else
        {
            customAmountPaisa = 0;
        }

        selectedPreset = null;
        if (presetButtons.Count > 0 && activeRechargeBtn < presetButtons.Count && presetButtons[activeRechargeBtn] != null)
        {
            var oldBtn = presetButtons[activeRechargeBtn];
            var oldText = oldBtn.transform.GetChild(0).GetComponent<Text>();
            if (oldText != null) oldText.color = Color.black;
            oldBtn.image.color = Color.white;
        }

        var presets = controller != null ? controller.GetDefaultAmounts() : null;
        customBonusPaisa = Features.Recharge.Utils.RechargeBonusCalculator.GetApplicableBonus(customAmountPaisa, presets);

        int customRupees = (int)(customAmountPaisa / 100);
        int bonusRupees = (int)(customBonusPaisa / 100);

        rechargeAmountTxt.text = customAmountPaisa > 0 ? "Add Cash ₹" + customRupees : "";
        totalGetTMP.text = customAmountPaisa > 0 ? "₹" + (customRupees + bonusRupees) : "";
        cashTMP.text = customAmountPaisa > 0 ? "₹" + customRupees : "";
        bonusTMP.text = (customAmountPaisa > 0 && customBonusPaisa > 0) ? "₹" + bonusRupees : "";
    }

    private void LoadPresetsUI()
    {
        ClearPresets();
        if (rechargeBtns_Parent == null || amountBtnPrefab == null) return;

        var presets = controller != null ? controller.GetDefaultAmounts() : null;
        if (presets == null || presets.Count == 0)
        {
            PopupManager.Instance?.ShowError("Recharge presets unavailable. Please try again later.");
            return;
        }

        allRechargeBtn_Array = new Button[presets.Count];

        for (int i = 0; i < presets.Count; i++)
        {
            var preset = presets[i];
            int amountRupees = (int)(preset.amount_paisa / 100);
            int bonusRupees = (int)(preset.bonus_amount_paisa / 100);

            var g = Instantiate(amountBtnPrefab, rechargeBtns_Parent);
            int idx = i;
            Button b = g.GetComponent<Button>();
            presetButtons.Add(b);
            allRechargeBtn_Array[i] = b;

            Text amountTextChild = g.transform.GetChild(0).GetComponent<Text>();
            if (amountTextChild != null) amountTextChild.text = "₹" + amountRupees;

            Transform bonusContainer = g.transform.GetChild(1);
            if (bonusContainer != null && bonusContainer.childCount > 0)
            {
                Text bonusTextChild = bonusContainer.GetChild(0).GetComponent<Text>();
                if (bonusTextChild != null) bonusTextChild.text = "₹" + bonusRupees;
            }

            b.onClick.AddListener(() => SelectRechargeAmount(amountRupees, bonusRupees, idx));
        }

        if (presetButtons.Count > 0 && selectedPreset == null && customAmountPaisa == 0)
        {
            var first = presets[0];
            SelectRechargeAmount((int)(first.amount_paisa / 100), (int)(first.bonus_amount_paisa / 100), 0);
        }
    }

    public void SelectRechargeAmount(int val, int bonus, int index)
    {
        if (presetButtons.Count == 0 || index < 0 || index >= presetButtons.Count) return;

        if (customAmountInput != null)
        {
            _suppressCustomAmountEvent = true;
            customAmountInput.text = "";
            _suppressCustomAmountEvent = false;
        }
        customAmountPaisa = 0;
        customBonusPaisa = 0;

        if (activeRechargeBtn < presetButtons.Count && presetButtons[activeRechargeBtn] != null)
        {
            var oldBtn = presetButtons[activeRechargeBtn];
            oldBtn.transform.GetChild(0).GetComponent<Text>().color = Color.black;
            oldBtn.image.color = Color.white;
        }

        activeRechargeBtn = index;
        var newBtn = presetButtons[index];
        newBtn.transform.GetChild(0).GetComponent<Text>().color = Color.white;
        newBtn.image.color = primaryColor;

        rechargeAmount = val;
        bonusAmount = bonus;
        rechargeAmountTxt.text = "Add Cash ₹" + val;
        totalGetTMP.text = "₹" + (val + bonus);
        cashTMP.text = "₹" + val;
        bonusTMP.text = "₹" + bonus;

        var presets = controller != null ? controller.GetDefaultAmounts() : null;
        if (presets != null && index < presets.Count)
            selectedPreset = presets[index];
        else
            selectedPreset = new RechargeAmountPreset(val * 100L);
    }

    private void ClearPresets()
    {
        foreach (var btn in presetButtons)
        {
            if (btn != null) Destroy(btn.gameObject);
        }
        presetButtons.Clear();
        selectedPreset = null;
        activeRechargeBtn = 0;
    }
}
