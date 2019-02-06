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
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MyBudgetExplorer.Models
{
    [Serializable]
    public class ProjectedSpendingScenario : ISerializable
    {
        #region Properties
        public string AccountId { get; set; }
        public long Amount { get; set; }
        public string CategoryId { get; set; }
        public IList<int> Days { get; set; } = new List<int>();
        public bool IsEnabled { get; set; }
        public bool IsExactAmount { get; set; }
        public string ScenarioId { get; set; }
        #endregion

        #region Constructors
        public ProjectedSpendingScenario() { }
        #endregion

        #region ISerializable
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            var writer = SerializationWriter.GetWriter();

            writer.WriteString(AccountId);
            writer.Write(Amount);
            writer.WriteString(CategoryId);
            writer.Write(Days);
            writer.Write(IsEnabled);
            writer.Write(IsExactAmount);
            writer.WriteString(ScenarioId);

            writer.AddToInfo(info);
        }
        public ProjectedSpendingScenario(SerializationInfo info, StreamingContext context)
        {
            var reader = SerializationReader.GetReader(info);

            AccountId = reader.ReadString();
            Amount = reader.ReadInt64();
            CategoryId = reader.ReadString();
            Days = reader.ReadList<int>();
            IsEnabled = reader.ReadBoolean();
            IsExactAmount = reader.ReadBoolean();
            ScenarioId = reader.ReadString();
        }
        #endregion
    }
}
