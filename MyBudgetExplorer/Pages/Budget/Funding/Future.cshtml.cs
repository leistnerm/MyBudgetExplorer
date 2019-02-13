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

namespace MyBudgetExplorer.Pages.Budget.Funding
{
    [Authorize]
    public class FutureModel : PageModel
    {
        private IConfiguration _configuration;
        private IMemoryCache _cache;
        public List<OutflowModel> Outflow { get; set; } = new List<OutflowModel>();
        public string Funded { get; set; }
        public string Needed { get; set; }
        public string Percent { get; set; }
        public FutureModel(IConfiguration configuration, IMemoryCache memoryCache)
        {
            _configuration = configuration;
            _cache = memoryCache;
        }
        public void OnGet()
        {
            var currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1);
            ViewData["Title"] = $"Overview > Funding > {currentMonth.ToString("MMMM yyyy")}";

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

            var expenses = forecast.MonthFundStatus[currentMonth.AddMonths(-1).ToShortDateString()]
                .Where(e => e.Date.Year == currentMonth.Year && e.Date.Month == currentMonth.Month)
                .OrderBy(e => e.Date)
                .ThenBy(e => e.Amount);
            
            foreach (var outflow in expenses)
            {
                var of = new OutflowModel
                {
                    Amount = outflow.Amount.ToDisplay(),
                    Category = outflow.CategoryName,
                    Date = outflow.Date.ToShortDateString(),
                    Funded = outflow.Funded.ToDisplay(),
                    Payee = outflow.PayeeName,
                    Percent = "N/A"
                };
                if (outflow.Amount != 0)
                    of.Percent = Math.Abs(Decimal.Divide(outflow.Funded, outflow.Amount) * 100M).ToString("N2");

                Outflow.Add(of);
            }
            var funded = expenses.Sum(e => e.Funded);
            Funded = funded.ToDisplay();

            var needed = expenses.Sum(e => e.Amount);
            Needed = needed.ToDisplay();

            if (needed != 0)
                Percent = Math.Abs(Decimal.Divide(funded, needed) * 100M).ToString("N2");
            else
                Percent = "N/A";
        }

        public class OutflowModel
        {
            public string Date { get; set; }
            public string Payee { get; set; }
            public string Category { get; set; }
            public string Amount { get; set; }
            public string Funded { get; set; }
            public string Percent { get; set; }
        }
    }
}