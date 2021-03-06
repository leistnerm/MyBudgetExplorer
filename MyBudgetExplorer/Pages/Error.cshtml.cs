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
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MyBudgetExplorer.Pages
{
    public class ErrorModel : PageModel
    {
        private IMemoryCache _cache;

        public ErrorModel(IMemoryCache memoryCache)
        {
            _cache = memoryCache;
        }

        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public string ErrorDetails { get; set; } = "Technical details are unavailable";

        public void OnGet(string id = "")
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

            string details;
            _cache.TryGetValue(id, out details);
            if (!string.IsNullOrWhiteSpace(details))
                ErrorDetails = details;
        }
    }
}
