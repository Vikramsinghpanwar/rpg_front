using System;
using System.Collections.Generic;

namespace Core.API
{
    // Builder for the Idempotency-Key header. The header NAME lives in ApiHeaders; this
    // class generates a fresh UUIDv4 and pairs it with the dict shape ApiClient expects.
    public static class IdempotencyHeader
    {
        public static (string key, Dictionary<string, string> headers) New()
        {
            var key = Guid.NewGuid().ToString();
            return (key, ApiHeaders.WithIdempotency(key));
        }

        public static Dictionary<string, string> For(string key)
            => ApiHeaders.WithIdempotency(key);
    }
}
