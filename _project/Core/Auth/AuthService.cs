using System.Threading.Tasks;
using Core.API;
using Core.API.Endpoints;
using UnityEngine;

namespace Core.Auth
{
    // Thin wrapper around ApiClient that maps every player-auth endpoint to a Task method.
    // Stateless — AuthManager owns the state machine and stores tokens via TokenProvider.
    // All routes come from Core.API.Endpoints.AuthRoutes — no string literals here.
    public static class AuthService
    {
        public static Task<OtpRequestResponse> RequestOtp(string mobile, string channel = AuthChannels.Sms)
        {
            return ApiClient.Instance.Post<OtpRequestResponse>(
                AuthRoutes.LoginOtpReq,
                new OtpRequestBody { Mobile = mobile, Channel = channel });
        }

        public static Task<PlayerLoginResponse> VerifyOtp(string challengeId, string code, DeviceInfo device = null)
        {
            return ApiClient.Instance.Post<PlayerLoginResponse>(
                AuthRoutes.LoginOtpVerify,
                new OtpVerifyBody { ChallengeId = challengeId, Code = code, Device = device ?? CurrentDevice() });
        }

        public static Task<PlayerLoginResponse> LoginWithGoogle(string idToken, DeviceInfo device = null)
        {
            return ApiClient.Instance.Post<PlayerLoginResponse>(
                AuthRoutes.LoginGoogle,
                new GoogleLoginBody { IdToken = idToken, Device = device ?? CurrentDevice() });
        }

        public static Task<TokenPair> Refresh(string refreshToken)
        {
            return ApiClient.Instance.Post<TokenPair>(
                AuthRoutes.Refresh,
                new RefreshBody { RefreshToken = refreshToken });
        }

        public static Task<object> Logout(string refreshToken)
        {
            return ApiClient.Instance.Post<object>(
                AuthRoutes.Logout,
                new LogoutBody { RefreshToken = refreshToken });
        }

        public static Task<AuthMe> GetMe()
        {
            return ApiClient.Instance.Get<AuthMe>(AuthRoutes.Me);
        }

        public static Task<IdentitiesList> GetIdentities()
        {
            return ApiClient.Instance.Get<IdentitiesList>(AuthRoutes.Identities);
        }

        public static Task<IdentityResponse> LinkGoogle(string idToken)
        {
            return ApiClient.Instance.Post<IdentityResponse>(
                AuthRoutes.LinkGoogle,
                new GoogleLoginBody { IdToken = idToken, Device = CurrentDevice() });
        }

        public static Task<OtpRequestResponse> LinkMobileStart(string mobile, string channel = AuthChannels.Sms)
        {
            return ApiClient.Instance.Post<OtpRequestResponse>(
                AuthRoutes.LinkMobileStart,
                new LinkMobileStartBody { Mobile = mobile, Channel = channel });
        }

        public static Task<IdentityResponse> LinkMobileVerify(string challengeId, string code)
        {
            return ApiClient.Instance.Post<IdentityResponse>(
                AuthRoutes.LinkMobileVerify,
                new LinkMobileVerifyBody { ChallengeId = challengeId, Code = code });
        }

        public static Task<object> UnlinkIdentity(string provider)
        {
            return ApiClient.Instance.Delete<object>(AuthRoutes.Unlink(provider));
        }

        // The device block is optional on the server, but supplying it lets the backend
        // attach a session to a physical device for push and anomaly detection.
        public static DeviceInfo CurrentDevice() => new DeviceInfo
        {
            DeviceId = SystemInfo.deviceUniqueIdentifier,
            Platform = "android", // NormalizedPlatform() is more accurate but causes issues with some gateway configs. Hardcode to avoid.
            Model = SystemInfo.deviceModel,
            OsVersion = SystemInfo.operatingSystem,
            AppVersion = Application.version
        };

        static string NormalizedPlatform()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.Android: return "android";
                case RuntimePlatform.IPhonePlayer: return "ios";
                case RuntimePlatform.WebGLPlayer: return "web";
                default: return Application.platform.ToString().ToLowerInvariant();
            }
        }
    }
}
