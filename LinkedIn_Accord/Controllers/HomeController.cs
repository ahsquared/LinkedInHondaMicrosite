using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using Newtonsoft.Json.Linq;

using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.IO;

namespace LinkedIn_Accord.Controllers
{
    public class HomeController : Controller
    {
        private LinkedIn_Accord.Models.LinkedIn_HondaEntities db = new Models.LinkedIn_HondaEntities();

        public ActionResult Index()
        {
            ViewBag.Message = "";

            return View();
        }
        public ActionResult PointDetail()
        {
            ViewBag.Message = "";

            return View();
        }

        public JsonResult GetTopThree()
        {
            LinkedIn_Accord.Models.LinkedIn_HondaEntities db = new Models.LinkedIn_HondaEntities();
            var entries = db.Entries.Select(x => new { x.Name, x.TotalPoints }).OrderByDescending(x => x.TotalPoints).Take(3);
            return Json(entries, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetTopThreeNetwork()
        {
            JObject credentials = (JObject)this.Session["credentials"];
            string oauth_token = (string)this.Session["oauth_token"];
            string oauth_token_secret = (string) this.Session["oauth_token_secret"];

            oAuthBase2 oauth = new oAuthBase2();
            string normailizedUrl;
            string normalisedRequestParameters;

            string requestUrl = "http://api.linkedin.com/v1/people/~/connections:(id,first-name,last-name)";
            string postData = "format=json";
            string signature = oauth.GenerateSignature(new Uri(requestUrl + "?" + postData), "s0pnibdnyjkv", "QBxVDhANOwR0lGAG", oauth_token, oauth_token_secret, "GET", oauth.GenerateTimeStamp(), oauth.GenerateNonce(), out normailizedUrl, out normalisedRequestParameters);

            requestUrl = normailizedUrl + "?" + normalisedRequestParameters + "&oauth_signature=" + HttpUtility.UrlEncode(signature);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUrl);
            request.Method = "GET";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            StreamReader reader = new StreamReader(response.GetResponseStream());
            string result = reader.ReadToEnd();

            JObject resultJson = JObject.Parse(result);

            JArray values = (JArray) resultJson["values"];
            List<string> networkIds = new List<string>();

            foreach(JObject value in values)
            {
                networkIds.Add((string)value["id"]);
            }

            LinkedIn_Accord.Models.LinkedIn_HondaEntities db = new Models.LinkedIn_HondaEntities();
            var entries = db.Entries.OrderByDescending(x => x.TotalPoints).Where(entity => networkIds.Contains(entity.LinkedInId)).Take(3);
            return Json(entries, JsonRequestBehavior.AllowGet);
        }



        [HttpPost]
        public void Auth()
        {
            // linkedin_oauth_s0pnibdnyjkv cookie contains token. https only.
            string credentialsJson = this.Request.Cookies["linkedin_oauth_s0pnibdnyjkv"].Value;
            credentialsJson = HttpUtility.UrlDecode(credentialsJson);
            JObject credentials = JObject.Parse(credentialsJson);
            this.Session.Add("credentials", credentials);
        }

        [HttpPost]
        public ActionResult Index(Models.Entry entry)
        {
            if (this.Request["PostUpdate"] == "on")
            {
                entry.PostUpdate = true;
            }
            if (this.Request["AgreedTerms"] == "on")
            {
                entry.AgreedTerms = true;
            }
            if (this.Request["Offers"] == "on")
            {
                entry.Offers = true;
            }

            JObject credentials = (JObject)this.Session["credentials"];
            string requestUrl = "https://api.linkedin.com/uas/oauth/accessToken";
            
            oAuthBase2 oauth = new oAuthBase2();
            string normailizedUrl;
            string normalisedRequestParameters;

            string postData = "oauth_consumer_key=s0pnibdnyjkv&xoauth_oauth2_access_token=" + HttpUtility.UrlEncode(credentials["access_token"].ToString()) + "&oauth_signature_method=HMAC-SHA1";
            string signature = oauth.GenerateSignature(new Uri(requestUrl + "?" + postData), "s0pnibdnyjkv", "QBxVDhANOwR0lGAG", (string)credentials["access_token"], null, "GET", oauth.GenerateTimeStamp(), oauth.GenerateNonce(), out normailizedUrl, out normalisedRequestParameters);
            requestUrl = normailizedUrl + "?" + normalisedRequestParameters + "&oauth_signature=" + HttpUtility.UrlEncode(signature);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUrl);
            request.Method = "GET";

            HttpWebResponse response = (HttpWebResponse ) request.GetResponse();

            StreamReader reader = new StreamReader(response.GetResponseStream());
            string result = reader.ReadToEnd();

            string[] results = result.Split("&".ToCharArray());

            string oauth_token = results[0].Split("=".ToCharArray())[1];
            string oauth_token_secret = results[1].Split("=".ToCharArray())[1];

            this.Session["oauth_token"] = oauth_token;
            this.Session["oauth_token_secret"] = oauth_token_secret;

            // look up entry.HighAchieverLinkedInId

            requestUrl = "https://api.linkedin.com/v1/people/id=" + entry.HighAchieverLinkedInId.ToString() + ":(picture-url,positions)";
            postData = "format=json";
            signature = oauth.GenerateSignature(new Uri(requestUrl + "?" + postData), "s0pnibdnyjkv", "QBxVDhANOwR0lGAG", oauth_token, oauth_token_secret, "GET", oauth.GenerateTimeStamp(), oauth.GenerateNonce(), out normailizedUrl, out normalisedRequestParameters);

            requestUrl = normailizedUrl + "?" + normalisedRequestParameters + "&oauth_signature=" + HttpUtility.UrlEncode(signature);

            request = (HttpWebRequest)WebRequest.Create(requestUrl);
            request.Method = "GET";

            response = (HttpWebResponse)request.GetResponse();

            reader = new StreamReader(response.GetResponseStream());
            result = reader.ReadToEnd();

            JObject resultJson = JObject.Parse(result);
            try
            {
                int positons = (int) resultJson["positions"]["values"].Count();
                //int years = 2013 - startYear;
                entry.HighAchieverPoints = (positons * 500) + 25;

            }
            catch
            {
                entry.HighAchieverPoints = 50;
            }
            entry.HighAchieverImageUrl = (string)resultJson["pictureUrl"];

            if (String.IsNullOrEmpty(entry.HighAchieverImageUrl))
            {
                entry.HighAchieverImageUrl = "/Content/img/blank-user-pic.png";
            }

            // look up entry.SocialButterflyLinkedInId

            requestUrl = "https://api.linkedin.com/v1/people/id=" + entry.SocialButterflyLinkedInId.ToString() + ":(picture-url,current-share,current-status-timestamp)";
            postData = "format=json";
            signature = oauth.GenerateSignature(new Uri(requestUrl + "?" + postData), "s0pnibdnyjkv", "QBxVDhANOwR0lGAG", oauth_token, oauth_token_secret, "GET", oauth.GenerateTimeStamp(), oauth.GenerateNonce(), out normailizedUrl, out normalisedRequestParameters);

            requestUrl = normailizedUrl + "?" + normalisedRequestParameters + "&oauth_signature=" + HttpUtility.UrlEncode(signature);

            request = (HttpWebRequest)WebRequest.Create(requestUrl);
            request.Method = "GET";

            response = (HttpWebResponse)request.GetResponse();

            reader = new StreamReader(response.GetResponseStream());
            result = reader.ReadToEnd();

            resultJson = JObject.Parse(result);

            try
            {
                System.DateTime whenShared = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                whenShared = whenShared.AddMilliseconds((long)resultJson["currentStatusTimestamp"]).ToLocalTime();

                TimeSpan span = System.DateTime.Now - whenShared;

                if(whenShared > System.DateTime.Now.AddHours(-24))
                {
                    entry.SocialButterflyPoints = Convert.ToInt32(((1441 - span.TotalMinutes) / 1440) * 500 + 1500);
                }
                else if(whenShared > System.DateTime.Now.AddDays(-30))
                {
                    entry.SocialButterflyPoints = Convert.ToInt32(((31 - span.TotalDays) / 30) * 500 + 1000);
                }
                else if(whenShared > System.DateTime.Now.AddMonths(-12))
                {
                    double months = span.TotalDays / 30;
                    entry.SocialButterflyPoints = Convert.ToInt32(((13 - months) / 12) * 500 + 500);
                }
                else
                {
                    double years = span.TotalDays / 365;
                    entry.SocialButterflyPoints = Convert.ToInt32(((6 - years) / 5) * 500);
                }
                
                // entry.SocialButterflyPoints = (((string)resultJson["currentShare"]["comment"]).Length * 20) + 50;
            }
            catch
            {
                entry.SocialButterflyPoints = 50;
            }

            entry.SocialButterflyImageUrl = (string) resultJson["pictureUrl"];

            if (String.IsNullOrEmpty(entry.SocialButterflyImageUrl))
            {
                entry.SocialButterflyImageUrl = "/Content/img/blank-user-pic.png";
            }

            // look up entry.OriginalThinkerLinkedInId

            requestUrl = "https://api.linkedin.com/v1/people/id=" + entry.OriginalThinkerLinkedInId.ToString() + ":(picture-url,headline)";
            postData = "format=json";
            signature = oauth.GenerateSignature(new Uri(requestUrl + "?" + postData), "s0pnibdnyjkv", "QBxVDhANOwR0lGAG", oauth_token, oauth_token_secret, "GET", oauth.GenerateTimeStamp(), oauth.GenerateNonce(), out normailizedUrl, out normalisedRequestParameters);

            requestUrl = normailizedUrl + "?" + normalisedRequestParameters + "&oauth_signature=" + HttpUtility.UrlEncode(signature);

            request = (HttpWebRequest)WebRequest.Create(requestUrl);
            request.Method = "GET";

            response = (HttpWebResponse)request.GetResponse();

            reader = new StreamReader(response.GetResponseStream());
            result = reader.ReadToEnd();

            resultJson = JObject.Parse(result);

            try
            {
                entry.OriginalThinkerPoints = (((string)resultJson["headline"]).Length * 20) + 50;
            }
            catch
            {
                entry.OriginalThinkerPoints = 50;
            }
            entry.OriginalThinkerImageUrl = (string)resultJson["pictureUrl"];

            if (String.IsNullOrEmpty(entry.OriginalThinkerImageUrl))
            {
                entry.OriginalThinkerImageUrl = "/Content/img/blank-user-pic.png";
            }


            // look up entry.DependableLinkedInId

            requestUrl = "https://api.linkedin.com/v1/people/id=" + entry.DependableLinkedInId.ToString() + ":(picture-url,positions)";
            postData = "format=json";
            signature = oauth.GenerateSignature(new Uri(requestUrl + "?" + postData), "s0pnibdnyjkv", "QBxVDhANOwR0lGAG", oauth_token, oauth_token_secret, "GET", oauth.GenerateTimeStamp(), oauth.GenerateNonce(), out normailizedUrl, out normalisedRequestParameters);

            requestUrl = normailizedUrl + "?" + normalisedRequestParameters + "&oauth_signature=" + HttpUtility.UrlEncode(signature);

            request = (HttpWebRequest)WebRequest.Create(requestUrl);
            request.Method = "GET";

            response = (HttpWebResponse)request.GetResponse();

            reader = new StreamReader(response.GetResponseStream());
            result = reader.ReadToEnd();

            resultJson = JObject.Parse(result);

            try
            {
                int startYear = (int)resultJson["positions"]["values"][0]["startDate"]["year"];
                int years = 2013 - startYear;
                entry.DependablePoints = (years * 75) + 100;

            }
            catch
            {
                entry.DependablePoints = 50;
            }
            entry.DependableImageUrl = (string)resultJson["pictureUrl"];

            if (String.IsNullOrEmpty(entry.DependableImageUrl))
            {
                entry.DependableImageUrl = "/Content/img/blank-user-pic.png";
            }



            // look up entry.DiplomatLinkedInId

            requestUrl = "https://api.linkedin.com/v1/people/id=" + entry.DiplomatLinkedInId.ToString() + ":(picture-url,positions)";
            postData = "format=json";
            signature = oauth.GenerateSignature(new Uri(requestUrl + "?" + postData), "s0pnibdnyjkv", "QBxVDhANOwR0lGAG", oauth_token, oauth_token_secret, "GET", oauth.GenerateTimeStamp(), oauth.GenerateNonce(), out normailizedUrl, out normalisedRequestParameters);

            requestUrl = normailizedUrl + "?" + normalisedRequestParameters + "&oauth_signature=" + HttpUtility.UrlEncode(signature);

            request = (HttpWebRequest)WebRequest.Create(requestUrl);
            request.Method = "GET";

            response = (HttpWebResponse)request.GetResponse();

            reader = new StreamReader(response.GetResponseStream());
            result = reader.ReadToEnd();

            resultJson = JObject.Parse(result);

            entry.DiplomatImageUrl = (string)resultJson["pictureUrl"];

            if (String.IsNullOrEmpty(entry.DiplomatImageUrl))
            {
                entry.DiplomatImageUrl = "/Content/img/blank-user-pic.png";
            }

            try
            {
                int companyId = (int)resultJson["positions"]["values"][0]["company"]["id"];
                
                requestUrl = "http://api.linkedin.com/v1/companies/" + companyId + ":(name,locations)";
                postData = "format=json";
                signature = oauth.GenerateSignature(new Uri(requestUrl + "?" + postData), "s0pnibdnyjkv", "QBxVDhANOwR0lGAG", oauth_token, oauth_token_secret, "GET", oauth.GenerateTimeStamp(), oauth.GenerateNonce(), out normailizedUrl, out normalisedRequestParameters);

                requestUrl = normailizedUrl + "?" + normalisedRequestParameters + "&oauth_signature=" + HttpUtility.UrlEncode(signature);

                request = (HttpWebRequest)WebRequest.Create(requestUrl);
                request.Method = "GET";

                response = (HttpWebResponse)request.GetResponse();

                reader = new StreamReader(response.GetResponseStream());
                result = reader.ReadToEnd();

                resultJson = JObject.Parse(result);

                int locations = (int)resultJson["locations"]["_total"];

                if(locations == 0)
                {
                    locations = 1;
                }

                entry.DiplomatPoints = (locations * 20) + 100;
            }
            catch
            {
                entry.DiplomatPoints = 100;
            }



            // look up entry.LinkedInId

            requestUrl = "https://api.linkedin.com/v1/people/id=" + entry.LinkedInId.ToString() + ":(picture-url)";
            postData = "format=json";
            signature = oauth.GenerateSignature(new Uri(requestUrl + "?" + postData), "s0pnibdnyjkv", "QBxVDhANOwR0lGAG", oauth_token, oauth_token_secret, "GET", oauth.GenerateTimeStamp(), oauth.GenerateNonce(), out normailizedUrl, out normalisedRequestParameters);

            requestUrl = normailizedUrl + "?" + normalisedRequestParameters + "&oauth_signature=" + HttpUtility.UrlEncode(signature);

            request = (HttpWebRequest)WebRequest.Create(requestUrl);
            request.Method = "GET";

            response = (HttpWebResponse)request.GetResponse();

            reader = new StreamReader(response.GetResponseStream());
            result = reader.ReadToEnd();

            resultJson = JObject.Parse(result);

            entry.ImageUrl = (string)resultJson["pictureUrl"];

            if (String.IsNullOrEmpty(entry.ImageUrl))
            {
                entry.ImageUrl = "/Content/img/blank-user-pic.png";
            }

            entry.TotalPoints = entry.DependablePoints + entry.DiplomatPoints + entry.HighAchieverPoints + entry.OriginalThinkerPoints + entry.SocialButterflyPoints;

            db.Entry(entry).State = EntityState.Added;
            db.SaveChanges();

            Session["Entry"] = entry;

            // if opted in, post network update
            if (entry.PostUpdate)
            {
                requestUrl = "http://api.linkedin.com/v1/people/~/person-activities";
                signature = oauth.GenerateSignature(new Uri(requestUrl), "s0pnibdnyjkv", "QBxVDhANOwR0lGAG", oauth_token, oauth_token_secret, "POST", oauth.GenerateTimeStamp(), oauth.GenerateNonce(), out normailizedUrl, out normalisedRequestParameters);

                requestUrl = normailizedUrl + "?" + normalisedRequestParameters + "&oauth_signature=" + HttpUtility.UrlEncode(signature);

                request = (HttpWebRequest)WebRequest.Create(requestUrl);
                request.Method = "POST";
                request.ContentType = "application/xml";
                request.Accept = "application/xml";

                string body = "<a href='" + entry.ProfileUrl + "'>" + entry.Name + "</a> just created a Honda Accord A-Team to go in the draw to win <a href='https://www.hondaaccordateam.com.au/'>Business Technology Products</a> valued at $3k";

                body = "<activity locale='en_US'><content-type>linkedin-html</content-type><body>" +
        HttpUtility.HtmlEncode(body) +
        "</body></activity>";

                byte[] bytes = Encoding.UTF8.GetBytes(body);

                request.ContentLength = bytes.Length;

                using (Stream putStream = request.GetRequestStream())
                {
                    putStream.Write(bytes, 0, bytes.Length);
                }

                response = (HttpWebResponse)request.GetResponse();

                reader = new StreamReader(response.GetResponseStream());
                result = reader.ReadToEnd();


                if ((result == "") && (response.StatusCode == HttpStatusCode.Created))
                {
                }
                else
                {
                }

            }

            return RedirectToAction("Confirmation");
        }

