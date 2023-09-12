using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using DapperLabs.Flow.Sdk.Unity;
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
            try
            {
                if (response is FclAuthResponse fclAuthResponse)
                {
                    if (fclAuthResponse.Local == null)
                    {
                        throw new Exception("Fcl: HttpPostStrategy: fclAuthResponse.Local was null.");
                    }

                    var url = FetchService.BuildUrl(fclAuthResponse.Local);

                    UnityThreadExecutor.ExecuteInUpdate(() =>
                    {
                        Application.OpenURL(url.AbsoluteUri);
                    });

                    await Poller(fclAuthResponse);
                    
                    return await FetchService.FetchAndReadResponseAsync<T>(fclAuthResponse.Updates ?? fclAuthResponse.AuthorizationUpdates, httpMethod: HttpMethod.Get).ConfigureAwait(false);
                }

                throw new Exception("Fcl: HttpPostStrategy: response type is not FclAuthResponse");
            }
            catch (Exception ex)
            {
                throw new Exception($"Fcl: HttpPostStrategy: {ex.Message}", ex);
            }
        }

        private async Task<bool> Poller(FclAuthResponse fclAuthResponse)
        {
            var delayMs = 1000;
            var timeoutMs = 300000;
            var startTime = DateTime.UtcNow;

            while (true)
            {
                try
                {
                    var pollingResponse = await FetchService.FetchAndReadResponseAsync<FclAuthResponse>(fclAuthResponse.Updates ?? fclAuthResponse.AuthorizationUpdates, httpMethod: HttpMethod.Get).ConfigureAwait(false);

                    if (pollingResponse.Status == ResponseStatus.Approved || pollingResponse.Status == ResponseStatus.Declined)
                    {
                        Debug.Log($"Fcl: HttpPostStrategy: Status is {pollingResponse.Status}");
                        return true;
                    }

                    if (DateTime.UtcNow.Subtract(startTime).TotalMilliseconds > timeoutMs)
                    {
                        throw new Exception("Fcl: HttpPostStrategy: Timed out polling.");
                    }

                    await Task.Delay(delayMs).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Fcl: HttpPostStrategy: {ex.Message}", ex);
                }
            }
        }
    }
}
