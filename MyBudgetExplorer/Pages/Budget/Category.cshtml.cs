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
using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
    public class CategoryModel : PageModel
    {
        private IConfiguration _configuration;
        private IMemoryCache _cache;
        public string LastMonthAbbreviation { get; set; }
        public string LastMonthAvailable { get; set; }
        public string Budgeted { get; set; }
        public string Activity { get; set; }
        public string Available { get; set; }
        public List<TransactionModel> Transactions { get; set; } = new List<TransactionModel>();
        public string Previous { get; set; }
        public string Next { get; set; }
        public string Id { get; set; }
        public DateTime Date { get; set; }
        public CategoryModel(IConfiguration configuration, IMemoryCache memoryCache)
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

            var currentDate = forecast.CurrentMonthStart;
            if (date != null)
                DateTime.TryParse(date, out currentDate);

            currentDate = new DateTime(currentDate.Year, currentDate.Month, 1);

            var month = forecast.Months.Single(m => m.Month == currentDate);
            var category = month.Categories.SingleOrDefault(c => c.CategoryId == id);
            if (category == null)
            {
                var ex = new ApplicationException("The specified category could not be found.");
                ex.Data.Add("Category Id", id);
                throw ex;
            }

            ViewData["LastUpdated"] = forecast.LastModifiedOn;

            Date = currentDate;
            Previous = currentDate.AddMonths(-1).ToShortDateString();
            Next = currentDate.AddMonths(1).ToShortDateString();

            LastMonthAbbreviation = currentDate.AddMonths(-1).ToString("MMM yy");

            // Transactions
            foreach (var t in forecast.Transactions.Where(t => currentDate <= t.Date && t.Date < currentDate.AddMonths(1)))
            {
                if (t.CategoryId == id)
                {
                    Transactions.Add(new TransactionModel
                    {
                        Date = t.Date,
                        Payee = forecast.Payees.Single(p => p.PayeeId == t.PayeeId).Name,
                        Amount = t.Amount,
                        Available = ""
                    });
                }
                else
                {
                    foreach (var s in forecast.SubTransactions)
                    {
                        if (s.TransactionId == t.TransactionId && s.CategoryId == id)
                        {
                            Transactions.Add(new TransactionModel
                            {
                                Date = t.Date,
                                Payee = forecast.Payees.Single(p => p.PayeeId == t.PayeeId).Name,
                                Amount = s.Amount,
                                Available = ""
                            });
                        }
                    }
                }
            }
            var budgetedThisMonth = 0L;
            // Budgeted Income
            foreach (var t in forecast.Transactions.Where(t => currentDate <= t.Date && t.Date < currentDate.AddMonths(1) && t.ImportId == "scheduled" && t.Amount > 0))
            {
                var funding = forecast.GetIncomeFunding(t.TransactionId);
                foreach (var f in funding.Where(f => f.CategoryId == id))
                {
                    var trans = new TransactionModel
                    {
                        Date = t.Date,
                        Payee = $"[Projected Budget: {forecast.Payees.Single(p => p.PayeeId == t.PayeeId).Name} ({f.Date.ToShortDateString()})]",
                        Amount = f.Amount,
                        Available = ""
                    };
                    if (f.Date >= currentDate.AddMonths(1))
                        trans.Payee = trans.Payee.Replace("Projected Budget", "Projected Budget Forward");
                    else
                        budgetedThisMonth += trans.Amount;
                    Transactions.Add(trans);
                }
            }

            Budgeted = category.Budgeted.ToDisplay();
            Activity = category.Activity.ToDisplay();
            Available = category.Balance.ToDisplay();

            var group = forecast.CategoryGroups.Single(g => g.CategoryGroupId == category.CategoryGroupId);
            ViewData["Title"] = $"Explore > {group.Name} > {category.Name}";
            var budgeted = forecast.GetOriginalBudgeted(month.Month, id);

            if (budgeted > 0)
                Transactions.Add(new TransactionModel
                {
                    Date = month.Month,
                    Payee = $"[Budgeted {month.Month.ToString("MMM yy")}]",
                    Amount = budgeted,
                    Available = ""
                });
            if (category.Budgeted - budgeted > 0)
            {
                var tempBudgeted = category.Budgeted - budgeted - budgetedThisMonth;
                if (tempBudgeted > 0)
                Transactions.Add(new TransactionModel
                {
                    Date = month.Month,
                    Payee = $"[Projected Budgeted {month.Month.ToString("MMM yy")}]",
                    Amount = category.Budgeted - budgeted - budgetedThisMonth,
                    Available = ""
                });
            }

            var lastMonth = forecast.Months.SingleOrDefault(m => m.Month == currentDate.AddMonths(-1));
            if (lastMonth != null)
            {
                var lastMonthCategory = lastMonth.Categories.SingleOrDefault(c => c.CategoryId == id);
                if (lastMonthCategory != null)
                {
                    Transactions.Add(new TransactionModel
                    {
                        Date = currentDate.AddDays(-1),
                        Payee = $"[From {LastMonthAbbreviation}]",
                        Amount = lastMonthCategory.Balance,
                        Available = lastMonthCategory.Balance.ToDisplay()
                    });
                    LastMonthAvailable = lastMonthCategory.Balance.ToDisplay();
                }
            }

            Transactions = Transactions.OrderBy(t => t.Date).ThenByDescending(t => t.Amount).ToList();

            var currentBalance = 0L;
            foreach (var t in Transactions)
            {
                currentBalance += t.Amount;
                t.Available = currentBalance.ToDisplay();
            }

            //Transactions = Transactions.Where(t => t.Date.Year == currentDate.Year && t.Date.Month == currentDate.Month).ToList();

        }

        public class TransactionModel
        {
            public DateTime Date { get; set; }
            public string DateDisplay { get { return Date.ToShortDateString(); } }
            public string Payee { get; set; }
            public long Amount { get; set; }
            public string AmountDisplay { get { return Amount.ToDisplay(); } }
            public string Available { get; set; }
        }
    }
}