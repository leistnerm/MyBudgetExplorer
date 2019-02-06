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
    public class Payee : ISerializable
    {
        #region Properties
        public bool Deleted { get; set; }
        public string Name { get; set; }
        public string PayeeId { get; set; }
        public string TransferAccountId { get; set; }
        #endregion

        #region Constructors
        public Payee() { }
        #endregion

        #region Public Methods
        public static Payee Load(dynamic d)
        {
            try
            {
                var payee = new Payee
                {
                    Deleted = d.deleted,
                    Name = d.name,
                    PayeeId = d.id,
                    TransferAccountId = d.transfer_account_id,
                };
                return payee;
            }
            catch (Exception e)
            {
                if (!e.Data.Contains("json"))
                    e.Data.Add("json", d.ToString());
                throw e;
            }
        }
        #endregion

        #region ISerializable
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            var writer = SerializationWriter.GetWriter();

            writer.Write(Deleted);
            writer.WriteString(Name);
            writer.WriteString(PayeeId);
            writer.WriteString(TransferAccountId);

            writer.AddToInfo(info);
        }
        public Payee(SerializationInfo info, StreamingContext context)
        {
            var reader = SerializationReader.GetReader(info);

            Deleted = reader.ReadBoolean();
            Name = reader.ReadString();
            PayeeId = reader.ReadString();
            TransferAccountId = reader.ReadString();
        }
        #endregion
    }
}
