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
using Microsoft.Extensions.Configuration;
using MyBudgetExplorer.Models;
using MyBudgetExplorer.Models.YNAB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace MyBudgetExplorer.Pages.Budget
{
    public class AccountModel : PageModel
    {
        private IConfiguration _configuration;
        public List<TransactionModel> Transactions { get; set; }
        public string Previous { get; set; }
        public string Next { get; set; }
        public string Id { get; set; }
        public DateTime Date { get; set; }
        public AccountModel(IConfiguration configuration)
        {
            _configuration = configuration;
            Transactions = new List<TransactionModel>();
        }
        public void OnGet(string id, string date = null)
        {
            Id = id;

            string accessToken = HttpContext.GetTokenAsync("access_token").Result;
            var forecast = Cache.GetForecast(accessToken, User.FindFirst(ClaimTypes.NameIdentifier).Value);
            ViewData["LastUpdated"] = forecast.LastModifiedOn;

            var currentDate = forecast.CurrentMonthStart;
            if (date != null)
                DateTime.TryParse(date, out currentDate);

            currentDate = new DateTime(currentDate.Year, currentDate.Month, 1);

            Date = currentDate;
            Previous = currentDate.AddMonths(-1).ToShortDateString();
            Next = currentDate.AddMonths(1).ToShortDateString();

            var account = forecast.Accounts.Single(a => a.AccountId == id);
            ViewData["Title"] = $"Explore > {account.Name}";

            var transactions = forecast.Transactions
                .Where(t => !t.Deleted && t.AccountId == account.AccountId)
                .Where(t => t.Date < currentDate.AddMonths(1))
                .OrderBy(t => t.Date)
                .ThenByDescending(t => t.Amount);

            foreach (var t in transactions)
            {
                var trans = new TransactionModel
                {
                    Date = t.Date,
                    Payee = forecast.Payees.Single(p => p.PayeeId == t.PayeeId).Name,
                    Amount = t.Amount,
                    Available = ""
                };

                if (t.ImportId == "projected")
                {
                    var cat = forecast.Categories.Single(c => c.CategoryId == t.CategoryId);
                    trans.Payee += $" ({cat.Name})";
                }

                Transactions.Add(trans);
            }

            var currentBalance = 0;
            foreach (var t in Transactions)
            {
                currentBalance += t.Amount;
                t.Available = currentBalance.ToDisplay();
            }

            Transactions = Transactions.Where(t => t.Date.Year == currentDate.Year && t.Date.Month == currentDate.Month).ToList();
        }
        public class TransactionModel
        {
            public DateTime Date { get; set; }
            public string DateDisplay { get { return Date.ToShortDateString(); } }
            public string Payee { get; set; }
            public int Amount { get; set; }
            public string AmountDisplay { get { return Amount.ToDisplay(); } }
            public string Available { get; set; }
        }
    }
}