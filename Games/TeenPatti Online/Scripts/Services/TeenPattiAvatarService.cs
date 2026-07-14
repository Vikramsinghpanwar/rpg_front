using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Features.Profile.Services;

namespace Teenpatti
{
    public static class TeenPattiAvatarService
    {
        private static Dictionary<string, Sprite> _localAvatarCache;
        private static Dictionary<string, Sprite> _remoteCache;
        private static Dictionary<string, Task<Sprite>> _pendingDownloads;
        private static Sprite _placeholder;
        private static bool _preloaded;
        private static readonly object _pendingLock = new object();

        private static void EnsurePreloaded()
        {
            if (_preloaded) return;

            _localAvatarCache = new Dictionary<string, Sprite>();
            _remoteCache = new Dictionary<string, Sprite>();
            _pendingDownloads = new Dictionary<string, Task<Sprite>>();

            var sprites = Resources.LoadAll<Sprite>("Avatar");
            if (sprites != null && sprites.Length > 0)
            {
                foreach (var sprite in sprites)
                {
                    if (sprite == null) continue;
                    string key = "Avatar/" + sprite.name;
                    if (!_localAvatarCache.ContainsKey(key))
                    {
                        _localAvatarCache[key] = sprite;
                    }
                }

                if (!_localAvatarCache.TryGetValue("Avatar/0", out _placeholder))
                {
                    _placeholder = sprites[0];
                }
            }
            else
            {
                Debug.LogWarning("[TeenPattiAvatarService] No sprites found in Resources/Avatar");
            }

            _preloaded = true;
        }

        public static void SetAvatar(
            PlayerManager playerManager,
            string profileImageUrl,
            string expectedUserId)
        {
            // ── TEMPORARY DIAGNOSTIC LOGGING ──────────────────────────────
            // TODO: Remove these logs after Round 2 cache-hit verification.
            Debug.Log($"[Avatar] SetAvatar called | User: {playerManager?.myId ?? "null"} | ExpectedUser: {expectedUserId ?? "null"} | IncomingURL: {profileImageUrl ?? "(null)"} | StoredURL: {playerManager?.profileImageUrl ?? "(null)"} | RemoteCacheCount: {_remoteCache?.Count ?? 0}");
            // ──────────────────────────────────────────────────────────────

            if (playerManager == null) return;

            EnsurePreloaded();

            // Race condition guard: reject if this player is no longer the expected player
            if (!string.IsNullOrEmpty(expectedUserId) &&
                !string.IsNullOrEmpty(playerManager.myId) &&
                playerManager.myId != expectedUserId)
            {
                Debug.LogWarning("[Avatar] REJECTED by race guard — myId mismatch");
                return;
            }

            string url = profileImageUrl ?? string.Empty;

            // ── CASE A: Incoming URL empty ────────────────────────────────
            if (string.IsNullOrEmpty(url))
            {
                // Fallback to the URL previously stored on the player object.
                // This preserves avatars across rounds when the backend
                // sends an empty profileImageUrl for a seated player.
                url = playerManager.profileImageUrl ?? string.Empty;
                Debug.Log($"[Avatar] Empty incoming URL, fell back to stored URL: {url ?? "(null)"}");
            }

            if (string.IsNullOrEmpty(url))
            {
                // Still empty → placeholder
                Debug.Log("[Avatar] CASE A: Empty URL after fallback → Assigning PLACEHOLDER");
                playerManager.profileImg.sprite = GetPlaceholder();
                return;
            }

            // ── CASE B: Local avatar (Avatar/X) ──────────────────────────
            if (url.StartsWith("Avatar/"))
            {
                Debug.Log($"[Avatar] CASE B: Local path detected: {url}");
                bool localCacheHit = _localAvatarCache.TryGetValue(url, out var sprite);

                if (!localCacheHit)
                {
                    Debug.Log($"[Avatar] Local cache MISS → Resources.Load<Sprite>(\"{url}\")");
                    sprite = Resources.Load<Sprite>(url);
                    if (sprite != null)
                    {
                        _localAvatarCache[url] = sprite;
                    }
                }
                else
                {
                    Debug.Log("[Avatar] Local cache HIT");
                }

                playerManager.profileImg.sprite = sprite ?? GetPlaceholder();
                if (sprite != null)
                {
                    playerManager.profileImageUrl = url;
                    Debug.Log($"[Avatar] Local sprite assigned from {(localCacheHit ? "cache" : "Resources.Load")} | Stored URL: {url}");
                }
                else
                {
                    Debug.LogWarning("[Avatar] Local sprite was null after cache miss → Assigning PLACEHOLDER");
                }
                return;
            }

            // ── CASE C: Remote URL ───────────────────────────────────────
            Debug.Log($"[Avatar] CASE C: Remote URL detected: {url}");

            // FIRST: check synchronous cache hit.
            // This is the critical fix: we must reuse cached sprites
            // instantly without ever showing the placeholder.
            if (_remoteCache.TryGetValue(url, out var cachedRemote))
            {
                Debug.Log("[Avatar] Remote cache HIT → Assigning cached sprite synchronously");
                playerManager.profileImg.sprite = cachedRemote;
                playerManager.profileImageUrl = url;  // ensure stored URL is current
                Debug.Log($"[Avatar] Sprite assigned: {(cachedRemote?.name ?? "(null)")} | Stored URL: {url}");
                return;
            }

            Debug.Log("[Avatar] Remote cache MISS → Assigning PLACEHOLDER then starting async download");
            // Cache miss: show placeholder while download runs
            playerManager.profileImg.sprite = GetPlaceholder();
            _ = DownloadAndAssignAsync(playerManager, url, expectedUserId);
        }

