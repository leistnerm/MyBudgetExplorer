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
    public class ScheduledTransactionSummary : ISerializable
    {
        #region Properties
        public string AccountId { get; set; }
        public int Amount { get; set; }
        public string CategoryId { get; set; }
        public DateTime DateFirst { get; set; }
        public DateTime DateNext { get; set; }
        public bool Deleted { get; set; }
        public FlagColor? FlagColor { get; set; }
        public Frequency Frequency { get; set; }
        public string Memo { get; set; }
        public string PayeeId { get; set; }
        public string ScheduledTransactionId { get; set; }
        public string TransferAccountId { get; set; }
        #endregion

        #region Constructors
        public ScheduledTransactionSummary() { }
        #endregion

        #region Public Methods
        public static ScheduledTransactionSummary Load(dynamic d)
        {
            return new ScheduledTransactionSummary
            {
                AccountId = d.account_id,
                Amount = d.amount,
                CategoryId = d.category_id,
                DateFirst = d.date_first,
                DateNext = d.date_next,
                Deleted = d.deleted,
                FlagColor = d.flag_color,
                Frequency = d.frequency,
                Memo = d.memo,
                PayeeId = d.payee_id,
                ScheduledTransactionId = d.id,
                TransferAccountId = d.transfer_account_id,
            };
        }
        #endregion

        #region ISerializable
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            var writer = SerializationWriter.GetWriter();

            writer.WriteString(AccountId);
            writer.Write(Amount);
            writer.WriteString(CategoryId);
            writer.Write(DateFirst);
            writer.Write(DateNext);
            writer.Write(Deleted);
            writer.Write(FlagColor.HasValue);
            if (FlagColor.HasValue)
                writer.Write((byte)FlagColor.Value);
            writer.Write((byte)Frequency);
            writer.WriteString(Memo);
            writer.WriteString(PayeeId);
            writer.WriteString(ScheduledTransactionId);
            writer.WriteString(TransferAccountId);

            writer.AddToInfo(info);
        }
        public ScheduledTransactionSummary(SerializationInfo info, StreamingContext context)
        {
            var reader = SerializationReader.GetReader(info);

            AccountId = reader.ReadString();
            Amount = reader.ReadInt32();
            CategoryId = reader.ReadString();
            DateFirst = reader.ReadDateTime();
            DateNext = reader.ReadDateTime();
            Deleted = reader.ReadBoolean();
            if (reader.ReadBoolean())
                FlagColor = (FlagColor)reader.ReadByte();
            Frequency = (Frequency)reader.ReadByte();
            Memo = reader.ReadString();
            PayeeId = reader.ReadString();
            ScheduledTransactionId = reader.ReadString();
            TransferAccountId = reader.ReadString();
        }
        #endregion
    }
}