        public ActionResult Confirmation()        
        {
            ViewBag.Message = "";

            Models.Entry entry = (Models.Entry)Session["Entry"];
            return View(entry);
        }
        public ActionResult Home()
        {
            ViewBag.Message = "";

            return View();
        }
        public ActionResult Contest()
        {
            ViewBag.Message = "";

            return View();
        }
        public ActionResult ContestFilled()
        {
            ViewBag.Message = "";

            return View();
        }
        public ActionResult ChooseYou()
        {
            ViewBag.Message = "";

            return View();
        }
        public ActionResult ChooseTeam()
        {
            ViewBag.Message = "";

            return View();
        }
        public ActionResult SendInvitation()
        {
            ViewBag.Message = "";

            return View();
        }
        public ActionResult Error()
        {
            return View("Index");
        }

    }

    public class oAuthBase2
    {

        /// <summary>
        /// Provides a predefined set of algorithms that are supported officially by the protocol
        /// </summary>
        public enum SignatureTypes
        {
            HMACSHA1,
            PLAINTEXT,
            RSASHA1
        }

        /// <summary>
        /// Provides an internal structure to sort the query parameter
        /// </summary>
        protected class QueryParameter
        {
            private string name = null;
            private string value = null;

            public QueryParameter(string name, string value)
            {
                this.name = name;
                this.value = value;
            }

