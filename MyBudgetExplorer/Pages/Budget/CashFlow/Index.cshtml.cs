using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using MyBudgetExplorer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace MyBudgetExplorer.Pages.Budget.CashFlow
{
    public class IndexModel : PageModel
    {
        private IConfiguration _configuration;
        private IMemoryCache _cache;

        public List<Tuple<string, string, long>> GraphValues { get; set; } = new List<Tuple<string, string, long>>();
        public List<Tuple<string, string>> Accounts { get; set; } = new List<Tuple<string, string>>();
        public List<Tuple<string, string>> StartDates { get; set; } = new List<Tuple<string, string>>();
        public List<Tuple<string, string>> EndDates { get; set; } = new List<Tuple<string, string>>();

        public IndexModel(IConfiguration configuration, IMemoryCache memoryCache)
        {
            _configuration = configuration;
            _cache = memoryCache;
        }

        public void OnGet()
        {
            ViewData["Title"] = $"Explore > Cash Flow";

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

            foreach (var account in forecast.Accounts)
                Accounts.Add(new Tuple<string, string>(account.AccountId, account.Name));

            StartDates = forecast.Months.OrderBy(m => m.Month).Select(m => new Tuple<string, string>(m.Month.ToShortDateString(), m.Month.ToString("MMM yy"))).ToList();
            EndDates = forecast.Months.OrderBy(m => m.Month).Select(m => new Tuple<string, string>(m.Month.AddMonths(1).ToShortDateString(), m.Month.ToString("MMM yy"))).ToList();
        }

        public void OnPost(List<string> accounts, string start = "", string end = "")
        {
            DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime.TryParse(start, out startDate);

            DateTime endDate = startDate.AddMonths(4);
            DateTime.TryParse(end, out endDate);

            ViewData["Title"] = $"Explore > Cash Flow";

            Forecast forecast = null;
            var accessToken = HttpContext.GetTokenAsync("access_token").Result;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            _cache.TryGetValue(userId, out forecast);
            if (forecast == null)
            {
                forecast = Cache.GetForecast(accessToken, userId);
                _cache.Set(userId, forecast);
            }

            foreach (var account in forecast.Accounts.Where(a => accounts.Contains(a.AccountId)))
                Accounts.Add(new Tuple<string, string>(account.AccountId, account.Name));

            var accountBalances = forecast.Accounts.Where(a => accounts.Contains(a.AccountId)).ToDictionary(k => k.AccountId, v => 0L);
            var transactionsByDay = forecast.Transactions.Where(t => accounts.Contains(t.AccountId)).GroupBy(g => g.Date.Date).ToDictionary(k => k.Key, v => v.ToList());
            var day = forecast.Transactions.Min(t => t.Date.Date);
            while (day < forecast.ForecastUntil)
            {
                if (day >= endDate)
                    break;

                if (transactionsByDay.ContainsKey(day))
                {
                    var accountActivity = transactionsByDay[day].GroupBy(g => g.AccountId).ToDictionary(k => k.Key, v => v.Sum(t => t.Amount));
                    foreach (var accountId in accountActivity.Keys)
                        accountBalances[accountId] += accountActivity[accountId];
                }

                foreach (var accountId in accountBalances.Keys)
                    GraphValues.Add(new Tuple<string, string, long>(day.ToShortDateString(), accountId, accountBalances[accountId]));

                day = day.AddDays(1);
            }

            GraphValues = GraphValues.Where(t => DateTime.Parse(t.Item1) >= startDate).ToList();

            ViewData["LastUpdated"] = forecast.LastModifiedOn;
        }
    }
}