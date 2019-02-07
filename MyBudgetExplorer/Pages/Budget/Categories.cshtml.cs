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
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace MyBudgetExplorer.Pages.Budget
{
    [Authorize]
    public class CategoriesModel : PageModel
    {
        private IConfiguration _configuration;
        private IMemoryCache _cache;
        public List<CategoryGroupModel> CategoryGroups { get; set; } = new List<CategoryGroupModel>();
        public CategoriesModel(IConfiguration configuration, IMemoryCache memoryCache)
        {
            _configuration = configuration;
            _cache = memoryCache;
        }
        public void OnGet()
        {
            ViewData["Title"] = "Explore > Categories";

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
            foreach (var g in forecast.CategoryGroups.Where(g => g.Name != "Internal Master Category" && !g.Deleted))
            {
                var categoryGroup = new CategoryGroupModel
                {
                    Name = g.Name,
                    Budgeted = forecast.Categories.Where(c => c.CategoryGroupId == g.CategoryGroupId && !c.Deleted).Sum(c => c.Budgeted).ToDisplay(),
                    Available = forecast.Categories.Where(c => c.CategoryGroupId == g.CategoryGroupId && !c.Deleted).Sum(c => c.Balance).ToDisplay()
                };

                foreach (var c in forecast.Categories.Where(c => c.CategoryGroupId == g.CategoryGroupId && !c.Deleted))
                {
                    var category = new CategoryModel
                    {
                        CategoryId = c.CategoryId,
                        Name = c.Name,
                        Budgeted = c.Budgeted.ToDisplay(),
                        Available = c.Balance.ToDisplay()
                    };
                    categoryGroup.Categories.Add(category);
                }

                if (categoryGroup.Categories.Count > 0)
                    CategoryGroups.Add(categoryGroup);
            }
        }

        public class CategoryGroupModel
        {
            public string Name { get; set; }
            public string Budgeted { get; set; }
            public string Available { get; set; }
            public List<CategoryModel> Categories { get; set; }
            public CategoryGroupModel()
            {
                Categories = new List<CategoryModel>();
            }
        }

        public class CategoryModel
        {
            public string Name { get; set; }
            public string CategoryId { get; set; }
            public string Budgeted { get; set; }
            public string Available { get; set; }
        }
    }
}