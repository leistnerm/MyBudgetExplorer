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
using MyBudgetExplorer.Models.BinarySerialization;
using System;
using System.Runtime.Serialization;

namespace MyBudgetExplorer.Models.YNAB
{
    [Serializable]
    public class ScheduledSubTransaction : ISerializable
    {
        #region Properties
        public long Amount { get; set; }
        public string CategoryId { get; set; }
        public bool Deleted { get; set; }
        public string Memo { get; set; }
        public string PayeeId { get; set; }
        public string ScheduledSubTransactionId { get; set; }
        public string ScheduledTransactionId { get; set; }
        public string TransferAccountId { get; set; }
        #endregion

        #region Constructors
        public ScheduledSubTransaction() { }
        #endregion

        #region Public Methods
        public static ScheduledSubTransaction Load(dynamic dyn)
        {
            Func<dynamic, ScheduledSubTransaction> func = (d) =>
            {
                var st = new ScheduledSubTransaction
                {
                    Amount = d.amount,
                    CategoryId = d.category_id,
                    Deleted = d.deleted,
                    Memo = d.memo,
                    PayeeId = d.payee_id,
                    ScheduledSubTransactionId = d.id,
                    ScheduledTransactionId = d.scheduled_transaction_id,
                    TransferAccountId = d.transfer_account_id,
                };
                return st;
            };

            return YnabApi.ProcessApiResult(dyn, func);
        }
        #endregion

        #region ISerializable
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            var writer = SerializationWriter.GetWriter();

            writer.Write(Amount);
            writer.WriteString(CategoryId);
            writer.Write(Deleted);
            writer.WriteString(Memo);
            writer.WriteString(PayeeId);
            writer.WriteString(ScheduledSubTransactionId);
            writer.WriteString(ScheduledTransactionId);
            writer.WriteString(TransferAccountId);

            writer.AddToInfo(info);
        }
        public ScheduledSubTransaction(SerializationInfo info, StreamingContext context)
        {
            var reader = SerializationReader.GetReader(info);

            Amount = reader.ReadInt64();
            CategoryId = reader.ReadString();
            Deleted = reader.ReadBoolean();
            Memo = reader.ReadString();
            PayeeId = reader.ReadString();
            ScheduledSubTransactionId = reader.ReadString();
            ScheduledTransactionId = reader.ReadString();
            TransferAccountId = reader.ReadString();
        }
        #endregion
    }
}
