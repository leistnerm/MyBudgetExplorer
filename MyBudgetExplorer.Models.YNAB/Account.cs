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
    public class Account : ISerializable
    {
        #region Properties
        public string AccountId { get; set; }
        public int Balance { get; set; }
        public int ClearedBalance { get; set; }
        public bool Closed { get; set; }
        public bool Deleted { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public bool OnBudget { get; set; }
        public string TransferPayeeId { get; set; }
        public AccountType Type { get; set; }
        public int UnclearedBalance { get; set; }
        #endregion

        #region Constructors
        public Account() { }
        #endregion

        #region Public Methods
        public static Account Load(dynamic d)
        {
            var result = new Account
            {
                AccountId = d.id,
                Balance = d.balance,
                ClearedBalance = d.cleared_balance,
                Closed = d.closed,
                Deleted = d.deleted,
                Name = d.name,
                Note = d.note,
                OnBudget = d.on_budget,
                TransferPayeeId = d.transfer_payee_id,
                Type = d.type,
                UnclearedBalance = d.uncleared_balance,
            };
            return result;
        }
        #endregion

        #region ISerializable
        public Account(SerializationInfo info, StreamingContext context)
        {
            var reader = SerializationReader.GetReader(info);

            AccountId = reader.ReadString();
            Balance = reader.ReadInt32();
            ClearedBalance = reader.ReadInt32();
            Closed = reader.ReadBoolean();
            Deleted = reader.ReadBoolean();
            Name = reader.ReadString();
            Note = reader.ReadString();
            OnBudget = reader.ReadBoolean();
            TransferPayeeId = reader.ReadString();
            Type = (AccountType)reader.ReadByte();
            UnclearedBalance = reader.ReadInt32();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            var writer = SerializationWriter.GetWriter();

            writer.WriteString(AccountId);
            writer.Write(Balance);
            writer.Write(ClearedBalance);
            writer.Write(Closed);
            writer.Write(Deleted);
            writer.WriteString(Name);
            writer.WriteString(Note);
            writer.Write(OnBudget);
            writer.WriteString(TransferPayeeId);
            writer.Write((byte)Type);
            writer.Write(UnclearedBalance);

            writer.AddToInfo(info);
        }
        #endregion
    }
}
