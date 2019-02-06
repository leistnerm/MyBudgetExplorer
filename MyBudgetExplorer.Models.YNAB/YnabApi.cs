/* 
 * Copyright 2019 Mark D. Leistner
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *   http://www.apache.org/licenses/LICENSE-2.0
 *   
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using Amazon.XRay.Recorder.Handlers.System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MyBudgetExplorer.Models.YNAB
{
    public class YnabApi
    {
        #region Fields
        private string accessToken = null;
        #endregion

        #region Constructors
        public YnabApi(string accessToken)
        {
            this.accessToken = accessToken;
        }
        #endregion

        #region Private Methods
        private dynamic Connect(string method, string resource, JObject param = null)
        {
            Uri uri = new Uri(string.Format("https://api.youneedabudget.com/v1{0}", resource));

            using (HttpClient hc = new HttpClient(new HttpClientXRayTracingHandler(new HttpClientHandler())))
            {
                hc.Timeout = new TimeSpan(0, 5, 0);

                if (accessToken != null)
                    hc.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", accessToken));

                Task<string> result;
                switch (method.ToUpper())
                {
                    default:
                        throw new ArgumentException();
                    case "GET":
                        result = hc.GetStringAsync(uri);
                        break;
                    case "POST":
                        HttpContent postcontent;
                        if (param == null) postcontent = new StringContent("", Encoding.UTF8, "application/json");
                        else postcontent = new StringContent(param.ToString(), Encoding.UTF8, "application/json");
                        var postresponse = hc.PostAsync(uri, postcontent);
                        result = postresponse.Result.Content.ReadAsStringAsync();
                        break;
                    case "PUT":
                        var putresponse = hc.PostAsync(uri, new StringContent(param.ToString(), Encoding.UTF8, "application/json")).Result;
                        result = putresponse.Content.ReadAsStringAsync();
                        break;
                    case "DELETE":
                        var deleteresponse = hc.DeleteAsync(uri).Result;
                        result = deleteresponse.Content.ReadAsStringAsync();
                        break;
                }
                
                return JObject.Parse(result.Result);
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// /budgets/{budgetId}/accounts/{accountId} - Single account
        /// </summary>
        /// <param name="accountId">The id of the account</param>
        /// <param name="budgetId">The id of the budget (“last-used” can also be used to specify the last used budget)</param>
        public AccountResponse GetAccount(string accountId, string budgetId = "last-used")
        {
            return AccountResponse.Load(Connect("GET", $"/budgets/{budgetId}/accounts/{accountId}"));
        }
        /// <summary>
        /// /budgets/{budgetId}/accounts - Account list
        /// </summary>
        /// <param name="budgetId">The id of the budget (“last-used” can also be used to specify the last used budget)</param>
        /// <returns>Returns all accounts</returns>
        public AccountsResponse GetAccounts(string budgetId = "last-used")
        {
            return AccountsResponse.Load(Connect("GET", $"/budgets/{budgetId}/accounts"));
        }
        /// <summary>
        /// /budgets/{budgetId} - Single Budget
        /// </summary>
        /// <param name="last">The starting server knowledge. If provided, only entities that have changed since last_knowledge_of_server will be included.</param>
        /// <param name="budgetId">The id of the budget (“last-used” can also be used to specify the last used budget)</param>
        /// <returns>Returns a single budget with all related entities. This resource is effectively a full budget export.</returns>
        public BudgetDetailResponse GetBudget(int last = 0, string budgetId = "last-used")
        {
            if (last <= 0)
                return BudgetDetailResponse.Load(Connect("GET", $"/budgets/{budgetId}"));
            else
                return BudgetDetailResponse.Load(Connect("GET", $"/budgets/{budgetId}?last_knowledge_of_server={last}"));
        }
        /// <summary>
        /// /budgets - List budgets
        /// </summary>
        /// <returns>Returns budgets list with summary information</returns>
        public BudgetSummaryResponse GetBudgets()
        {
            return BudgetSummaryResponse.Load(Connect("GET", "/budgets"));
        }
        /// <summary>
        /// /budgets/{budgetId}/settings - Budget settings
        /// </summary>
        /// <param name="budgetId">The id of the budget (“last-used” can also be used to specify the last used budget)</param>
        /// <returns>Returns settings for a budget</returns>
        public BudgetSettingsResponse GetBudgetSettings(string budgetId = "last-used")
        {
            return BudgetSettingsResponse.Load(Connect("GET", $"/budgets/{budgetId}/settings"));
        }
        /// <summary>
        /// /budgets/{budgetId}/categories - List categories
        /// </summary>
        /// <param name="budgetId">The id of the budget (“last-used” can also be used to specify the last used budget)</param>
        /// <returns>Returns all categories grouped by category group. Amounts (budgeted, activity, balance, etc.) are specific to the current budget month (UTC).</returns>
        public CategoriesResponse GetCategories(string budgetId = "last-used")
        {
            return CategoriesResponse.Load(Connect("GET", $"/budgets/{budgetId}/categories"));
        }
        /// <summary>
        /// /budgets/{budgetId}/months/{month}/categories/{categoryId} - Single category for a specific budget month
        /// </summary>
        /// <param name="categoryId">The id of the category</param>
        /// <param name="budgetId">The id of the budget (“last-used” can also be used to specify the last used budget)</param>
        /// <param name="month">The budget month in ISO format (e.g. 2016-12-30) (“current” can also be used to specify the current calendar month (UTC))</param>
        public CategoryResponse GetCategory(string categoryId, string budgetId = "last-used", string month = "current")
        {
            return CategoryResponse.Load(Connect("GET", $"/budgets/{budgetId}/months/{month}/categories/{categoryId}"));
        }
        /// <summary>
        /// /budgets/{budgetId}/months/{month} - Single budget month
        /// </summary>
        /// <param name="month">The budget month in ISO format (e.g. 2016-12-30) (“current” can also be used to specify the current calendar month (UTC))</param>
        /// <param name="budgetId">The id of the budget (“last-used” can also be used to specify the last used budget)</param>
        /// <returns>Returns a single budget month</returns>
        public MonthDetailResponse GetMonth(string month = "current", string budgetId = "last-used")
        {
            return MonthDetailResponse.Load(Connect("GET", $"/budgets/{budgetId}/months/{month}"));
        }
        /// <summary>
        /// /budgets/{budgetId}/months - List budget months
        /// </summary>
        /// <param name="budgetId">The id of the budget (“last-used” can also be used to specify the last used budget)</param>
        /// <returns>Returns all budget months</returns>
        public MonthSummariesResponse GetMonths(string budgetId = "last-used")
        {
            return MonthSummariesResponse.Load(Connect("GET", $"/budgets/{budgetId}/months"));
        }
        /// <summary>
        /// /budgets/{budgetId}/payees/{payeeId} - Single payee
        /// </summary>
        /// <param name="payeeId">The id of the payee</param>
        /// <param name="budgetId">The id of the budget (“last-used” can also be used to specify the last used budget)</param>
        /// <returns>Returns single payee</returns>
        public Task<PayeeResponse> GetPayee(string payeeId, string budgetId = "last-used")
        {
            return PayeesResponse.Load(Connect("GET", $"/budgets/{budgetId}/payees/{payeeId}"));
        }
        /// <summary>
        /// /budgets/{budgetId}/payee_locations/{payeeLocationId} - Single payee location
        /// </summary>
        /// <param name="payeeLocationId">id of payee location</param>
        /// <param name="budgetId">The id of the budget (“last-used” can also be used to specify the last used budget)</param>
        /// <returns>Returns a single payee location</returns>
        public PayeeLocationResponse GetPayeeLocation(string payeeLocationId, string budgetId = "last-used")
        {
            return PayeeLocationResponse.Load(Connect("GET", $"/budgets/{budgetId}/payee_locations/{payeeLocationId}"));
        }
        /// <summary>
        /// /budgets/{budgetId}/payee_locations - List payee locations
        /// </summary>
        /// <param name="payeeId">id of payee ("all" can also be used to specify all payee locations)</param>
        /// <param name="budgetId">The id of the budget (“last-used” can also be used to specify the last used budget)</param>
        /// <returns>Returns all payee locations</returns>
        public PayeeLocationsResponse GetPayeeLocations(string payeeId = "all", string budgetId = "last-used")
        {
            if (payeeId == "all")
                return PayeeLocationsResponse.Load(Connect("GET", $"/budgets/{budgetId}/payee_locations"));

            return PayeeLocationsResponse.Load(Connect("GET", $"/budgets/{budgetId}/payee/{payeeId}/payee_locations"));
        }
        /// <summary>
        /// /budgets/{budgetId}/payees - List payees
        /// </summary>
        /// <param name="budgetId">The id of the budget (“last-used” can also be used to specify the last used budget)</param>
        /// <returns>Returns all payees</returns>
        public PayeesResponse GetPayees(string budgetId = "last-used")
        {
            return PayeesResponse.Load(Connect("GET", $"/budgets/{budgetId}/payees"));
        }
        /// <summary>
        /// /budgets/{budgetId}/scheduled_transactions/{scheduledTransactionId} - Single scheduled transaction
        /// </summary>
        /// <param name="scheduledTransactionId">The id of the scheduled transaction</param>
        /// <param name="budgetId">The id of the budget (“last-used” can also be used to specify the last used budget)</param>
        /// <returns>Returns a single scheduled transaction</returns>
        public ScheduledTransactionResponse GetScheduledTransaction(string scheduledTransactionId, string budgetId = "last-used")
        {
            return ScheduledTransactionResponse.Load(Connect("GET", $"/budgets/{budgetId}/scheduled_transactions/{scheduledTransactionId}"));
        }
        /// <summary>
        /// /budgets/{budgetId}/scheduled_transactions - List scheduled transactions
        /// </summary>
        /// <param name="budgetId">The id of the budget (“last-used” can also be used to specify the last used budget)</param>
        /// <returns>Returns all scheduled transactions</returns>
        public ScheduledTransactionsResponse GetScheduledTransactions(string budgetId = "last-used")
        {
            return ScheduledTransactionsResponse.Load(Connect("GET", $"/budgets/{budgetId}/scheduled_transactions"));
        }
        /// <summary>
        /// /budgets/{budgetId}/transactions/{transactionId} - Single transaction
        /// </summary>
        /// <param name="transactionId">The id of the transaction</param>
        /// <param name="budgetId">The id of the budget (“last-used” can also be used to specify the last used budget)</param>
        /// <returns>Returns a single transaction</returns>
        public TransactionResponse GetTransaction(string transactionId, string budgetId = "last-used")
        {
            return TransactionResponse.Load(Connect("GET", $"/budgets/{budgetId}/transactions/{transactionId}"));
        }
        /// <summary>
        /// /budgets/{budgetId}/transactions - List transactions
        /// </summary>
        /// <param name="last">The starting server knowledge. If provided, only entities that have changed since last_knowledge_of_server will be included.</param>
        /// <param name="sinceDate">If specified, only transactions on or after this date will be included. The date should be ISO formatted (e.g. 2016-12-30).</param>
        /// <param name="type">If specified, only transactions of the specified type will be included. ‘uncategorized’ and ‘unapproved’ are currently supported.</param>
        /// <param name="budgetId">The id of the budget (“last-used” can also be used to specify the last used budget)</param>
        /// <returns>Returns budget transactions</returns>
        public TransactionsResponse GetTransactions(int last = 0, string sinceDate = null, string type = null, string budgetId = "last-used")
        {
            var url = $"/budgets/{budgetId}/transactions";
            var query = new List<string>();
            if (last > 0)
                query.Add($"last_knowledge_of_server={last}");
            if (!string.IsNullOrEmpty(sinceDate))
                query.Add($"since_date={sinceDate}");
            if (!string.IsNullOrEmpty(type))
                query.Add($"type={type}");
            if (query.Count > 0)
                url += $"?{string.Join("&", query)}";

            return TransactionsResponse.Load(Connect("GET", url));
        }
        /// <summary>
        /// /budgets/{budgetId}/accounts/{accountId}/transactions - List account transactions
        /// </summary>
        /// <param name="accountId">The id of the account</param>
        /// <param name="last">The starting server knowledge. If provided, only entities that have changed since last_knowledge_of_server will be included.</param>
        /// <param name="sinceDate">If specified, only transactions on or after this date will be included. The date should be ISO formatted (e.g. 2016-12-30).</param>
        /// <param name="type">If specified, only transactions of the specified type will be included. ‘uncategorized’ and ‘unapproved’ are currently supported.</param>
        /// <param name="budgetId">The id of the budget (“last-used” can also be used to specify the last used budget)</param>
        /// <returns>Returns budget transactions</returns>
        public TransactionsResponse GetTransactionsByAccount(string accountId, int last = 0, string sinceDate = null, string type = null, string budgetId = "last-used")
        {
            var url = $"/budgets/{budgetId}/accounts/{accountId}/transactions";
            var query = new List<string>();
            if (last > 0)
                query.Add($"last_knowledge_of_server={last}");
            if (!string.IsNullOrEmpty(sinceDate))
                query.Add($"since_date={sinceDate}");
            if (!string.IsNullOrEmpty(type))
                query.Add($"type={type}");
            if (query.Count > 0)
                url += $"?{string.Join("&", query)}";

            return TransactionsResponse.Load(Connect("GET", url));
        }
        /// <summary>
        /// /budgets/{budgetId}/categories/{categoryId}/transactions - List category transactions
        /// </summary>
        /// <param name="categoryId">The id of the category</param>
        /// <param name="last">The starting server knowledge. If provided, only entities that have changed since last_knowledge_of_server will be included.</param>
        /// <param name="sinceDate">If specified, only transactions on or after this date will be included. The date should be ISO formatted (e.g. 2016-12-30).</param>
        /// <param name="type">If specified, only transactions of the specified type will be included. ‘uncategorized’ and ‘unapproved’ are currently supported.</param>
        /// <param name="budgetId">The id of the budget (“last-used” can also be used to specify the last used budget)</param>
        /// <returns>Returns budget transactions</returns>
        public TransactionsResponse GetTransactionsByCategory(string categoryId, int last = 0, string sinceDate = null, string type = null, string budgetId = "last-used")
        {
            var url = $"/budgets/{budgetId}/categories/{categoryId}/transactions";
            var query = new List<string>();
            if (last > 0)
                query.Add($"last_knowledge_of_server={last}");
            if (!string.IsNullOrEmpty(sinceDate))
                query.Add($"since_date={sinceDate}");
            if (!string.IsNullOrEmpty(type))
                query.Add($"type={type}");
            if (query.Count > 0)
                url += $"?{string.Join("&", query)}";

            return TransactionsResponse.Load(Connect("GET", url));
        }
        /// <summary>
        /// /budgets/{budgetId}/payees/{payeeId}/transactions - List payee transactions
        /// </summary>
        /// <param name="payeeId">The id of the payee</param>
        /// <param name="last">The starting server knowledge. If provided, only entities that have changed since last_knowledge_of_server will be included.</param>
        /// <param name="sinceDate">If specified, only transactions on or after this date will be included. The date should be ISO formatted (e.g. 2016-12-30).</param>
        /// <param name="type">If specified, only transactions of the specified type will be included. ‘uncategorized’ and ‘unapproved’ are currently supported.</param>
        /// <param name="budgetId">The id of the budget (“last-used” can also be used to specify the last used budget)</param>
        /// <returns>Returns budget transactions</returns>
        public TransactionsResponse GetTransactionsByPayee(string payeeId, int last = 0, string sinceDate = null, string type = null, string budgetId = "last-used")
        {
            var url = $"/budgets/{budgetId}/payees/{payeeId}/transactions";
            var query = new List<string>();
            if (last > 0)
                query.Add($"last_knowledge_of_server={last}");
            if (!string.IsNullOrEmpty(sinceDate))
                query.Add($"since_date={sinceDate}");
            if (!string.IsNullOrEmpty(type))
                query.Add($"type={type}");
            if (query.Count > 0)
                url += $"?{string.Join("&", query)}";

            return TransactionsResponse.Load(Connect("GET", url));
        }
        /// <summary>
        /// /user - User Info
        /// </summary>
        /// <returns>Returns authenticated user information</returns>
        public UserResponse GetUser()
        {
            return UserResponse.Load(Connect("GET", "/user"));
        }
        #endregion
    }
}