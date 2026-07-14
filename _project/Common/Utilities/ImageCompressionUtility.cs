using System;
using System.IO;
using UnityEngine;

namespace Core.Utilities
{
    public static class ImageCompressionUtility
    {
        public static byte[] CompressAvatar(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
                return null;

            byte[] sourceBytes = File.ReadAllBytes(imagePath);
            if (!IsValidImage(sourceBytes))
                return null;

            Texture2D sourceTex = new Texture2D(2, 2, TextureFormat.RGB24, false);
            if (!sourceTex.LoadImage(sourceBytes))
            {
                UnityEngine.Object.Destroy(sourceTex);
                return null;
            }

            int maxDim = 512;
            int targetWidth = sourceTex.width;
            int targetHeight = sourceTex.height;

            if (targetWidth > maxDim || targetHeight > maxDim)
            {
                float ratio = (float)targetWidth / targetHeight;
                if (targetWidth >= targetHeight)
                {
                    targetWidth = maxDim;
                    targetHeight = Mathf.RoundToInt(maxDim / ratio);
                }
                else
                {
                    targetHeight = maxDim;
                    targetWidth = Mathf.RoundToInt(maxDim * ratio);
                }
            }

            Texture2D textureToEncode = sourceTex;
            if (sourceTex.width != targetWidth || sourceTex.height != targetHeight)
            {
                textureToEncode = ResizeTexture(sourceTex, targetWidth, targetHeight);
                UnityEngine.Object.Destroy(sourceTex);
                sourceTex = null;
            }

            if (textureToEncode == null)
                return null;

            byte[] jpegBytes = null;
            try
            {
                jpegBytes = textureToEncode.EncodeToJPG(75);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ImageCompressionUtility] JPEG encode failed: {ex.Message}");
            }
            finally
            {
                if (textureToEncode != null)
                    UnityEngine.Object.Destroy(textureToEncode);
            }

            if (jpegBytes != null && jpegBytes.Length > 0)
                Resources.UnloadUnusedAssets();

            return jpegBytes;
        }

        public static byte[] CompressTicketAttachment(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
                return null;

            byte[] sourceBytes = File.ReadAllBytes(imagePath);
            if (!IsValidImage(sourceBytes))
                return null;

            Texture2D sourceTex = new Texture2D(2, 2, TextureFormat.RGB24, false);
            if (!sourceTex.LoadImage(sourceBytes))
            {
                UnityEngine.Object.Destroy(sourceTex);
                return null;
            }

            int maxWidth = 1280;
            int targetWidth = sourceTex.width;
            int targetHeight = sourceTex.height;

            if (targetWidth > maxWidth)
            {
                float ratio = (float)targetHeight / targetWidth;
                targetWidth = maxWidth;
                targetHeight = Mathf.RoundToInt(maxWidth * ratio);
            }

            Texture2D textureToEncode = sourceTex;
            if (sourceTex.width != targetWidth || sourceTex.height != targetHeight)
            {
                textureToEncode = ResizeTexture(sourceTex, targetWidth, targetHeight);
                UnityEngine.Object.Destroy(sourceTex);
                sourceTex = null;
            }

            if (textureToEncode == null)
                return null;

            byte[] jpegBytes = null;
            try
            {
                jpegBytes = textureToEncode.EncodeToJPG(80);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ImageCompressionUtility] JPEG encode failed: {ex.Message}");
            }
            finally
            {
                if (textureToEncode != null)
                    UnityEngine.Object.Destroy(textureToEncode);
            }

            if (jpegBytes != null && jpegBytes.Length > 0)
                Resources.UnloadUnusedAssets();

            return jpegBytes;
        }

        private static Texture2D ResizeTexture(Texture2D source, int targetWidth, int targetHeight)
        {
            if (source == null)
                return null;

            RenderTexture rt = new RenderTexture(targetWidth, targetHeight, 0);
            rt.filterMode = FilterMode.Bilinear;

            RenderTexture.active = rt;
            Graphics.Blit(source, rt);

            Texture2D result = new Texture2D(targetWidth, targetHeight, TextureFormat.RGB24, false);
            result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
            result.Apply();

            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);

            return result;
        }

        private static bool IsValidImage(byte[] bytes)
        {
            if (bytes == null || bytes.Length < 4)
                return false;

            if (bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
                return true;
            if (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
                return true;

            return false;
        }

        public static bool IsSupportedImage(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return false;

            try
            {
                byte[] header = new byte[Math.Min(12, new FileInfo(filePath).Length)];
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    fs.Read(header, 0, header.Length);
                }
                return IsValidImage(header);
            }
            catch
            {
                return false;
            }
        }
    }
}
