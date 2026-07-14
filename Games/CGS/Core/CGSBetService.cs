using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.API;
using Core.API.Endpoints;
using Core.Services;
using Core.Models;
using UnityEngine;

// CGS bet service — single entry point for all casino game HTTP bet operations.
//
// Rules enforced here:
//   • Bets go through HTTP POST /cgs/bets — never via Socket.IO
//   • Every bet carries a fresh Idempotency-Key (UUID); retries of the same logical
//     bet reuse the key so the server deduplicates double-taps safely
//   • Amounts are integer paisa (long) — never float rupees
//   • userId is NOT sent in the request body; the server reads it from the JWT
//   • Wallet display is updated from the server response balance after every bet/cashout
public class CGSBetService : MonoBehaviour
{
    public static CGSBetService Instance { get; private set; }

        public static void EnsureInitialized()
        {
            if (Instance != null) return;
            var go = new GameObject("[CGSBetService]");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<CGSBetService>();
        }

        // ── Bet ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Place a bet via HTTP POST /cgs/bets.
    /// <paramref name="amountPaisa"/> must be integer paisa (₹1 = 100 paisa).
    /// Throws <see cref="ApiException"/> on rejection (caller should surface error to player).
    /// </summary>
    public async Task<CGSBetResponse> PlaceBetAsync(
        string gameKey,
        object betOn,
        long   amountPaisa,
        string tableCode = null)
    {
        var request = new CGSBetRequest
        {
            GameKey     = gameKey,
            BetOn       = betOn,
            AmountPaisa = amountPaisa,
            TableCode   = tableCode
        };

        var headers = new Dictionary<string, string>
        {
            { "Idempotency-Key", Guid.NewGuid().ToString() }
        };

        var response = await ApiClient.Instance.Post<CGSBetResponse>(CGSRoutes.PlaceBet, request, headers);
        UpdateWalletFromBalance(response?.Data?.Balance);
        return response;
    }

    // ── Cashout ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Cash out an active multiplier bet via HTTP POST /cgs/cashout (Crash, Aviator).
    /// Throws <see cref="ApiException"/> if no active bet or round already ended.
    /// </summary>
    public async Task<CGSCashoutResponse> CashoutAsync(string gameKey, string tableCode = null)
    {
        var request = new CGSCashoutRequest
        {
            GameKey   = gameKey,
            TableCode = tableCode
        };

        var response = await ApiClient.Instance.Post<CGSCashoutResponse>(CGSRoutes.Cashout, request, null);
        UpdateWalletFromBalance(response?.Data?.Balance);
        return response;
    }

    // ── Wallet helpers ───────────────────────────────────────────────────────

    // Apply a server-authoritative balance snapshot to the legacy Wallet display.
    // Bridge method: keeps Wallet.cs working until it is replaced in Phase 6.
    public void UpdateWalletFromBalance(CGSBalance balance)
    {
        if (balance == null) return;
        MainThreadDispatcher.Enqueue(() =>
        {
            Wallet.SetDepositWallet((float)(balance.Wallet    / 100.0));
            Wallet.SetWinWallet    ((float)(balance.WinAmount / 100.0));
            Wallet.SetBonus        ((float)(balance.Bonus     / 100.0));
        });
    }

// Best-effort wallet refresh after a round result. Calls the wallet balance
// endpoint; silently ignored if the endpoint is not yet exposed through the gateway.
public async Task RefreshWalletAsync()
{
    try
    {
        string userId = TokenProvider.Instance?.GetUserId();
        if (string.IsNullOrEmpty(userId)) return;

        var response = await ApiClient.Instance.Get<Core.Models.WalletBalanceResponse>(WalletRoutes.MeBalance());
        if (response == null) return;

        Wallet.SetDepositWallet((float)(response.deposit_balance / 100.0));
        Wallet.SetWinWallet    ((float)(response.win_balance     / 100.0));
        Wallet.SetBonus        ((float)(response.bonus_balance   / 100.0));

        Core.Bootstrap.BootstrapService.Instance?.ApplyWalletBalance(response);
    }
    catch (Exception ex)
    {
        Debug.LogWarning($"[CGSBetService] Post-round wallet refresh skipped: {ex.Message}");
    }
}
}
