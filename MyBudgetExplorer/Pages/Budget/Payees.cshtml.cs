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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using MyBudgetExplorer.Models;

namespace MyBudgetExplorer.Pages.Budget
{
    public class PayeesModel : PageModel
    {
        private IConfiguration _configuration;
        private IMemoryCache _cache;
        public List<PayeeModel> Payees { get; set; } = new List<PayeeModel>();
        public DateTime Date { get; set; }
        public PayeesModel(IConfiguration configuration, IMemoryCache memoryCache)
        {
            _configuration = configuration;
            _cache = memoryCache;
        }

        public void OnGet()
        {
            ViewData["Title"] = "Explore > Payees";

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
            Date = forecast.CurrentMonthStart;

            var validPayees = forecast.Transactions.Where(t => !t.Deleted).Select(t => t.PayeeId)
                .Union(forecast.SubTransactions.Where(t => !t.Deleted).Select(t => t.PayeeId))
                .Distinct()
                .ToList();

            foreach (var p in forecast.Payees.Where(p=> !p.Deleted).OrderBy(p => p.Name))
            {
                // If the payee has no transactions, don't display it.
                if (!validPayees.Contains(p.PayeeId))
                    continue;

                var payee = new PayeeModel
                {
                    PayeeId = p.PayeeId,
                    Name = p.Name,
                };

                payee.Current = forecast.Transactions.Where(t => t.PayeeId == p.PayeeId)
                    .Where(t => t.Date.Year == forecast.CurrentMonthStart.Year && t.Date.Month == forecast.CurrentMonthStart.Month)
                    .Sum(t => t.Amount).ToDisplay();

                var monthCount = forecast.Months.Count(m => m.Month < forecast.CurrentMonthStart);

                if (monthCount > 0)
                {
                    var previous = forecast.Transactions.Where(t => t.PayeeId == p.PayeeId)
                        .Where(t => t.Date < forecast.CurrentMonthStart)
                        .Sum(t => t.Amount);
                    payee.Average = Convert.ToInt64(decimal.Divide(previous, monthCount)).ToDisplay();
                }
                else
                    payee.Average = 0L.ToDisplay();

                Payees.Add(payee);
            }
        }

        public class PayeeModel
        {
            public string PayeeId { get; set; }
            public string Name { get; set; }
            public string Current { get; set; }
            public string Average { get; set; }
        }
    }
}