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
using MyBudgetExplorer.Models.YNAB;
using System;
using System.Runtime.Serialization;

namespace MyBudgetExplorer.Models
{
    [Serializable]
    public class ForecastItem : ISerializable
    {
        #region Properties
        public string AccountId { get; set; }
        public int Amount { get; set; }
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public DateTime Date { get; set; }
        public FlagColor? FlagColor { get; set; }
        public ForecastItemType ForecastItemType { get; set; }
        public int Funded { get; set; }
        public bool IsSplit { get; set; }
        public string Memo { get; set; }
        public string PayeeId { get; set; }
        public string PayeeName { get; set; }
        public int Remaining { get { return Amount + Funded; } }
        public string ScheduledSubTransactionId { get; set; }
        public string ScheduledTransactionId { get; set; }
        public string SubTransactionId { get; set; }
        public string TransactionId { get; set; }
        public string TransferAccountId { get; set; }
        #endregion

        #region Constructors
        public ForecastItem() { }
        #endregion

        #region ISerializable
        public ForecastItem(SerializationInfo info, StreamingContext context)
        {
            var reader = SerializationReader.GetReader(info);

            AccountId = reader.ReadString();
            Amount = reader.ReadInt32();
            CategoryId = reader.ReadString();
            CategoryName = reader.ReadString();
            Date = reader.ReadDateTime();
            if (reader.ReadBoolean())
                FlagColor = (FlagColor)reader.ReadByte();
            ForecastItemType = (ForecastItemType)reader.ReadByte();
            Funded = reader.ReadInt32();
            IsSplit = reader.ReadBoolean();
            Memo = reader.ReadString();
            PayeeId = reader.ReadString();
            PayeeName = reader.ReadString();
            ScheduledSubTransactionId = reader.ReadString();
            ScheduledTransactionId = reader.ReadString();
            SubTransactionId = reader.ReadString();
            TransactionId = reader.ReadString();
            TransferAccountId = reader.ReadString();
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            var writer = SerializationWriter.GetWriter();

            writer.WriteString(AccountId);
            writer.Write(Amount);
            writer.WriteString(CategoryId);
            writer.WriteString(CategoryName);
            writer.Write(Date);
            writer.Write(FlagColor.HasValue);
            if (FlagColor.HasValue)
                writer.Write((byte)FlagColor.Value);
            writer.Write((byte)ForecastItemType);
            writer.Write(Funded);
            writer.Write(IsSplit);
            writer.WriteString(Memo);
            writer.WriteString(PayeeId);
            writer.WriteString(PayeeName);
            writer.WriteString(ScheduledSubTransactionId);
            writer.WriteString(ScheduledTransactionId);
            writer.WriteString(SubTransactionId);
            writer.WriteString(TransactionId);
            writer.WriteString(TransferAccountId);

            writer.AddToInfo(info);
        }
        #endregion
    }
}
