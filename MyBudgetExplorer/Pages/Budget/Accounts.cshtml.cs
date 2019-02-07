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
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace MyBudgetExplorer.Pages.Budget
{
    public class AccountsModel : PageModel
    {
        private IConfiguration _configuration;
        private IMemoryCache _cache;
        public List<AccountModel> Accounts { get; set; } = new List<AccountModel>();
        public AccountsModel(IConfiguration configuration, IMemoryCache memoryCache)
        {
            _configuration = configuration;
            _cache = memoryCache;
        }
        public void OnGet()
        {
            ViewData["Title"] = "Explore > Accounts";

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
            foreach (var a in forecast.Accounts.Where(a => !a.Deleted && !a.Closed).OrderBy(a => a.Name))
            {
                Accounts.Add(new AccountModel
                {
                    AccountId = a.AccountId,
                    Name = a.Name,
                    Balance = a.Balance.ToDisplay(),
                    ClearedBalance = a.ClearedBalance.ToDisplay()
                });
            }
        }

        public class AccountModel
        {
            public string AccountId { get; set; }
            public string Name { get; set; }
            public string Balance { get; set; }
            public string ClearedBalance { get; set; }
        }
    }
}