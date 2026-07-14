using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Features.Profile.Services
{
    public static class AvatarLoader
    {
        public static async Task<Sprite> LoadRemoteAsync(string avatarUrl)
        {
            if (string.IsNullOrEmpty(avatarUrl))
                return null;

            try
            {
                var request = UnityWebRequestTexture.GetTexture(avatarUrl);
                request.timeout = 30;
                var operation = request.SendWebRequest();
                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"[AvatarLoader] Download failed for {avatarUrl}: {request.error}");
                    return null;
                }

                var texture = DownloadHandlerTexture.GetContent(request);
                if (texture == null)
                {
                    Debug.LogWarning($"[AvatarLoader] Texture is null for {avatarUrl}");
                    return null;
                }

                var sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f));

                return sprite;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[AvatarLoader] Exception loading {avatarUrl}: {ex.Message}");
                return null;
            }
        }

        public static async Task<Sprite> LoadLocalAsync(string filePath, int maxSize = 512)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return null;

            try
            {
                var texture = NativeGallery.LoadImageAtPath(filePath, maxSize, false);
                if (texture == null)
                {
                    Debug.LogWarning($"[AvatarLoader] Failed to load local texture from {filePath}");
                    return null;
                }

                var sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f));

                return sprite;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[AvatarLoader] Exception loading local {filePath}: {ex.Message}");
                return null;
            }
        }
    }
}
