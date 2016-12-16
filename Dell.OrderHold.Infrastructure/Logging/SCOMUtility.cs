using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dell.OrderHold.Infrastructure.Logging
{
    public class SCOMUtility
    {
        public int GetMessageId(SCOMLogTypes type)
        {
            return (int)type;
        }

        public string BuildMessage(string smallMessage, string ipAddress, string environment, string url, string className, string methodName, string correlationId, string verb, params KeyValuePair<string, string>[] properties)
        {
            if (string.IsNullOrWhiteSpace(smallMessage))
                smallMessage = "";
            if (string.IsNullOrWhiteSpace(ipAddress))
                ipAddress = "";
            if (string.IsNullOrWhiteSpace(environment))
                environment = "";
            if (string.IsNullOrWhiteSpace(url))
                url = "";
            if (string.IsNullOrWhiteSpace(className))
                className = "";
            if (string.IsNullOrWhiteSpace(methodName))
                methodName = "";
            if (string.IsNullOrWhiteSpace(correlationId))
                correlationId = "";
            if (string.IsNullOrWhiteSpace(verb))
                verb = "";

            string appDomainName = "";
            string identityName = "";
            string processId = "";
            string processName = "";
            string threadName = "";
            string threadId = "";

            if (AppDomain.CurrentDomain != null && !string.IsNullOrEmpty(AppDomain.CurrentDomain.FriendlyName))
                appDomainName = AppDomain.CurrentDomain.FriendlyName;
            if (AppDomain.CurrentDomain != null && AppDomain.CurrentDomain.ApplicationIdentity != null && !string.IsNullOrWhiteSpace(AppDomain.CurrentDomain.ApplicationIdentity.FullName))
                identityName = AppDomain.CurrentDomain.ApplicationIdentity.FullName;

            var currentProcess = Process.GetCurrentProcess();
            if (currentProcess != null)
                processId = currentProcess.Id.ToString();

            if (currentProcess != null && !string.IsNullOrWhiteSpace(currentProcess.ProcessName))
                processName = currentProcess.ProcessName;

            if (System.Threading.Thread.CurrentThread != null && !string.IsNullOrWhiteSpace(System.Threading.Thread.CurrentThread.Name))
                threadName = System.Threading.Thread.CurrentThread.Name;

            if (System.Threading.Thread.CurrentThread != null)
                threadId = System.Threading.Thread.CurrentThread.ManagedThreadId.ToString();

            var msg = @"Message:" + smallMessage.Replace('\n', ' ') + @"
CorrelationId:" + correlationId + @"
IP Address:" + ipAddress + @"
Environment:" + environment + @"
Timestamp:" + DateTime.UtcNow.ToLongDateString() + " " + DateTime.UtcNow.ToLongTimeString() + @"
URL:" + verb + " " + url + @"
Class:" + className + @"
Method:" + methodName + @"
AppDomain:" + appDomainName + @"
Identity:" + identityName + @"
ProcessId:" + processId + @"
ProcessName:" + processName + @"
ThreadName:" + threadName + @"
ThreadId:" + threadId;

            if (properties != null && properties.Any())
            {
                msg += "\nproperties: ";
                try
                {
                    msg += JsonConvert.SerializeObject(properties);
                }
                catch { }
            }

            return msg;
        }
    }

    /// <summary>
    /// No need to use this right now, we are re-evaluating how/if we use SCOM.
    /// 
    /// 2000 - Web Api
    /// 3000 - Bounded Context Category
    /// 4000 - REST Proxy
    /// 5000 - WCF Proxy
    /// 6000 - Configuration
    /// 7000 - Security Category
    /// 8000 - Caching
    /// 9000 - Enterprise Services
    /// </summary>
    public enum SCOMLogTypes
    {
        RouteNotFound = 2501,
        DbCommandExecuted = 3000,
        AddOrUpdateFailed = 3500,
        DeleteFailed = 3501,
        QueryFailed = 3502,
        ObjectNotFound = 3503,
        CartCreationFailure = 3600,
        CartUpdationFailure = 3601,
        CatalogGetDetailsFailureInDSA = 3700,
        CfoSendQuoteFailure = 3621,
        CfoGetPreviewFailure = 3622,
        CfoResendFailure = 3623,
        DAMDataWarningForAccount = 3800,
        DAMDataWarningForAccountGroup = 3801,
        GoalDataWarningForAccount = 3802,
        GetContractFailed = 3803,
        CalculateSpecialPricing = 3804,
        DomainContextRequestExceptions = 4500,
        BootstrapperStarted = 6000,
        BootstrapperCompleted = 6001,
        BootstrapperStartingService = 6002,
        BootstrapperServicesStarted = 6003,
        BootstrapperFailure = 6500,
        RightRequested = 7000,
        RightDemanded = 7001,
        RightRequestFailed = 7002,
        PermissionRequested = 7003,
        PermissionDemanded = 7004,
        ClaimsBuildersLoaded = 7005,
        ClaimsTokenCreated = 7006,
        RightDemandFailed = 7500,
        PermissionDemandFailed = 7501,
        ClaimsTokenCacheFailure = 7502,
        AmbiguousCustomerContext = 7503,
        RequestNotAuthenticated = 7600,
        UserHasNoRoles = 7601,
        DirectoryReferralError = 7200,
        CacheFailure = 8000,
        DCStoreEfValidationErrors = 8100,
        ServiceFailure = 9501,
        ServiceExceededExpectedResponseTime = 9502,
        ServiceMissingOrInvalidData = 9503,
        ServiceConnectivityError = 9504,
        ServiceCommunicationError = 9505,
        ServiceSecurityError = 9506,
        ServiceAuthenticationError = 9507
    }

    public static class ScomExtensions
    {
        public static string ToLogMessage(this SCOMLogTypes type)
        {
            return ((int)type).ToString();
        }
    }
}
