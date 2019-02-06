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
    public class TransactionSummary : ISerializable
    {
        #region Properties
        public string AccountId { get; set; }
        public int Amount { get; set; }
        public bool Approved { get; set; }
        public string CategoryId { get; set; }
        public TransactionStatus Cleared { get; set; }
        public DateTime Date { get; set; }
        public bool Deleted { get; set; }
        public FlagColor? FlagColor { get; set; }
        public string ImportId { get; set; }
        public string Memo { get; set; }
        public string PayeeId { get; set; }
        public string TransactionId { get; set; }
        public string TransferAccountId { get; set; }
        public string TransferTransactionId { get; set; }
        #endregion

        #region Constructors
        public TransactionSummary() { }
        #endregion

        #region Public Methods
        public static TransactionSummary Load(dynamic d)
        {
            return new TransactionSummary
            {
                AccountId = d.account_id,
                Amount = d.amount,
                Approved = d.approved,
                CategoryId = d.category_id,
                Cleared = d.cleared,
                Date = d.date,
                Deleted = d.deleted,
                FlagColor = d.flag_color,
                ImportId = d.import_id,
                Memo = d.memo,
                PayeeId = d.payee_id,
                TransactionId = d.id,
                TransferAccountId = d.transfer_account_id,
                TransferTransactionId = d.transfer_transaction_id,
            };
        }
        #endregion

        #region ISerializable
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            var writer = SerializationWriter.GetWriter();

            writer.WriteString(AccountId);
            writer.Write(Amount);
            writer.Write(Approved);
            writer.WriteString(CategoryId);
            writer.Write((byte)Cleared);
            writer.Write(Date);
            writer.Write(Deleted);
            writer.Write(FlagColor.HasValue);
            if (FlagColor.HasValue)
                writer.Write((byte)FlagColor.Value);
            writer.WriteString(ImportId);
            writer.WriteString(Memo);
            writer.WriteString(PayeeId);
            writer.WriteString(TransactionId);
            writer.WriteString(TransferAccountId);
            writer.WriteString(TransferTransactionId);

            writer.AddToInfo(info);
        }
        public TransactionSummary(SerializationInfo info, StreamingContext context)
        {
            var reader = SerializationReader.GetReader(info);

            AccountId = reader.ReadString();
            Amount = reader.ReadInt32();
            Approved = reader.ReadBoolean();
            CategoryId = reader.ReadString();
            Cleared = (TransactionStatus) reader.ReadByte();
            Date = reader.ReadDateTime();
            Deleted = reader.ReadBoolean();
            if (reader.ReadBoolean())
                FlagColor = (FlagColor)reader.ReadByte();
            ImportId = reader.ReadString();
            Memo = reader.ReadString();
            PayeeId = reader.ReadString();
            TransactionId = reader.ReadString();
            TransferAccountId = reader.ReadString();
            TransferTransactionId = reader.ReadString();
        }
        #endregion
    }
}