            public string Name
            {
                get { return name; }
            }

            public string Value
            {
                get { return value; }
            }
        }

        /// <summary>
        /// Comparer class used to perform the sorting of the query parameters
        /// </summary>
        protected class QueryParameterComparer : IComparer<QueryParameter>
        {

            #region IComparer<QueryParameter> Members

            public int Compare(QueryParameter x, QueryParameter y)
            {
                if (x.Name == y.Name)
                {
                    return string.Compare(x.Value, y.Value);
                }
                else
                {
                    return string.Compare(x.Name, y.Name);
                }
            }

            #endregion
        }

        protected const string OAuthVersion = "1.0";
        protected const string OAuthParameterPrefix = "oauth_";

        //
        // List of know and used oauth parameters' names
        //        
        protected const string OAuthConsumerKeyKey = "oauth_consumer_key";
        protected const string OAuthCallbackKey = "oauth_callback";
        protected const string OAuthVersionKey = "oauth_version";
        protected const string OAuthSignatureMethodKey = "oauth_signature_method";
        protected const string OAuthSignatureKey = "oauth_signature";
        protected const string OAuthTimestampKey = "oauth_timestamp";
        protected const string OAuthNonceKey = "oauth_nonce";
        protected const string OAuthTokenKey = "oauth_token";
        protected const string oAauthVerifier = "oauth_verifier";
        protected const string OAuthTokenSecretKey = "oauth_token_secret";

