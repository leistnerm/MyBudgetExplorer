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
    public class ScheduledTransactionDetail
    {
        #region Properties
        public string AccountId { get; set; }
        public string AccountName { get; set; }
        public int Amount { get; set; }
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public DateTime DateFirst { get; set; }
        public DateTime DateNext { get; set; }
        public bool Deleted { get; set; }
        public FlagColor? FlagColor { get; set; }
        public Frequency Frequency { get; set; }
        public string Memo { get; set; }
        public string PayeeId { get; set; }
        public string PayeeName { get; set; }
        public string ScheduledTransactionId { get; set; }
        public IList<ScheduledSubTransaction> SubTransactions { get; set; } = new List<ScheduledSubTransaction>();
        public string TransferAccountId { get; set; }
        #endregion

        #region Public Methods
        public static ScheduledTransactionDetail Load(dynamic d)
        {
            var result = new ScheduledTransactionDetail
            {
                AccountId = d.account_id,
                AccountName = d.account_name,
                Amount = d.amount,
                CategoryId = d.category_id,
                CategoryName = d.category_name,
                DateFirst = d.date_first,
                DateNext = d.date_next,
                Deleted = d.deleted,
                FlagColor = d.flag_color,
                Frequency = d.frequency,
                Memo = d.memo,
                PayeeId = d.payee_id,
                PayeeName = d.payee_name,
                ScheduledTransactionId = d.id,
                TransferAccountId = d.transfer_account_id,
            };
            foreach (var s in d.subtransactions)
                result.SubTransactions.Add(ScheduledSubTransaction.Load(s));
            return result;
        }
        #endregion
    }
}