        private static async Task DownloadAndAssignAsync(
            PlayerManager playerManager,
            string url,
            string expectedUserId)
        {
            Debug.Log("[Avatar] DownloadAndAssignAsync started");
            try
            {
                var sprite = await GetOrDownloadRemoteAsync(url);
                Debug.Log($"[Avatar] DownloadAndAssignAsync resumed | sprite null: {sprite == null}");

                if (sprite == null || playerManager == null)
                {
                    Debug.LogWarning("[Avatar] Download returned null sprite → leaving placeholder");
                    return;
                }

                // Final race-condition check before assignment
                if (!string.IsNullOrEmpty(expectedUserId) &&
                    !string.IsNullOrEmpty(playerManager.myId) &&
                    playerManager.myId != expectedUserId)
                {
                    Debug.LogWarning("[Avatar] REJECTED by race guard in DownloadAndAssignAsync — myId mismatch");
                    return;
                }

                if (playerManager.isVacant || string.IsNullOrEmpty(playerManager.myId))
                {
                    Debug.LogWarning("[Avatar] REJECTED — player is vacant or myId is empty");
                    return;
                }

                playerManager.profileImg.sprite = sprite;
                playerManager.profileImageUrl = url;
                Debug.Log($"[Avatar] Remote sprite assigned after download | Sprite: {sprite?.name ?? "(null)"} | URL: {url} | CacheCount: {_remoteCache.Count}");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[TeenPattiAvatarService] Remote avatar assignment failed: {ex.Message}");
            }
        }

        private static async Task<Sprite> GetOrDownloadRemoteAsync(string url)
        {
            if (string.IsNullOrEmpty(url)) return null;

            // 1. Fast cache hit
            if (_remoteCache.TryGetValue(url, out var cached))
            {
                Debug.Log("[Avatar] GetOrDownloadRemoteAsync: cache hit");
                return cached;
            }

            // 2. In-flight dedup: another caller is already downloading this URL
            Task<Sprite> inFlight;
            bool hasInFlight;
            lock (_pendingLock)
            {
                hasInFlight = _pendingDownloads.TryGetValue(url, out inFlight);
            }
            if (hasInFlight)
            {
                Debug.Log("[Avatar] GetOrDownloadRemoteAsync: in-flight dedup hit");
                return await inFlight;
            }

            // 3. Start new download
            Debug.Log($"[Avatar] GetOrDownloadRemoteAsync: STARTING download | URL: {url}");
            var downloadTask = DownloadRemoteSpriteAsync(url);
            lock (_pendingLock)
            {
                _pendingDownloads[url] = downloadTask;
            }

            try
            {
                var result = await downloadTask;
                if (result != null)
                {
                    // Insert into cache BEFORE returning so all awaiters see the cached value
                    _remoteCache[url] = result;
                    Debug.Log($"[Avatar] GetOrDownloadRemoteAsync: download COMPLETE | Sprite: {result?.name ?? "(null)"} | CacheCount: {_remoteCache.Count}");
                }
                else
                {
                    Debug.LogWarning("[Avatar] GetOrDownloadRemoteAsync: download returned null");
                }
                return result;
            }
            finally
            {
                lock (_pendingLock)
                {
                    _pendingDownloads.Remove(url);
                }
            }
        }

        private static async Task<Sprite> DownloadRemoteSpriteAsync(string url)
        {
            try
            {
                Debug.Log($"[Avatar] DownloadRemoteSpriteAsync: calling AvatarLoader | URL: {url}");
                return await AvatarLoader.LoadRemoteAsync(url);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[TeenPattiAvatarService] Download failed for {url}: {ex.Message}");
                return null;
            }
        }

        public static Sprite GetPlaceholder()
        {
            EnsurePreloaded();
            if (_placeholder != null) return _placeholder;

            _placeholder = Resources.Load<Sprite>("Avatar/0");
            return _placeholder;
        }

        public static bool IsRemoteUrl(string url)
        {
            return !string.IsNullOrEmpty(url) &&
                   (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    url.StartsWith("https://", StringComparison.OrdinalIgnoreCase));
        }

        public static bool IsLocalAvatarPath(string url)
        {
            return !string.IsNullOrEmpty(url) && url.StartsWith("Avatar/");
        }
    }
}
