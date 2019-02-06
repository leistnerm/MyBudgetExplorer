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
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyBudgetExplorer.Models;
using MyBudgetExplorer.Models.YNAB;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace MyBudgetExplorer.Pages.Budget
{
    public class AccountsModel : PageModel
    {
        private IConfiguration _configuration;
        public List<AccountModel> Accounts { get; set; }
        public AccountsModel(IConfiguration configuration)
        {
            _configuration = configuration;
            Accounts = new List<AccountModel>();
        }
        public void OnGet()
        {
            ViewData["Title"] = "Explore > Accounts";
            string accessToken = HttpContext.GetTokenAsync("access_token").Result;
            var forecast = Cache.GetForecast(accessToken, User.FindFirst(ClaimTypes.NameIdentifier).Value, _configuration["AWS:AccessKey"], _configuration["AWS:SecretKey"]);
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