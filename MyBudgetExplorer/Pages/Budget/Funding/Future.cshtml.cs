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
using MyBudgetExplorer.Models;
using MyBudgetExplorer.Models.YNAB;
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
        public List<OutflowModel> Outflow { get; set; }
        public string Funded { get; set; }
        public string Needed { get; set; }
        public string Percent { get; set; }
        public FutureModel(IConfiguration configuration)
        {
            _configuration = configuration;
            Outflow = new List<OutflowModel>();
        }
        public void OnGet()
        {
            var currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1);
            ViewData["Title"] = $"Overview > Funding > {currentMonth.ToString("MMMM yyyy")}";

            string accessToken = HttpContext.GetTokenAsync("access_token").Result;
            var forecast = Cache.GetForecast(accessToken, User.FindFirst(ClaimTypes.NameIdentifier).Value, _configuration["AWS:AccessKey"], _configuration["AWS:SecretKey"]);
            ViewData["LastUpdated"] = forecast.LastModifiedOn;

            var expenses = forecast.MonthFundStatus[currentMonth.AddMonths(-1).ToShortDateString()]
                .Where(e => e.Date.Year == currentMonth.Year && e.Date.Month == currentMonth.Month)
                .OrderBy(e => e.Date)
                .ThenBy(e => e.Amount);
            
            foreach (var outflow in expenses)
            {
                    Outflow.Add(new OutflowModel
                    {
                        Amount = outflow.Amount.ToDisplay(),
                        Category = outflow.CategoryName,
                        Date = outflow.Date.ToShortDateString(),
                        Funded = outflow.Funded.ToDisplay(),
                        Payee = outflow.PayeeName,
                        Percent = Math.Abs(Decimal.Divide(outflow.Funded, outflow.Amount) * 100M).ToString("N2")
                    });
            }
            var funded = expenses.Sum(e => e.Funded);
            Funded = funded.ToDisplay();

            var needed = expenses.Sum(e => e.Amount);
            Needed = needed.ToDisplay();

            Percent = Math.Abs(Decimal.Divide(funded, needed) * 100M).ToString("N2");
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