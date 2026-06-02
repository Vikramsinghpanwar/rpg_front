using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Core.Auth
{
    // Auth state machine values. AuthManager exposes the current value via CurrentState
    // and fires OnStateChanged when it transitions.
    public enum AuthState
    {
        Uninitialized,
        Authenticated,
        Unauthenticated,
        RefreshingToken,
        SessionExpired
    }

    public static class AuthChannels
    {
        public const string Sms = "sms";
        public const string WhatsApp = "whatsapp";
        public const string Voice = "voice";
    }

    // ── OTP login ───────────────────────────────────────────────────────────────
    [Serializable]
    public class OtpRequestBody
    {
        [JsonProperty("mobile")] public string Mobile;
        [JsonProperty("channel")] public string Channel;
    }

    [Serializable]
    public class OtpRequestResponse
    {
        [JsonProperty("challenge_id")] public string ChallengeId;
        [JsonProperty("expires_in")] public int ExpiresIn;
        [JsonProperty("resend_available_in")] public int ResendAvailableIn;
        [JsonProperty("max_attempts")] public int MaxAttempts;
    }

    [Serializable]
    public class OtpVerifyBody
    {
        [JsonProperty("challenge_id")] public string ChallengeId;
        [JsonProperty("code")] public string Code;
        [JsonProperty("platform")] public string Platform;
        [JsonProperty("device")] public DeviceInfo Device;
    }

    [Serializable]
    public class DeviceInfo
    {
        [JsonProperty("device_id")] public string DeviceId;
        [JsonProperty("platform")] public string Platform;
        [JsonProperty("model")] public string Model;
        [JsonProperty("os_version")] public string OsVersion;
        [JsonProperty("app_version")] public string AppVersion;
        [JsonProperty("push_token")] public string PushToken;
    }

    // ── Google login ────────────────────────────────────────────────────────────
    [Serializable]
    public class GoogleLoginBody
    {
        [JsonProperty("id_token")] public string IdToken;
        [JsonProperty("platform")] public string Platform;
        [JsonProperty("device")] public DeviceInfo Device;
    }

    // ── Login response (shared by OTP verify + Google) ─────────────────────────
    [Serializable]
    public class PlayerLoginResponse
    {
        [JsonProperty("access_token")] public string AccessToken;
        [JsonProperty("refresh_token")] public string RefreshToken;
        [JsonProperty("expires_in")] public int ExpiresIn;
        [JsonProperty("token_type")] public string TokenType;
        [JsonProperty("user")] public AuthUser User;
    }

    [Serializable]
    public class AuthUser
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("email")] public string Email;
        [JsonProperty("mobile")] public string Mobile;
        [JsonProperty("is_new")] public bool IsNew;
    }

    // ── Refresh + logout ───────────────────────────────────────────────────────
    [Serializable]
    public class RefreshBody
    {
        [JsonProperty("refresh_token")] public string RefreshToken;
    }

    [Serializable]
    public class LogoutBody
    {
        [JsonProperty("refresh_token")] public string RefreshToken;
    }

    [Serializable]
    public class TokenPair
    {
        [JsonProperty("access_token")] public string AccessToken;
        [JsonProperty("refresh_token")] public string RefreshToken;
        [JsonProperty("expires_in")] public int ExpiresIn;
        [JsonProperty("token_type")] public string TokenType;
    }

    // ── Me / Identity ──────────────────────────────────────────────────────────
    [Serializable]
    public class AuthMe
    {
        [JsonProperty("sub")] public string Sub;
        [JsonProperty("email")] public string Email;
        [JsonProperty("role")] public string Role;
        [JsonProperty("perms")] public List<string> Perms;
        [JsonProperty("exp")] public long Exp;
        [JsonProperty("jti")] public string Jti;
    }

    [Serializable]
    public class IdentityResponse
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("provider")] public string Provider;
        [JsonProperty("subject")] public string Subject;
        [JsonProperty("is_verified")] public bool IsVerified;
        [JsonProperty("created_at")] public string CreatedAt;
    }

    [Serializable]
    public class IdentitiesList
    {
        [JsonProperty("identities")] public List<IdentityResponse> Identities;
    }

    [Serializable]
    public class LinkMobileStartBody
    {
        [JsonProperty("mobile")] public string Mobile;
        [JsonProperty("channel")] public string Channel;
    }

    [Serializable]
    public class LinkMobileVerifyBody
    {
        [JsonProperty("challenge_id")] public string ChallengeId;
        [JsonProperty("code")] public string Code;
    }
}
