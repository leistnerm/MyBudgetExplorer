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
    public class MonthSummary
    {
        #region Properties
        public long? Activity { get; set; }
        public int? AgeOfMoney { get; set; }
        public long? Budgeted { get; set; }
        public long? Income { get; set; }
        public DateTime Month { get; set; }
        public string Note { get; set; }
        public long? ToBeBudgeted { get; set; }
        #endregion

        #region Public Methods
        public static MonthSummary Load(dynamic dyn)
        {
            Func<dynamic, MonthSummary> func = (d) =>
            {
                return new MonthSummary
                {
                    Activity = d.activity,
                    AgeOfMoney = d.age_of_money,
                    Budgeted = d.budgeted,
                    Income = d.income,
                    Month = d.month,
                    Note = d.note,
                    ToBeBudgeted = d.to_be_budgeted,
                };
            };

            return YnabApi.ProcessApiResult(dyn, func);
        }
        #endregion
    }
}