        protected const string HMACSHA1SignatureType = "HMAC-SHA1";
        protected const string PlainTextSignatureType = "PLAINTEXT";
        protected const string RSASHA1SignatureType = "RSA-SHA1";

        protected Random random = new Random();

        private string oauth_verifier;
        public string Verifier { get { return oauth_verifier; } set { oauth_verifier = value; } }


        protected string unreservedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";

        /// <summary>
        /// Helper function to compute a hash value
        /// </summary>
        /// <param name="hashAlgorithm">The hashing algoirhtm used. If that algorithm needs some initialization, like HMAC and its derivatives, they should be initialized prior to passing it to this function</param>
        /// <param name="data">The data to hash</param>
        /// <returns>a Base64 string of the hash value</returns>
        private string ComputeHash(HashAlgorithm hashAlgorithm, string data)
        {
            if (hashAlgorithm == null)
            {
                throw new ArgumentNullException("hashAlgorithm");
            }

            if (string.IsNullOrEmpty(data))
            {
                throw new ArgumentNullException("data");
            }

            byte[] dataBuffer = System.Text.Encoding.ASCII.GetBytes(data);
            byte[] hashBytes = hashAlgorithm.ComputeHash(dataBuffer);

            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// Internal function to cut out all non oauth query string parameters (all parameters not begining with "oauth_")
        /// </summary>
        /// <param name="parameters">The query string part of the Url</param>
        /// <returns>A list of QueryParameter each containing the parameter name and value</returns>
        private List<QueryParameter> GetQueryParameters(string parameters)
        {
            if (parameters.StartsWith("?"))
            {
                parameters = parameters.Remove(0, 1);
            }

            List<QueryParameter> result = new List<QueryParameter>();

            if (!string.IsNullOrEmpty(parameters))
            {
                string[] p = parameters.Split('&');
                foreach (string s in p)
                {
                    if (!string.IsNullOrEmpty(s) && !s.StartsWith(OAuthParameterPrefix))
                    {
                        if (s.IndexOf('=') > -1)
                        {
                            string[] temp = s.Split('=');
                            result.Add(new QueryParameter(temp[0], temp[1]));
                        }
                        else
                        {
                            result.Add(new QueryParameter(s, string.Empty));
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// This is a different Url Encode implementation since the default .NET one outputs the percent encoding in lower case.
        /// While this is not a problem with the percent encoding spec, it is used in upper case throughout OAuth
        /// </summary>
        /// <param name="value">The value to Url encode</param>
        /// <returns>Returns a Url encoded string</returns>
        public string UrlEncode(string value)
        {
            StringBuilder result = new StringBuilder();

            foreach (char symbol in value)
            {
                if (unreservedChars.IndexOf(symbol) != -1)
                {
                    result.Append(symbol);
                }
                else
                {
                    result.Append('%' + String.Format("{0:X2}", (int)symbol));
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Normalizes the request parameters according to the spec
        /// </summary>
        /// <param name="parameters">The list of parameters already sorted</param>
        /// <returns>a string representing the normalized parameters</returns>
        protected string NormalizeRequestParameters(IList<QueryParameter> parameters)
        {
            StringBuilder sb = new StringBuilder();
            QueryParameter p = null;
            for (int i = 0; i < parameters.Count; i++)
            {
                p = parameters[i];
                sb.AppendFormat("{0}={1}", p.Name, p.Value);

                if (i < parameters.Count - 1)
                {
                    sb.Append("&");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Generate the signature base that is used to produce the signature
        /// </summary>
        /// <param name="url">The full url that needs to be signed including its non OAuth url parameters</param>
        /// <param name="consumerKey">The consumer key</param>        
        /// <param name="token">The token, if available. If not available pass null or an empty string</param>
        /// <param name="tokenSecret">The token secret, if available. If not available pass null or an empty string</param>
        /// <param name="httpMethod">The http method used. Must be a valid HTTP method verb (POST,GET,PUT, etc)</param>
        /// <param name="signatureType">The signature type. To use the default values use <see cref="OAuthBase.SignatureTypes">OAuthBase.SignatureTypes</see>.</param>
        /// <returns>The signature base</returns>
        public string GenerateSignatureBase(Uri url, string consumerKey, string token, string tokenSecret, string httpMethod, string timeStamp, string nonce, string signatureType, out string normalizedUrl, out string normalizedRequestParameters)
        {
            if (token == null)
            {
                token = string.Empty;
            }

            if (tokenSecret == null)
            {
                tokenSecret = string.Empty;
            }

            if (string.IsNullOrEmpty(consumerKey))
            {
                throw new ArgumentNullException("consumerKey");
            }

            if (string.IsNullOrEmpty(httpMethod))
            {
                throw new ArgumentNullException("httpMethod");
            }

            if (string.IsNullOrEmpty(signatureType))
            {
                throw new ArgumentNullException("signatureType");
            }

            normalizedUrl = null;
            normalizedRequestParameters = null;

            List<QueryParameter> parameters = GetQueryParameters(url.Query);
            parameters.Add(new QueryParameter(OAuthVersionKey, OAuthVersion));
            parameters.Add(new QueryParameter(OAuthNonceKey, nonce));
            parameters.Add(new QueryParameter(OAuthTimestampKey, timeStamp));
            parameters.Add(new QueryParameter(OAuthSignatureMethodKey, signatureType));
            parameters.Add(new QueryParameter(OAuthConsumerKeyKey, consumerKey));

            if (!string.IsNullOrEmpty(token))
            {
                parameters.Add(new QueryParameter(OAuthTokenKey, token));
            }

            if (!string.IsNullOrEmpty(oauth_verifier))
            {
                parameters.Add(new QueryParameter(oAauthVerifier, oauth_verifier));
            }


            parameters.Sort(new QueryParameterComparer());


            normalizedUrl = string.Format("{0}://{1}", url.Scheme, url.Host);
            if (!((url.Scheme == "http" && url.Port == 80) || (url.Scheme == "https" && url.Port == 443)))
            {
                normalizedUrl += ":" + url.Port;
            }
            normalizedUrl += url.AbsolutePath;
            normalizedRequestParameters = NormalizeRequestParameters(parameters);

            StringBuilder signatureBase = new StringBuilder();
            signatureBase.AppendFormat("{0}&", httpMethod.ToUpper());
            signatureBase.AppendFormat("{0}&", UrlEncode(normalizedUrl));
            signatureBase.AppendFormat("{0}", UrlEncode(normalizedRequestParameters));

            return signatureBase.ToString();
        }


        /// <summary>
        /// Generate the signature value based on the given signature base and hash algorithm
        /// </summary>
        /// <param name="signatureBase">The signature based as produced by the GenerateSignatureBase method or by any other means</param>
        /// <param name="hash">The hash algorithm used to perform the hashing. If the hashing algorithm requires initialization or a key it should be set prior to calling this method</param>
        /// <returns>A base64 string of the hash value</returns>
        public string GenerateSignatureUsingHash(string signatureBase, HashAlgorithm hash)
        {
            return ComputeHash(hash, signatureBase);
        }

        /// <summary>
        /// Generates a signature using the HMAC-SHA1 algorithm
        /// </summary>		
        /// <param name="url">The full url that needs to be signed including its non OAuth url parameters</param>
        /// <param name="consumerKey">The consumer key</param>
        /// <param name="consumerSecret">The consumer seceret</param>
        /// <param name="token">The token, if available. If not available pass null or an empty string</param>
        /// <param name="tokenSecret">The token secret, if available. If not available pass null or an empty string</param>
        /// <param name="httpMethod">The http method used. Must be a valid HTTP method verb (POST,GET,PUT, etc)</param>
        /// <returns>A base64 string of the hash value</returns>
        public string GenerateSignature(Uri url, string consumerKey, string consumerSecret, string token, string tokenSecret, string httpMethod, string timeStamp, string nonce, out string normalizedUrl, out string normalizedRequestParameters)
        {
            return GenerateSignature(url, consumerKey, consumerSecret, token, tokenSecret, httpMethod, timeStamp, nonce, SignatureTypes.HMACSHA1, out normalizedUrl, out normalizedRequestParameters);
        }

        /// <summary>
        /// Generates a signature using the specified signatureType 
        /// </summary>		
        /// <param name="url">The full url that needs to be signed including its non OAuth url parameters</param>
        /// <param name="consumerKey">The consumer key</param>
        /// <param name="consumerSecret">The consumer seceret</param>
        /// <param name="token">The token, if available. If not available pass null or an empty string</param>
        /// <param name="tokenSecret">The token secret, if available. If not available pass null or an empty string</param>
        /// <param name="httpMethod">The http method used. Must be a valid HTTP method verb (POST,GET,PUT, etc)</param>
        /// <param name="signatureType">The type of signature to use</param>
        /// <returns>A base64 string of the hash value</returns>
        public string GenerateSignature(Uri url, string consumerKey, string consumerSecret, string token, string tokenSecret, string httpMethod, string timeStamp, string nonce, SignatureTypes signatureType, out string normalizedUrl, out string normalizedRequestParameters)
        {
            normalizedUrl = null;
            normalizedRequestParameters = null;

            switch (signatureType)
            {
                case SignatureTypes.PLAINTEXT:
                    return HttpUtility.UrlEncode(string.Format("{0}&{1}", consumerSecret, tokenSecret));
                case SignatureTypes.HMACSHA1:
                    string signatureBase = GenerateSignatureBase(url, consumerKey, token, tokenSecret, httpMethod, timeStamp, nonce, HMACSHA1SignatureType, out normalizedUrl, out normalizedRequestParameters);

                    HMACSHA1 hmacsha1 = new HMACSHA1();
                    hmacsha1.Key = Encoding.ASCII.GetBytes(string.Format("{0}&{1}", UrlEncode(consumerSecret), string.IsNullOrEmpty(tokenSecret) ? "" : UrlEncode(tokenSecret)));

                    return GenerateSignatureUsingHash(signatureBase, hmacsha1);
                case SignatureTypes.RSASHA1:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentException("Unknown signature type", "signatureType");
            }
        }

        /// <summary>
        /// Generate the timestamp for the signature        
        /// </summary>
        /// <returns></returns>

        public virtual string GenerateTimeStamp()
        {
            // Default implementation of UNIX time of the current UTC time
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }

        /*
       public virtual string GenerateTimeStamp() {
           // Default implementation of UNIX time of the current UTC time
           TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
           string timeStamp = ts.TotalSeconds.ToString();
           timeStamp = timeStamp.Substring(0, timeStamp.IndexOf(","));
           return Convert.ToInt64(timeStamp).ToString(); 
       }*/

        /// <summary>
        /// Generate a nonce
        /// </summary>
        /// <returns></returns>
        public virtual string GenerateNonce()
        {
            // Just a simple implementation of a random number between 123400 and 9999999
            return random.Next(123400, 9999999).ToString();
        }

    }
}
