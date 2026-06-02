namespace Core.API.Endpoints
{
    // /api/v1/player/auth/* — see USER_API_CONTRACTS.md > Player Auth.
    public static class AuthRoutes
    {
        public static readonly string Base = $"{ApiRoutes.V1}/player/auth";

        public static readonly string Signup        = $"{Base}/signup";
        public static readonly string LoginGoogle   = $"{Base}/login/google";
        public static readonly string LoginOtpReq   = $"{Base}/login/otp/request";
        public static readonly string LoginOtpVerify= $"{Base}/login/otp/verify";
        public static readonly string Refresh       = $"{Base}/refresh";
        public static readonly string Logout        = $"{Base}/logout";
        public static readonly string Me            = $"{Base}/me";
        public static readonly string Identities    = $"{Base}/identities";
        public static readonly string LinkGoogle    = $"{Base}/link/google";
        public static readonly string LinkMobileStart  = $"{Base}/link/mobile/start";
        public static readonly string LinkMobileVerify = $"{Base}/link/mobile/verify";

        public static string Unlink(string provider) => $"{Base}/link/{provider}";
    }
}
