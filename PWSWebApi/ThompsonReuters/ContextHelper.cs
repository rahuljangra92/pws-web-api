using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using DataScope.Select.Api.EvaluationConnect;
using DataScope.Select.Api.Authentication;
using DataScope.Select.Api.Extractions;
using DataScope.Select.Api.Jobs;
using DataScope.Select.Api.Quota;
using DataScope.Select.Api.Search;
using DataScope.Select.Api.StandardExtractions;
using DataScope.Select.Api.Users;

namespace PWSWebApi.ThompsonReuters
{
    public class ContextHelper
    {
        private static string _token;

        public static void ClearToken()
        {
            _token = null;
        }

        static string userId = "9025709";
        static string password = "$PwS20141231!";
        //static string serviceUrl = @"https://hosted.datascopeapi.reuters.com/RestApi/v1/";
        static string serviceUrl = @"https://selectapi.datascope.refinitiv.com/RestApi/v1/";

        public static JobsContext CreateJobsContext()
        {
            var serverUri = new Uri(serviceUrl);
            var creds = new NetworkCredential(userId, password);

            return new JobsContext(serverUri, _token, () => creds, token => _token = token);
        }

        public static JobsContext CreateAskTrpsContext()
        {
            var serverUri = new Uri(serviceUrl);
            var creds = new NetworkCredential(userId, password);

            //NOTE: Sample Mode is set to true for the AskTrpsContext because without this the example application would be
            //submitting actual challenges for which evaluators will be responding.
            return new JobsContext(serverUri, _token, () => creds, token => _token = token, true);
        }

        //
        public static ExtractionsContext CreateExtractionsContext()
        {
            var serverUri = new Uri(serviceUrl);
            var creds = new NetworkCredential(userId, password);

            return new ExtractionsContext(serverUri, _token, () => creds, token => _token = token);
        }

        public static StandardExtractionsContext CreateStandardExtractionsContext()
        {
            var serverUri = new Uri(serviceUrl);
            var creds = new NetworkCredential(userId, password);

            return new StandardExtractionsContext(serverUri, _token, () => creds, token => _token = token);
        }

        public static SearchContext CreateSearchContext()
        {
            var serverUri = new Uri(serviceUrl);
            var creds = new NetworkCredential(userId, password);

            return new SearchContext(serverUri, _token, () => creds, token => _token = token);
        }

        public static QuotaContext CreateQuotaContext()
        {
            var serverUri = new Uri(serviceUrl);
            var creds = new NetworkCredential(userId, password);

            return new QuotaContext(serverUri, _token, () => creds, token => _token = token, true);
        }

        public static UsersContext CreateUsersContext(bool authenticate = true)
        {
            var serverUri = new Uri(serviceUrl);
            var creds = new NetworkCredential(userId, password);

            var context = new UsersContext(serverUri, _token, () => creds, token => _token = token);

            if (authenticate)
                _token = context.SessionToken;

            return context;
        }

        public static AuthenticationContext CreateAuthenticationContext(bool authenticate = true)
        {
            var serverUri = new Uri(serviceUrl);
            var creds = new NetworkCredential(userId, password);

            var context = new AuthenticationContext(serverUri, _token, () => creds, token => _token = token);

            if (authenticate)
                _token = context.SessionToken;

            return context;
        }

    }
}