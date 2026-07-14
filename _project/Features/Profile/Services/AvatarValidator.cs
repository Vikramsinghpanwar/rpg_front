using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Core.Utilities;

namespace Features.Profile.Services
{
    public static class AvatarValidator
    {
        const long MaxBytes = 5 * 1024 * 1024;
        static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png" };

        public static bool IsValid(string filePath, out string error)
        {
            error = null;

            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                error = "File not found.";
                return false;
            }

            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            if (Array.IndexOf(AllowedExtensions, ext) < 0)
            {
                error = "Invalid format. Please select a PNG or JPG image under 5MB.";
                return false;
            }

            var info = new FileInfo(filePath);
            if (info.Length > MaxBytes)
            {
                error = "File too large. Please select a PNG or JPG image under 5MB.";
                return false;
            }

            if (!ImageCompressionUtility.IsSupportedImage(filePath))
            {
                error = "Invalid or corrupted image file. Please select a valid PNG or JPG.";
                return false;
            }

            return true;
        }
    }
}
