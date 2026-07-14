using System;
using System.Threading.Tasks;
using Core.API;
using Core.API.Endpoints;
using Core.Bootstrap;
using Core.Models;
using Core.Services;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Features.Profile.Services
{
    public static class ProfileService
    {
        public static string GetCurrentUserId()
        {
            return BootstrapService.Instance?.Profile?.public_id
                   ?? TokenProvider.Instance?.GetUserId();
        }

        public static Task<BootstrapProfile> UpdateProfileAsync(string username, string avatar)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                Debug.LogWarning("[ProfileService] Cannot update profile: no user id available");
                return Task.FromResult<BootstrapProfile>(null);
            }

            string updatedUsername = string.IsNullOrWhiteSpace(username) ? null : username.Trim();
            string updatedAvatar = string.IsNullOrWhiteSpace(avatar) ? null : avatar.Trim();

            if (string.IsNullOrEmpty(updatedUsername) && string.IsNullOrEmpty(updatedAvatar))
                return Task.FromResult<BootstrapProfile>(null);

            var body = new UpdateUserRequest
            {
                Username = updatedUsername,
                Avatar = updatedAvatar
            };

            return ApiClient.Instance.Put<BootstrapProfile>(
                UserRoutes.Me,
                body);
        }

        public static async Task<AvatarUploadResponse> UploadAvatarAsync(byte[] imageBytes, string fileName, string avatarUrl = null)
        {
            if (imageBytes == null || imageBytes.Length == 0)
                throw new ArgumentException("imageBytes is empty", nameof(imageBytes));

            if (string.IsNullOrEmpty(fileName))
                fileName = "avatar.jpg";

            string mimeType = GetMimeType(fileName);

            var extraFields = new System.Collections.Generic.Dictionary<string, string>();
            extraFields["category"] = "PROFILE_IMAGE";
            var currentUserId = GetCurrentUserId();
            if (!string.IsNullOrEmpty(currentUserId))
                extraFields["entityId"] = currentUserId;
            if (!string.IsNullOrEmpty(avatarUrl))
                extraFields["avatar"] = avatarUrl;

            var resp = await ApiClient.Instance.PostMultipart<AvatarUploadResponse>(
                UserRoutes.UploadAvatar,
                "file",
                imageBytes,
                fileName,
                mimeType,
                extraFields,
                null);

            if (resp?.Data == null || string.IsNullOrEmpty(resp.Data.PublicUrl))
                return null;

            return new AvatarUploadResponse { Data = new AvatarUploadData { PublicUrl = resp.Data.PublicUrl, Id = resp.Data.Id } };
        }

        static string GetMimeType(string fileName)
        {
            var ext = fileName.ToLowerInvariant();
            if (ext.EndsWith(".png")) return "image/png";
            if (ext.EndsWith(".jpg") || ext.EndsWith(".jpeg")) return "image/jpeg";
            return "application/octet-stream";
        }
    }
}
