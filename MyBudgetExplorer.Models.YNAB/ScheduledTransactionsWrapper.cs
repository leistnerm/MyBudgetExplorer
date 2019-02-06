﻿/* 
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

namespace MyBudgetExplorer.Models.YNAB
{
    [Serializable]
    public class ScheduledTransactionsWrapper
    {
        #region Properties
        public IList<ScheduledTransactionDetail> ScheduledTransactions { get; set; } = new List<ScheduledTransactionDetail>();
        #endregion

        #region Public Methods
        public static ScheduledTransactionsWrapper Load(dynamic d)
        {
            var result = new ScheduledTransactionsWrapper();
            foreach (var t in d.scheduled_transactions)
                result.ScheduledTransactions.Add(ScheduledTransactionDetail.Load(t));
            return result;
        }
        #endregion
    }
}
