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
    public class BudgetSummaryResponse
    {
        #region Properties
        public BudgetSummaryWrapper Data { get; set; } = new BudgetSummaryWrapper();
        #endregion

        #region Public Methods
        public static BudgetSummaryResponse Load(dynamic dyn)
        {
            Func<dynamic, BudgetSummaryResponse> func = (d) =>
            {
                return new BudgetSummaryResponse
                {
                    Data = BudgetSummaryWrapper.Load(d.data)
                };
            };

            return YnabApi.ProcessApiResult(dyn, func);
        }
        #endregion
    }
}
