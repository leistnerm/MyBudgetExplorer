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
    public class TransactionDetail
    {
        #region Properties
        public string AccountId { get; set; }
        public string AccountName { get; set; }
        public long Amount { get; set; }
        public bool Approved { get; set; }
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public TransactionStatus Cleared { get; set; }
        public DateTime Date { get; set; }
        public bool Deleted { get; set; }
        public FlagColor? FlagColor { get; set; }
        public string ImportId { get; set; }
        public string Memo { get; set; }
        public string PayeeId { get; set; }
        public string PayeeName { get; set; }
        public IList<SubTransaction> SubTransactions { get; set; } = new List<SubTransaction>();
        public string TransactionId { get; set; }
        public string TransferAccountId { get; set; }
        public string TransferTransactionId { get; set; }
        #endregion

        #region Public Methods
        public static TransactionDetail Load(dynamic d)
        {
            try
            {
                var result = new TransactionDetail
                {
                    AccountId = d.account_id,
                    AccountName = d.account_name,
                    Amount = d.amount,
                    Approved = d.approved,
                    CategoryId = d.category_id,
                    CategoryName = d.category_name,
                    Cleared = d.cleared,
                    Date = d.date,
                    Deleted = d.deleted,
                    FlagColor = d.flag_color,
                    ImportId = d.import_id,
                    Memo = d.memo,
                    PayeeId = d.payee_id,
                    PayeeName = d.payee_name,
                    TransactionId = d.id,
                    TransferAccountId = d.transfer_account_id,
                    TransferTransactionId = d.transfer_transaction_id,
                };
                foreach (var s in d.subtransactions)
                    result.SubTransactions.Add(SubTransaction.Load(s));

                return result;
            }
            catch (Exception e)
            {
                if (!e.Data.Contains("json"))
                    e.Data.Add("json", d.ToString());
                throw e;
            }
        }
        #endregion
    }
}