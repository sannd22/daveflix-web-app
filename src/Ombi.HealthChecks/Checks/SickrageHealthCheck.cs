﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Ombi.Api.SickRage;
using Ombi.Core.Settings;
using Ombi.Settings.Settings.Models.External;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ombi.HealthChecks.Checks
{
    public class SickrageHealthCheck : BaseHealthCheck
    {
        public SickrageHealthCheck(IServiceScopeFactory serviceScopeFactory) : base(serviceScopeFactory)
        {
        }
        public override async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            using (var scope = CreateScope())
            {
                var settingsProvider = scope.ServiceProvider.GetRequiredService<ISettingsService<SickRageSettings>>();
                var api = scope.ServiceProvider.GetRequiredService<ISickRageApi>();
                var settings = await settingsProvider.GetSettingsAsync();
                if (!settings.Enabled)
                {
                    return HealthCheckResult.Healthy("SickRage is not configured.");
                }

                try
                {
                    var result = await api.Ping(settings.ApiKey, settings.FullUri);
                    if (result != null)
                    {
                        return HealthCheckResult.Healthy();
                    }
                    return HealthCheckResult.Degraded("Couldn't get the status from SickRage");
                }
                catch (Exception e)
                {
                    return HealthCheckResult.Unhealthy("Could not communicate with SickRage", e);
                }
            }
        }
    }
}
