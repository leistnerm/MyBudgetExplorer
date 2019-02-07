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
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using MyBudgetExplorer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace MyBudgetExplorer.Pages.Budget
{
    public class PayeeModel : PageModel
    {
        private IConfiguration _configuration;
        private IMemoryCache _cache;
        public List<TransactionModel> Transactions { get; set; } = new List<TransactionModel>();
        public string Previous { get; set; }
        public string Next { get; set; }
        public string Id { get; set; }
        public DateTime Date { get; set; }
        public PayeeModel(IConfiguration configuration, IMemoryCache memoryCache)
        {
            _configuration = configuration;
            _cache = memoryCache;
        }
        public void OnGet(string id, string date = null)
        {
            Id = id;

            Forecast forecast = null;
            var accessToken = HttpContext.GetTokenAsync("access_token").Result;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            _cache.TryGetValue(userId, out forecast);
            if (forecast == null)
            {
                forecast = Cache.GetForecast(accessToken, userId);
                _cache.Set(userId, forecast);
            }

            ViewData["LastUpdated"] = forecast.LastModifiedOn;

            var currentDate = forecast.CurrentMonthStart;
            if (date != null)
                DateTime.TryParse(date, out currentDate);

            currentDate = new DateTime(currentDate.Year, currentDate.Month, 1);

            Date = currentDate;
            Previous = currentDate.AddMonths(-1).ToShortDateString();
            Next = currentDate.AddMonths(1).ToShortDateString();

            var payee = forecast.Payees.SingleOrDefault(p => p.PayeeId == id);
            if (payee == null)
            {
                var ex = new ApplicationException("The specified payee could not be found.");
                ex.Data.Add("Payee Id", id);
                throw ex;
            }

            ViewData["Title"] = $"Explore > {payee.Name}";

            var transactions = forecast.Transactions
                .Where(t => !t.Deleted && t.PayeeId == id)
                .Where(t => t.Date < currentDate.AddMonths(1))
                .OrderBy(t => t.Date)
                .ThenByDescending(t => t.Amount);

            // Transactions
            foreach (var t in forecast.Transactions)
            {
                if (t.PayeeId == id)
                {
                    var cat = forecast.Categories.SingleOrDefault(c => c.CategoryId == t.CategoryId);
                    if (cat != null)
                        Transactions.Add(new TransactionModel
                        {
                            Date = t.Date,
                            Category = cat.Name,
                            Amount = t.Amount,
                            Total = ""
                        });
                    else
                    {
                        foreach (var s in forecast.SubTransactions)
                        {
                            if (s.TransactionId == t.TransactionId )
                            {
                                var trans = new TransactionModel
                                {
                                    Date = t.Date,
                                    Category = forecast.Categories.Single(c => c.CategoryId == s.CategoryId).Name,
                                    Amount = s.Amount,
                                    Total = ""
                                };

                                if (s.PayeeId == id || s.PayeeId == null)
                                    Transactions.Add(trans);

                            }
                        }
                    }
                }
            }

            Transactions = Transactions.Where(t => t.Date.Year == currentDate.Year && t.Date.Month == currentDate.Month).OrderBy(t => t.Date).ThenByDescending(t => t.Amount).ToList();

            var currentBalance = 0L;
            foreach (var t in Transactions)
            {
                currentBalance += t.Amount;
                t.Total = currentBalance.ToDisplay();
            }
        }

        public class TransactionModel
        {
            public DateTime Date { get; set; }
            public string DateDisplay { get { return Date.ToShortDateString(); } }
            public string Category { get; set; }
            public long Amount { get; set; }
            public string AmountDisplay { get { return Amount.ToDisplay(); } }
            public string Total { get; set; }
        }
    }
}