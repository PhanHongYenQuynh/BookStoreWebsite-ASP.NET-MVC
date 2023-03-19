using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PayPal.Api;

namespace BookGrotto.Models
{
    //Get configuration from web.config file
    public class PaypalConfiguration
    {
        public readonly static string ClientId;
        public readonly static string ClientSecret;
        static PaypalConfiguration()
        {
            ClientId = "Abd5z4xNuWolufuFoX9n4vz0oxRV6na9uEwZGD6OaRNnBw1Nsdm03s8EfwECsjH9L2Z7WHLyMBmhNWkM";
            ClientSecret = "EO1luVGGCLtzRLk9RfIWpHWr5iRh0noAPGfTJLCt1wa_QmiWY2ReakRBw2o9lXUg7-wxoryS8oE_q92I";
        }

        public static Dictionary<string, string> GetConfig() 
        {
            return PayPal.Api.ConfigManager.Instance.GetProperties();
        }

        //Create access token
        private static string GetAccessToken()
        {
            string accessToken = new OAuthTokenCredential(ClientId, ClientSecret, GetConfig()).GetAccessToken();
            return accessToken;
        }

        //This will return APIContext object
        public static APIContext GetAPIContext() 
        {
            var apiContext = new APIContext(GetAccessToken());
            apiContext.Config = GetConfig();
            return apiContext;
        }
    }
}