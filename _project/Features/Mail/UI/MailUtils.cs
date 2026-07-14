using System.Collections.Generic;

namespace Features.Mail.UI
{
    public static class MailUtils
    {
        public static bool HasAttachment(object attachment)
        {
            if (attachment == null) return false;

            if (attachment is Newtonsoft.Json.Linq.JArray array && array.Count > 0)
            {
                return true;
            }

            if (attachment is System.Collections.ICollection collection)
            {
                return collection.Count > 0;
            }

            return false;
        }

        public static string FormatAttachmentCount(object attachment)
        {
            if (attachment == null) return "0";

            int count = 0;

            if (attachment is Newtonsoft.Json.Linq.JArray array)
            {
                count = array.Count;
            }
            else if (attachment is System.Collections.ICollection collection)
            {
                count = collection.Count;
            }
            else if (attachment is System.Collections.IEnumerable enumerable)
            {
                foreach (var _ in enumerable)
                {
                    count++;
                }
            }

            return count.ToString();
        }
    }
}
