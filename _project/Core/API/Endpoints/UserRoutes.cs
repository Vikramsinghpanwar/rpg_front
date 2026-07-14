namespace Core.API.Endpoints
{
    public static class UserRoutes
    {
        public static readonly string Base = $"{ApiRoutes.V1}/users";

        public static string ById(string userId) => $"{Base}/{userId}";
        public static readonly string Me = $"{Base}/me";

        public static readonly string UploadAvatar = $"{ApiRoutes.V1}/media/upload";
    }
}
