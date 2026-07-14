using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Features.Notice.Services
{
    public static class NoticeImageCache
    {
        static readonly string CacheRoot;
        static readonly string DefaultPlaceholderPath;

        static NoticeImageCache()
        {
            CacheRoot = Path.Combine(Application.persistentDataPath, "NoticeCache");
            DefaultPlaceholderPath = Path.Combine(Application.streamingAssetsPath, "notice_placeholder.png");
            if (!Directory.Exists(CacheRoot))
            {
                Directory.CreateDirectory(CacheRoot);
            }
        }

        public static Task<Sprite> GetImageAsync(string noticeId, string imageUrl)
        {
            if (string.IsNullOrEmpty(noticeId) || string.IsNullOrEmpty(imageUrl))
            {
                return Task.FromResult(LoadPlaceholder());
            }

            string fileName = BuildFileName(noticeId, imageUrl);
            string localPath = Path.Combine(CacheRoot, fileName);

            if (File.Exists(localPath))
            {
                try
                {
                    var sprite = LoadSpriteFromPath(localPath);
                    if (sprite != null)
                    {
                        return Task.FromResult(sprite);
                    }
                }
                catch
                {
                    File.Delete(localPath);
                }
            }

            return DownloadAndCacheAsync(noticeId, imageUrl, localPath);
        }

        static async Task<Sprite> DownloadAndCacheAsync(string noticeId, string imageUrl, string localPath)
        {
            try
            {

                using (var request = UnityWebRequestTexture.GetTexture(imageUrl))
                {
                    request.timeout = 30;
                    var operation = request.SendWebRequest();
                    while (!operation.isDone)
                    {
                        await Task.Yield();
                    }

                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogWarning($"[NoticeImageCache] Download failed for {imageUrl}: {request.error}");
                        return LoadPlaceholder();
                    }

                    var texture = DownloadHandlerTexture.GetContent(request);
                    var bytes = texture.EncodeToPNG();
                    File.WriteAllBytes(localPath, bytes);

                    var sprite = BuildSpriteFromTexture(texture);
                    UnityEngine.Object.Destroy(texture);
                    return sprite;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[NoticeImageCache] Exception downloading {imageUrl}: {ex.Message}");
                return LoadPlaceholder();
            }
        }

        public static Sprite LoadPlaceholder()
        {
            if (File.Exists(DefaultPlaceholderPath))
            {
                try
                {
                    return LoadSpriteFromPath(DefaultPlaceholderPath);
                }
                catch { /* ignored */ }
            }

            var fallbackTex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            fallbackTex.SetPixel(0, 0, new Color(0.3f, 0.3f, 0.3f, 1f));
            fallbackTex.Apply();

            return Sprite.Create(
                fallbackTex,
                new Rect(0, 0, fallbackTex.width, fallbackTex.height),
                new Vector2(0.5f, 0.5f));
        }

        static string BuildFileName(string noticeId, string imageUrl)
        {
            string raw = $"{noticeId}_{imageUrl}";
            var bytes = Encoding.UTF8.GetBytes(raw);

            using (var sha = SHA256.Create())
            {
                var hash = sha.ComputeHash(bytes);
                var sb = new StringBuilder();
                foreach (var b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }
                return $"{noticeId}_{sb}.png";
            }
        }

        static Sprite LoadSpriteFromPath(string path)
        {
            byte[] data = File.ReadAllBytes(path);
            var texture = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            texture.filterMode = FilterMode.Bilinear;
            texture.LoadImage(data);
            return BuildSpriteFromTexture(texture);
        }

        static Sprite BuildSpriteFromTexture(Texture2D texture)
        {
            return Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f));
        }
    }
}
