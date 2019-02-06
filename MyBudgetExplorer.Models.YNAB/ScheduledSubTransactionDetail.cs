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

namespace MyBudgetExplorer.Models.YNAB
{
    [Serializable]
    public class ScheduledSubTransactionDetail
    {
        #region Properties
        public string AccountName { get; set; }
        public int Amount { get; set; }
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public bool Deleted { get; set; }
        public string Memo { get; set; }
        public string PayeeId { get; set; }
        public string PayeeName { get; set; }
        public string ScheduledSubTransactionId { get; set; }
        public string ScheduledTransactionId { get; set; }
        public string TransferAccountId { get; set; }
        public IList<ScheduledSubTransaction> SubTransactions { get; set; } = new List<ScheduledSubTransaction>();
        #endregion
    }
}
