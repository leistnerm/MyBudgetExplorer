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

namespace MyBudgetExplorer.Models.YNAB
{
    [Serializable]
    public class BudgetSummary
    {
        #region Properties
        public string BudgetId { get; set; }
        public CurrencyFormat CurrencyFormat { get; set; }
        public DateFormat DateFormat { get; set; }
        public string FirstMonth { get; set; }
        public DateTime LastModifiedOn { get; set; }
        public string LastMonth { get; set; }
        public string Name { get; set; }
        #endregion

        #region Public Methods
        public static BudgetSummary Load(dynamic d)
        {
            return new BudgetSummary
            {
                BudgetId = d.budget_id,
                CurrencyFormat = CurrencyFormat.Load(d.currency_format),
                DateFormat = DateFormat.Load(d.date_format),
                FirstMonth = d.first_month,
                LastModifiedOn = d.last_modified_on,
                LastMonth = d.last_month,
                Name = d.name,
            };
        }
        #endregion
    }
}
