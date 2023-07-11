using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Fcl.Net.Core;
using Fcl.Net.Core.Interfaces;
using Fcl.Net.Core.Models;
using Fcl.Net.Core.Service;
using Fcl.Net.Core.Service.Strategies;
using UnityEngine;

namespace DapperLabs.Flow.Sdk.Fcl
{
    public class UnityHttpPostStrategy : HttpPostStrategy
    {
        public UnityHttpPostStrategy(FetchService fetchService, Dictionary<FclServiceMethod, ILocalView> localViews) : base(fetchService, localViews)
        {
            
        }

        public override async Task<T> PollAsync<T>(T response)
        {
            if (response is FclAuthResponse fclAuthResponse)
            {
                if (fclAuthResponse.Local == null)
                    throw new Exception("Local was null.");

                var url = FetchService.BuildUrl(fclAuthResponse.Local);
                Debug.Log($"url: {url.AbsoluteUri}");

                UnityThreadExecutor.ExecuteInUpdate(() =>
                {
                    Application.OpenURL(url.AbsoluteUri);
                });

                await Poller(fclAuthResponse);
                Debug.Log("FetchAndReadResponseAsync");
                return await FetchService.FetchAndReadResponseAsync<T>(fclAuthResponse.Updates ?? fclAuthResponse.AuthorizationUpdates, httpMethod: HttpMethod.Get).ConfigureAwait(false);
            }

            return response;
        }

        private async Task<bool> Poller(FclAuthResponse fclAuthResponse)
        {
            var delayMs = 1000;
            var timeoutMs = 300000;
            var startTime = DateTime.UtcNow;

            while (true)
            {
                Debug.Log("Polling...");
                var pollingResponse = await FetchService.FetchAndReadResponseAsync<FclAuthResponse>(fclAuthResponse.Updates ?? fclAuthResponse.AuthorizationUpdates, httpMethod: HttpMethod.Get).ConfigureAwait(false);

                if (pollingResponse.Status == ResponseStatus.Approved || pollingResponse.Status == ResponseStatus.Declined)
                {
                    Debug.Log($"Status is {pollingResponse.Status}");
                    //await RedirectToApp();
                    return true;
                }

                if (DateTime.UtcNow.Subtract(startTime).TotalMilliseconds > timeoutMs)
                    throw new Exception("Timed out polling.");

                await Task.Delay(delayMs).ConfigureAwait(false);
            }
        }
    }
}
