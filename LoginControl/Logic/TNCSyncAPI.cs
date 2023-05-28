using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Security;

namespace LoginControl.Logic
{
    public class TNCSAPIHelpers
    {

        public string EmailController { get; set; }

        public HttpClient Client;
        public static TNCSAPIHelpers Instance = new TNCSAPIHelpers();
        private TNCSAPIHelpers()
        {

        }
    }

    public class TNCSAPI
    {
        #region Common Properties
        private const string URLEmail = "{0}/IsAvailable?id={1}";
        private const string URLEmailPass = "{0}/HasPassword?id={1}";
        #endregion

        #region Common Method
        public HttpResponseMessage TNCSGET(string request)
        {
            return TNCSAPIHelpers.Instance.Client.GetAsync(request).Result;
        }
        public HttpResponseMessage TNCSPOST(string request, HttpContent postContent)
        {
            return TNCSAPIHelpers.Instance.Client.PostAsync(request, postContent).Result;
        }
        #endregion

        #region EmailMethods
        public async Task<bool> IsEmailRegistered(string EmailId)
        {
            bool EmailidExists = false;
            string requestString = string.Format(URLEmail, TNCSAPIHelpers.Instance.EmailController, EmailId);
            var response = TNCSGET(requestString);
            string result = "0";
            if(response.IsSuccessStatusCode == true)
            {
                result = await response.Content.ReadAsStringAsync();
                if (result != "0") EmailidExists = true;  // 1 for email id already registered
            }
            if (EmailidExists == false) return false;
            return await EmailHasPassword(int.Parse(result));
        }

        private async Task<bool> EmailHasPassword(int UserID)
        {
            string requestString = string.Format(URLEmailPass, TNCSAPIHelpers.Instance.EmailController, UserID);
            var response = TNCSGET(requestString);
            string result = "0";
            if(response.IsSuccessStatusCode == true)
            {
                result = await response.Content.ReadAsStringAsync();
            }
            if (result != "0") return true;
            return false;
        }
        #endregion
    }
}


