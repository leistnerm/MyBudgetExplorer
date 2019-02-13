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
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MyBudgetExplorer.Models.YNAB
{
    [Serializable]
    public class MonthDetail : ISerializable
    {
        #region Properties
        public long? Activity { get; set; }
        public long? AgeOfMoney { get; set; }
        public long? Budgeted { get; set; }
        public IList<Category> Categories { get; set; } = new List<Category>();
        public long? Income { get; set; }
        public DateTime Month { get; set; }
        public string Note { get; set; }
        public long? ToBeBudgeted { get; set; }
        #endregion

        #region Constructors
        public MonthDetail() { }
        #endregion

        #region Public Methods
        public static MonthDetail Load(dynamic dyn)
        {
            Func<dynamic, MonthDetail> func = (d) =>
            {
                var month = new MonthDetail
                {
                    Activity = d.activity,
                    AgeOfMoney = d.age_of_money,
                    Budgeted = d.budgeted,
                    Income = d.income,
                    Month = d.month,
                    Note = d.note,
                    ToBeBudgeted = d.to_be_budgeted,
                };

                foreach (var mc in d.categories)
                    month.Categories.Add(Category.Load(mc));

                return month;
            };

            return YnabApi.ProcessApiResult(dyn, func);
        }
        #endregion

        #region ISerializable
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            var writer = SerializationWriter.GetWriter();

            writer.Write(Activity.HasValue);
            if (Activity.HasValue)
                writer.Write(Activity.Value);
            writer.Write(AgeOfMoney.HasValue);
            if (AgeOfMoney.HasValue)
                writer.Write(AgeOfMoney.Value);
            writer.Write(Budgeted.HasValue);
            if (Budgeted.HasValue)
                writer.Write(Budgeted.Value);
            writer.Write(Categories);
            writer.Write(Income.HasValue);
            if (Income.HasValue)
                writer.Write(Income.Value);
            writer.Write(Month);
            writer.WriteString(Note);
            writer.Write(ToBeBudgeted.HasValue);
            if (ToBeBudgeted.HasValue)
                writer.Write(ToBeBudgeted.Value);

            writer.AddToInfo(info);
        }
        public MonthDetail(SerializationInfo info, StreamingContext context)
        {
            var reader = SerializationReader.GetReader(info);

            if (reader.ReadBoolean())
                Activity = reader.ReadInt64();
            if (reader.ReadBoolean())
                AgeOfMoney = reader.ReadInt64();
            if (reader.ReadBoolean())
                Budgeted = reader.ReadInt64();
            Categories = reader.ReadList<Category>();
            if (reader.ReadBoolean())
                Income = reader.ReadInt64();
            Month = reader.ReadDateTime();
            Note = reader.ReadString();
            if (reader.ReadBoolean())
                ToBeBudgeted = reader.ReadInt64();
        }
        #endregion
    }
}
