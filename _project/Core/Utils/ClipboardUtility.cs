using System.Text.RegularExpressions;
using UnityEngine;

namespace Core.Utils
{
    public static class ClipboardUtility
    {
        public static string GetClipboardText()
        {
            if (GUIUtility.systemCopyBuffer == null)
                return null;

            return GUIUtility.systemCopyBuffer;
        }

        public static bool IsValidPromoCode(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            text = text.Trim();

            return Regex.IsMatch(text, @"^REF-[A-Za-z0-9]{10}$");
        }
    }
}
