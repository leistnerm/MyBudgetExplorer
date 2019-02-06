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
    public class ScheduledSubTransactionScenario : ISerializable
    {
        #region Properties
        public long Amount { get; set; }
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
        public Frequency Frequency { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsExactAmount { get; set; }
        public string ScenarioId { get; set; }
        public string ScheduledSubTransactionId { get; set; }
        #endregion

        #region Constructors
        public ScheduledSubTransactionScenario() { }
        #endregion

        #region ISerializable
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            var writer = SerializationWriter.GetWriter();

            writer.Write(Amount);
            writer.Write(BeginDate);
            writer.Write(EndDate);
            writer.Write((byte)Frequency);
            writer.Write(IsEnabled);
            writer.Write(IsExactAmount);
            writer.WriteString(ScenarioId);
            writer.WriteString(ScheduledSubTransactionId);

            writer.AddToInfo(info);
        }
        public ScheduledSubTransactionScenario(SerializationInfo info, StreamingContext context)
        {
            var reader = SerializationReader.GetReader(info);

            Amount = reader.ReadInt64();
            BeginDate = reader.ReadDateTime();
            EndDate = reader.ReadDateTime();
            Frequency = (Frequency)reader.ReadByte();
            IsEnabled = reader.ReadBoolean();
            IsExactAmount = reader.ReadBoolean();
            ScenarioId = reader.ReadString();
            ScheduledSubTransactionId = reader.ReadString();
        }
        #endregion
    }
}
