using System;
using System.Threading.Tasks;
using Core.API;
using Core.API.Endpoints;
using Newtonsoft.Json;

namespace Core.VersionControl
{
    public class VersionService
    {
        public static VersionService Instance { get; } = new VersionService();

        VersionCheckRequest defaultRequest;

        public bool IsInitialized { get; private set; }

        VersionService() { }

        public void Initialize(VersionCheckRequest defaultReq)
        {
            defaultRequest = defaultReq;
            IsInitialized = true;
        }

        public async Task<(VersionCheckResponse response, string error)> CheckAsync(VersionCheckRequest overrideReq = null)
        {
            if (!IsInitialized || defaultRequest == null)
                return (null, "VersionService not initialized");

            var req = overrideReq ?? defaultRequest;
            var reqLogId = Guid.NewGuid().ToString("N");
            var start = DateTime.UtcNow;

            try
            {
                VersionLog.Request(reqLogId, req);
                var resp = await ApiClient.Instance.PostAsync<VersionCheckResponse>(
                    CommonRoutes.VersionCheck, req);

                if (resp == null)
                {
                    VersionLog.Response(reqLogId, 0, "null", start);
                    return (null, "empty_response");
                }

                VersionLog.Response(reqLogId, 0, JsonConvert.SerializeObject(resp), start);
                return (resp, null);
            }
            catch (ApiException ex)
            {
                VersionLog.Response(reqLogId, ex.StatusCode, ex.Message, start);
                VersionLog.Failed(reqLogId, req, ex.Message, ex.StatusCode, start);
                return (null, $"httperror:{ex.StatusCode}:{ex.Message}");
            }
            catch (Exception ex)
            {
                VersionLog.Response(reqLogId, 0, ex.Message, start);
                VersionLog.Failed(reqLogId, req, ex.Message, 0, start);
                return (null, ex.Message);
            }
        }
    }
}
