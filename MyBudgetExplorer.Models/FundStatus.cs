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
    public class FundStatus : ISerializable
    {
        #region Properties
        public long Amount { get; set; }
        public string CategoryName { get; set; }
        public DateTime Date { get; set; }
        public long Funded { get; set; }
        public string Id { get; set; }
        public string PayeeName { get; set; }
        #endregion

        #region Constructors
        public FundStatus() { }
        #endregion

        #region ISerializable
        public FundStatus(SerializationInfo info, StreamingContext context)
        {
            var reader = SerializationReader.GetReader(info);

            Amount = reader.ReadInt64();
            CategoryName = reader.ReadString();
            Date = reader.ReadDateTime();
            Funded = reader.ReadInt64();
            Id = reader.ReadString();
            PayeeName = reader.ReadString();
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            var writer = SerializationWriter.GetWriter();

            writer.Write(Amount);
            writer.WriteString(CategoryName);
            writer.Write(Date);
            writer.Write(Funded);
            writer.WriteString(Id);
            writer.WriteString(PayeeName);

            writer.AddToInfo(info);
        }
        #endregion
    }
}
