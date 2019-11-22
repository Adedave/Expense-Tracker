using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ExpenseTracker.Data.Domain.Models;
using ExpenseTracker.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2.Responses;
using Limilabs.Client.Authentication.Google;
using Limilabs.Client.IMAP;
using Limilabs.Mail;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Polly.Retry;
using Polly;
using System.Net;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Biz.IServices;
using ExpenseTracker.Web.Configuration;
using Microsoft.Extensions.Logging;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ExpenseTracker.Web.Controllers
{
    [Authorize]
    public class EmailController : Controller
    {
        private readonly ExpenseTrackerDbContext _context;
        private readonly OAuthConfig _oAuthConfig;
        private readonly UserManager<AppUser> _userManager;
        private readonly IBankAccountService _bankAccountService;
        private readonly IGoogleOAuthService _googleOAuthService;
        private readonly ILogger<EmailController> _logger;

        public EmailController(ExpenseTrackerDbContext context,OAuthConfig oAuthConfig, 
            UserManager<AppUser> userManager, IBankAccountService bankAccountService,
            IGoogleOAuthService googleOAuthService, ILogger<EmailController> logger)
        {
            _userManager = userManager;
            _bankAccountService = bankAccountService;
            _googleOAuthService = googleOAuthService;
            _logger = logger;
            _context = context;
            _oAuthConfig = oAuthConfig;
        }
        // GET: /<controller>/
        private BankAccount _bankAccount;
        public IActionResult LinkEmailToApp(BankAccount account)
        {
            _bankAccount = new BankAccount();
            _bankAccount = account;
            return RedirectToAction("GoogleOAuth");
        }

        public async Task<IActionResult> Index(string code)
        {
            try
            {
                //to check for direct access to this action without {code} or redirection from google's oauth page
                if (code == null)
                {
                    TempData["Message"] = "Bank account added successfully!";
                    return RedirectToAction("Index", "BankAccount");
                }
                var appUser = await GetCurrentUser();

                //string authCode = code;
                GoogleAuth googleAuth = await GetAccessToken(code);
                BankAccount bankAccount = await FindBankAccount();
                //fix here
                googleAuth = AddOAuthProp(appUser.Id, bankAccount?.AlertEmail, bankAccount?.AccountNumber, googleAuth);
                SaveGoogleOAuth(googleAuth);

                //if bankaccount is equals to null redirect to bank account not found page
                //set IsConnected property for the bank account
                _bankAccountService.SetIsConnectedProperty(bankAccount.BankAccountId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                _logger.LogError(ex.StackTrace);
            }
            TempData["Message"] = "Bank account added successfully!";
            return RedirectToAction("Index", "BankAccount");
        }

        private GoogleAuth AddOAuthProp(string id, string alertEmail, string accNumber, GoogleAuth googleAuth)
        {
            googleAuth.AppUserId = id;
            googleAuth.Email = alertEmail;
            googleAuth.AccountNumber = accNumber;
            return googleAuth;
        }

        private void SaveGoogleOAuth(GoogleAuth googleAuth)
        {
            var oauth = _googleOAuthService.GetGoogleOAuthByEmailAndAccNumber(googleAuth?.Email,googleAuth?.AccountNumber);
                //_context.GoogleAuths.FirstOrDefault(x => x.Email == googleAuth?.Email);
            if (oauth != null)
            {
               _googleOAuthService.DeleteGoogleAuth(oauth);
            }
           _googleOAuthService.AddGoogleAuth(googleAuth);
        }

        private void UpdateGoogleOAuth(GoogleAuth googleAuth)
        {
            _context.Entry(googleAuth).State = EntityState.Modified;
            _context.SaveChanges();
        }

        public async Task<IActionResult> GetBankTransactions(string accountNumber=null)
        {
            
            List<string> vs = new List<string>();
           
            BankAccount bankAccount = await FindBankAccount(accountNumber);

            GoogleAuth googleAuth = await GetGoogleAuth(bankAccount?.AlertEmail,bankAccount?.AccountNumber);

            List<BankTransaction> bankTransactions = new List<BankTransaction>();

            //long? largestUID = googleAuth.LargestUID;

            try
            {
                using (Imap imap = new Imap())
                {
                    GoogleApi api = new GoogleApi(googleAuth?.AccessToken);
                    string user = api.GetEmail();

                    imap.ConnectSSL("imap.gmail.com");  // or ConnectSSL for SSL 
                    imap.LoginOAUTH2(user, googleAuth?.AccessToken);

                    FolderStatus status = imap.SelectInbox();


                    imap.SelectInbox();

                    //while (true)
                    //{
                    //Uncomment the line below for instant push  email notification
                    //FolderStatus currentStatus = imap.Idle();
                    List<long> uids;
                    if (googleAuth.LargestUID == 0 || googleAuth.UIDValidity != status.UIDValidity)
                    {
                        //get emails in the last one month
                        uids = imap.Search().Where(
                            Expression.And(
                            Expression.From("gens@gtbank.com"),
                            Expression.SentSince(DateTime.Now.AddMonths(-1)))
                            );
                    }
                    else
                    {
                        uids = imap.Search().Where(
                            Expression.And(
                            Expression.From("gens@gtbank.com"),
                            Expression.UID(Range.From(googleAuth.LargestUID))
                            ));
                        uids.Remove(googleAuth.LargestUID);
                    }
                    //List<MessageInfo> infos = imap.GetMessageInfoByUID(uids);

                    //int counter = 0;
                    foreach (long uid in uids)
                    {
                        //counter++;
                        MessageInfo info = imap.GetMessageInfoByUID(uid);
                        //var 
                        if (info.Envelope.Subject.Contains("Transaction Alert [Credit", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }
                        var eml = imap.GetMessageByUID(uid);
                        IMail email = new MailBuilder().CreateFromEml(eml);

                        //List<long> uids = imap.Search(Expression.From("gtbank.com"));
                        //for (int i = 1; i < 20; i++)
                        //{

                        //IMail email = new MailBuilder()
                        //    .CreateFromEml(imap.GetMessageByUID(uids[uids.Count - i]));
                        if (email.Subject.Contains("Transaction Alert [Debit", StringComparison.OrdinalIgnoreCase)
                            || (email.GetBodyAsText().Contains("eLectronic Notification Service", StringComparison.OrdinalIgnoreCase)
                            && email.GetBodyAsText().Contains("Debit transaction", StringComparison.OrdinalIgnoreCase))
                            //&& email.GetBodyAsText().Contains($"{bankRegistered.AccountNo}")
                            )
                        {
                            Dictionary<string, string> transactionDetails = new Dictionary<string, string>();
                            string emailAsText = email.GetBodyAsText();
                            string emailAsTextTwo = email.GetTextFromHtml();
                            string emailAsTextThree = email.Text;
                            if (emailAsText.Length < 10)
                            {
                                continue;
                            }

                            transactionDetails = ExtractText(emailAsText);
                            if (transactionDetails["Account Number"] != accountNumber)
                            {
                                continue;
                            }
                            BankTransaction transaction = new BankTransaction
                            {
                                //transaction.AccountNo = transactionDetails["AccountNo"];
                                Subject = email.Subject
                            };
                            if (email.Subject.Contains("Debit", StringComparison.OrdinalIgnoreCase)
                                || email.GetBodyAsText().Contains("Debit", StringComparison.OrdinalIgnoreCase))
                            {
                                transaction.TransactionType = "Debit";
                            }
                            if (email.Subject.Contains("Credit", StringComparison.OrdinalIgnoreCase)
                                 || email.GetBodyAsText().Contains("Credit", StringComparison.OrdinalIgnoreCase))
                            {
                                transaction.TransactionType = "Credit";
                            }
                            transaction.Description = transactionDetails["Description"];
                            transaction.TransactionAmount = Convert.ToDecimal(transactionDetails["Amount"]);
                            transaction.Remarks = transactionDetails["Remarks"];
                            transaction.AccountNumber = transactionDetails["Account Number"];
                            //You can try to format the datetime to a britain english culture, IFormatProvider
                            transaction.TransactionDate = Convert.ToDateTime(transactionDetails["Date"] + " " + transactionDetails["Time"]);
                            bool IsExists = await VerifyTransaction(transaction.AccountNumber, transaction.TransactionDate,
                                                              transaction.TransactionType, transaction.TransactionAmount);
                            
                            if (IsExists)
                            {
                                continue;
                            }

                            //appuser Id is set in the Save transaction Method
                            await SaveTransaction(transaction);

                            bankTransactions.Add(transaction);

                        }
                    }
                            UpdateLargestUID(uids[(uids.Count)-1],googleAuth,status.UIDValidity);
                    //}
                    imap.Close();
                }
            }
            catch (ImapResponseException ex)
            {
                Debug.WriteLine(ex.Message);
                _logger.LogError(ex.StackTrace);
                googleAuth.AccessToken = await RefreshAccessToken(googleAuth.RefreshToken);
                UpdateGoogleOAuth(googleAuth);
                await GetBankTransactions(accountNumber);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                //log exception 
                _logger.LogError(ex.StackTrace);
            }
            return RedirectToAction("Index","BankTransaction",new { accountId = bankAccount.BankAccountId });
        }

        private void UpdateLargestUID(long uid,GoogleAuth googleAuth, long UIDValidity)
        {
            googleAuth.LargestUID = uid;
            googleAuth.UIDValidity = UIDValidity;
            _context.Entry(googleAuth).State = EntityState.Modified;
            _context.SaveChanges();
        }
        
        private async Task<GoogleAuth> GetGoogleAuth(string email, string accNumber)
        {
            GoogleAuth googleAuth = new GoogleAuth();
            try
            {
                var appUser = await GetCurrentUser();
                googleAuth = _context.GoogleAuths
                            .LastOrDefault(x => x.AppUserId == appUser.Id && x.Email == email && x.AccountNumber == accNumber);
                if ((DateTime.UtcNow - googleAuth.IssuedUtc).TotalHours > 1)
                {
                    googleAuth.AccessToken = await RefreshAccessToken(googleAuth.RefreshToken);
                    googleAuth.IssuedUtc = DateTime.UtcNow;
                    UpdateGoogleOAuth(googleAuth);
                }
                googleAuth = _context.GoogleAuths
                            .LastOrDefault(x => x.AppUserId == appUser.Id && x.Email == email && x.AccountNumber == accNumber);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                _logger.LogError(ex.StackTrace);
            }
            return googleAuth;
        }


        //private AsyncRetryPolicy<HttpResponseMessage> CreateTokenRefreshPolicy(Func<string, Task> tokenRefreshed)
        //{
        //    var policy = Policy
        //        .HandleResult<HttpResponseMessage>(message => message.StatusCode == HttpStatusCode.Unauthorized)
        //        .RetryAsync(1, async (result, retryCount, context) =>
        //        {
        //            if (context.ContainsKey("refresh_token"))
        //            {
        //                var accessToken = await RefreshAccessToken(context["refresh_token"].ToString());
        //                if (accessToken != null)
        //                {
        //                    await tokenRefreshed(accessToken);

        //                    context["access_token"] = accessToken;
        //                }
        //            }
        //        });

        //    return policy;
        //}
        
        private async Task<bool> VerifyTransaction(string accountNumber, DateTime transactionDate, string transactionType, decimal transactionAmount)
        {
            bool IsExists = false;
            try
            {
                var appUser = await GetCurrentUser();
                List<BankTransaction> bankTransactions = new List<BankTransaction>();
                bankTransactions = _context.BankTransactions
                                    .Where(x => x.AppUserId == appUser.Id && x.IsDeleted == false)
                                    .ToList();
                foreach (var item in bankTransactions)
                {
                    if (item.AccountNumber == accountNumber && item.TransactionDate == transactionDate
                        && item.TransactionType == transactionType && item.TransactionAmount == transactionAmount)
                    {
                        IsExists = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                _logger.LogError(ex.StackTrace);
            }
            return IsExists;
        }

        private async Task SaveTransaction(BankTransaction transaction)
        {
            var appUser = await GetCurrentUser();
            transaction.AppUserId = appUser.Id;
            _context.BankTransactions.Add(transaction);
            _context.SaveChanges();
        }

        public IActionResult GoogleOAuth(string accountNumber)
        {
            _bankAccountService.SetAboutToConnectProperty(accountNumber);
            string redirectUri = _oAuthConfig.RedirectUri; /* _configuration["OAUTH:redirectUri"];*/

            string clientID = _oAuthConfig.ClientId;  /*_configuration["OAUTH:clientID"];*/

            string clientSecret = _oAuthConfig.ClientSecret;  /*_configuration["OAUTH:clientSecret"];*/


            var clientSecrets = new ClientSecrets
            {
                ClientId = clientID,
                ClientSecret = clientSecret
            };

            var credential = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = clientSecrets,
                Scopes = new[] { GoogleScope.ImapAndSmtp.Name, GoogleScope.UserInfoEmailScope.Name }
            });

            AuthorizationCodeRequestUrl url = credential.CreateAuthorizationCodeRequest(redirectUri);

            return new RedirectResult(url.Build().ToString());
        }

        private async Task<BankAccount> FindBankAccount(string accountNumber=null)
        {
            BankAccount bankAccount = new BankAccount();
            var user = await GetCurrentUser();
            if (string.IsNullOrEmpty(accountNumber))
            {
                //it could be lastordefault should incase i have multiple about to connect
               bankAccount = _context.BankAccounts
                            .LastOrDefault(x => x.AppUserId == user.Id && x.AboutToConnect == true);
            }
            else
            {
                bankAccount = _context.BankAccounts
                                .SingleOrDefault(x => x.AppUserId == user.Id && x.AccountNumber == accountNumber);
            }
            return bankAccount;
        }

        public async Task<GoogleAuth> GetAccessToken(string code)
        {
            TokenResponse tokenResponse = new TokenResponse();
            string redirectUri = _oAuthConfig.RedirectUri; 

            string clientID = _oAuthConfig.ClientId; 

            string clientSecret = _oAuthConfig.ClientSecret; 

            string basUrl = "https://www.googleapis.com/oauth2/v4/token/";
            string body = "code=" + code + "&client_id=" + clientID + "&client_secret=" + clientSecret + "&grant_type=authorization_code&redirect_uri=" + redirectUri;

            using (var httpClientHandler = new HttpClientHandler())
            {
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                using (var httpClient = new HttpClient(httpClientHandler))
                {
                    StringContent content = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded");
                    using (var response = await httpClient.PostAsync(basUrl, content))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(apiResponse);
                    }
                }
            }
            GoogleAuth googleAuth = new GoogleAuth
            {
                AccessToken = tokenResponse.AccessToken,
                RefreshToken = tokenResponse.RefreshToken,
                ExpiresInSeconds = tokenResponse.ExpiresInSeconds,
                IdToken = tokenResponse.IdToken,
                IssuedUtc = DateTime.UtcNow,
                Scope = tokenResponse.Scope,
                TokenType = tokenResponse.TokenType
            };

            return googleAuth;

        }

        private async Task<string> RefreshAccessToken(string refreshToken)
        {
            TokenResponse tokenResponse = new TokenResponse();

            string redirectUri = _oAuthConfig.RedirectUri;

            string clientID = _oAuthConfig.ClientId;

            string clientSecret = _oAuthConfig.ClientSecret;

            string basUrl = "https://www.googleapis.com/oauth2/v4/token/";

            //I can continue to use the same refresh token as long as i have not reached the limit
            string body = "refresh_token=" + refreshToken + "&client_id=" + clientID + "&client_secret=" + clientSecret + "&grant_type=refresh_token";
            using (var httpClientHandler = new HttpClientHandler())
            {
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                using (var httpClient = new HttpClient(httpClientHandler))
                {
                    StringContent content = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded");
                    using (var response = await httpClient.PostAsync(basUrl, content))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(apiResponse);
                    }
                }
            }

            GoogleAuth googleAuth = new GoogleAuth
            {
                AccessToken = tokenResponse.AccessToken,
                RefreshToken = tokenResponse.RefreshToken,
                ExpiresInSeconds = tokenResponse.ExpiresInSeconds,
                IdToken = tokenResponse.IdToken,
                IssuedUtc = DateTime.UtcNow,
                Scope = tokenResponse.Scope,
                TokenType = tokenResponse.TokenType
            };


            // return null if we cannot request a new token
            return tokenResponse.AccessToken;
        }



        public Dictionary<string,string> ExtractText(string text)
        {
            Dictionary<string, string> transactionDetails = new Dictionary<string, string>();
            try
            {
                int amountIndex = text.IndexOf("Amount");
                int accountNumberIndex = text.IndexOf("Account Number");
                int descriptionIndex = text.IndexOf("Description");
                int dateIndex = text.IndexOf("Value Date");
                int timeIndex = text.IndexOf("Time of Transaction");
                int remarksIndex = text.IndexOf("Remarks");
                int documentNoIndex = text.IndexOf("Document Number");


                string amount = text.Substring(amountIndex, dateIndex - amountIndex - 1).Remove(0, "Amount :".Length);
                amount = amount.Replace("\r\n", "");
                amount = amount.Replace(":", "");
                amount = amount.Trim();
                transactionDetails.Add("Amount", amount);

                int transactLocationIndex = text.IndexOf("Transaction Location");
                string accountNumber = text.Substring(accountNumberIndex, transactLocationIndex - accountNumberIndex - 1).Remove(0, "Account Number :".Length);
                accountNumber = accountNumber.Replace("\r\n", "");
                accountNumber = accountNumber.Replace(":", "");
                accountNumber = accountNumber.Trim();
                transactionDetails.Add("Account Number", accountNumber);

                string description = text.Substring(descriptionIndex, amountIndex - descriptionIndex - 1).Remove(0, "Description :".Length);
                description = description.Replace("\r\n", "");
                description = description.Replace(":", "");
                description = description.Trim();
                transactionDetails.Add("Description", description);

                string date = text.Substring(dateIndex , remarksIndex - dateIndex-1).Remove(0, "Value Date :".Length);
                date = date.Replace("\r\n", "");
                date = date.Replace(":", "");
                date = date.Trim();
                transactionDetails.Add("Date", date); 

                string time = text.Substring(timeIndex, documentNoIndex - timeIndex-1).Remove(0, "Time of Transaction :".Length);
                time = time.Replace("\r\n", "");
                int firstIndex = time.IndexOf(':');
                time = time.Remove(firstIndex, 1);
                time = time.Trim();
                transactionDetails.Add("Time", time);

                string remarks = text.Substring(remarksIndex , timeIndex - remarksIndex-1).Remove(0, "Remarks :".Length);
                remarks = remarks.Replace("\r\n", "");
                remarks = remarks.Replace(":", "");
                remarks = remarks.Trim();
                transactionDetails.Add("Remarks", remarks);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                _logger.LogError(ex.StackTrace);
            }
            return transactionDetails;
        }

        public async Task<AppUser> GetCurrentUser()
        {
            var user = await _userManager.FindByNameAsync(HttpContext.User.Identity.Name);
            return user;
        }
    }
}
