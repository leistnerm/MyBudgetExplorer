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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using MyBudgetExplorer.Models;
using System;
using System.Linq;
using System.Security.Claims;

namespace MyBudgetExplorer.Pages.Budget
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private IConfiguration _configuration;
        private IMemoryCache _cache;
        public string NextIncomeDate { get; set; }
        public string NextIncomePayee { get; set; }
        public string NextIncomeAmount { get; set; }
        public string LastUpdated { get; set; }
        public string Current { get; set; }
        public string CurrentFunding { get; set; }
        public string CurrentFundingPercentage { get; set; }
        public string Future { get; set; }
        public string FutureFunding { get; set; }
        public string FutureFundingPercentage { get; set; }
        public IndexModel(IConfiguration configuration, IMemoryCache memoryCache)
        {
            _configuration = configuration;
            _cache = memoryCache;
        }

        public void OnGet()
        {
            Forecast forecast = null;
            var accessToken = HttpContext.GetTokenAsync("access_token").Result;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            _cache.TryGetValue<Forecast>(userId, out forecast);
            if (forecast == null)
            {
                forecast = Cache.GetForecast(accessToken, userId);
                _cache.Set<Forecast>(userId, forecast);
            }

            ViewData["Title"] = $"Overview > {forecast.Name}";

            var currentDate = DateTime.Now;
            var currentMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
            var futureMonth = currentMonth.AddMonths(1);

            var fundingStatus = forecast.MonthFundStatus[currentMonth.ToShortDateString()];
            var currentNeeded = Math.Abs(fundingStatus.Where(s => s.Date.Year == currentMonth.Year && s.Date.Month == currentMonth.Month).Sum(s => s.Amount));
            var currentFunded = Math.Abs(fundingStatus.Where(s => s.Date.Year == currentMonth.Year && s.Date.Month == currentMonth.Month).Sum(s => s.Funded));
            var futureNeeded = Math.Abs(fundingStatus.Where(s => s.Date.Year == futureMonth.Year && s.Date.Month == futureMonth.Month).Sum(s => s.Amount));
            var futureFunded = Math.Abs(fundingStatus.Where(s => s.Date.Year == futureMonth.Year && s.Date.Month == futureMonth.Month).Sum(s => s.Funded));

            Current = currentDate.ToString("MMMM");
            CurrentFunding = $"{Math.Abs(currentFunded).ToDisplay()} of {Math.Abs(currentNeeded).ToDisplay()}";
            if (currentNeeded != 0)
                CurrentFundingPercentage = Math.Abs(decimal.Divide(currentFunded, currentNeeded) * 100M).ToString("N2");
            else
                CurrentFundingPercentage = "0.00";
            FutureFunding = $"{Math.Abs(futureFunded).ToDisplay()} of {Math.Abs(futureNeeded).ToDisplay()}";
            if (futureNeeded != 0)
                FutureFundingPercentage = Math.Abs(decimal.Divide(futureFunded, futureNeeded) * 100M).ToString("N2");
            else
                FutureFundingPercentage = "0.00";
            Future = futureMonth.ToString("MMMM");

            var nextIncome = forecast.Transactions.Where(t => t.ImportId == "scheduled" && t.Amount > 0).OrderBy(t => t.Date).FirstOrDefault();
            if (nextIncome == null)
            {
                NextIncomeAmount = "0.00";
                NextIncomeDate = "N/A";
                NextIncomePayee = "None Scheduled";
            }
            else
            {
                NextIncomeAmount = nextIncome.Amount.ToDisplay();
                NextIncomeDate = nextIncome.Date.ToShortDateString();
                NextIncomePayee = forecast.Payees.Single(p => p.PayeeId == nextIncome.PayeeId).Name;
            }
            ViewData["LastUpdated"] = forecast.LastModifiedOn;
        }

        public IActionResult OnGetRefresh(string path)
        {
            string accessToken = HttpContext.GetTokenAsync("access_token").Result;
            string userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            Cache.GetApiBudget(accessToken, userId);

            _cache.Remove(userId);

            return Redirect(path);
        }
    }
}