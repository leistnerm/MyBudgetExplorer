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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using MyBudgetExplorer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace MyBudgetExplorer.Pages.Budget
{
    [Authorize]
    public class IncomeModel : PageModel
    {
        private IConfiguration _configuration;
        private IMemoryCache _cache;
        public List<IncomeDataModel> Income { get; set; } = new List<IncomeDataModel>();
        public IncomeModel(IConfiguration configuration, IMemoryCache memoryCache)
        {
            _configuration = configuration;
            _cache = memoryCache;
        }
        public void OnGet()
        {
            Forecast forecast = null;
            var accessToken = HttpContext.GetTokenAsync("access_token").Result;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            _cache.TryGetValue(userId, out forecast);
            if (forecast == null)
            {
                forecast = Cache.GetForecast(accessToken, userId);
                _cache.Set(userId, forecast);
            }

            ViewData["Title"] = "Explore > Income";
            ViewData["LastUpdated"] = forecast.LastModifiedOn;
            foreach (var t in forecast.Transactions.Where(t => t.ImportId == "scheduled" && t.Amount > 0).OrderBy(t => t.Date).ThenBy(t => t.Amount))
            {
                var cDate = t.Date;
                var fDate = cDate.AddMonths(1);

                var fundStatus = forecast.MonthFundStatus[t.TransactionId];
                var currentFunded = fundStatus.Where(s => s.Date.Year == cDate.Year && s.Date.Month == cDate.Month).Sum(s => s.Funded);
                var currentNeeded = fundStatus.Where(s => s.Date.Year == cDate.Year && s.Date.Month == cDate.Month).Sum(s => s.Amount);
                var futureFunded = fundStatus.Where(s => s.Date.Year == fDate.Year && s.Date.Month == fDate.Month).Sum(s => s.Funded);
                var futureNeeded = fundStatus.Where(s => s.Date.Year == fDate.Year && s.Date.Month == fDate.Month).Sum(s => s.Amount);
                var income = new IncomeDataModel
                {
                    Date = t.Date.ToShortDateString(),
                    Payee = forecast.Payees.Single(p => p.PayeeId == t.PayeeId).Name,
                    Amount = t.Amount.ToDisplay(),
                    Available = null
                };
                if (currentNeeded != 0)
                    income.CurrentFundingPercentage = Math.Abs(decimal.Divide(currentFunded, currentNeeded) * 100M).ToString("N2");
                else
                    income.CurrentFundingPercentage = "0.00";
                if (futureNeeded != 0)
                    income.FutureFundingPercentage = Math.Abs(decimal.Divide(futureFunded, futureNeeded) * 100M).ToString("N2");
                else
                    income.FutureFundingPercentage = "0.00";

                var funding = forecast.GetIncomeFunding(t.TransactionId);
                var available = funding.SingleOrDefault(f => f.CategoryId == forecast.RemainingFundsCategoryId);
                if (available != null)
                    income.Available = available.Amount.ToDisplay();

                foreach (var fund in funding.Where(f => f.CategoryId != forecast.RemainingFundsCategoryId))
                {
                    income.Projected.Add(new ProjectedModel
                    {
                        Date = fund.Date.ToShortDateString(),
                        Payee = fund.Payee,
                        Category = fund.CategoryName,
                        Amount = (fund.Amount * -1).ToDisplay()
                    });
                }

                Income.Add(income);
            }
        }
        public class IncomeDataModel
        {
            public string Date { get; set; }
            public string Payee { get; set; }
            public string Amount { get; set; }
            public string CurrentFundingPercentage { get; set; }
            public string FutureFundingPercentage { get; set; }
            public string Available { get; set; }
            public List<ProjectedModel> Projected { get; set; }
            public IncomeDataModel()
            {
                Projected = new List<ProjectedModel>();
            }
        }

        public class ProjectedModel
        {
            public string Date { get; set; }
            public string Payee { get; set; }
            public string Category { get; set; }
            public string Amount { get; set; }
        }
    }
}