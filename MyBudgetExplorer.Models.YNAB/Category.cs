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
    public class Category : ISerializable
    {
        #region Properties
        public long Activity { get; set; }
        public long Balance { get; set; }
        public long Budgeted { get; set; }
        public string CategoryGroupId { get; set; }
        public string CategoryId { get; set; }
        public bool Deleted { get; set; }
        public string GoalCreationMonth { get; set; }
        public int? GoalPercentageComplete { get; set; }
        public long GoalTarget { get; set; }
        public string GoalTargetMonth { get; set; }
        public GoalType? GoalType { get; set; }
        public bool Hidden { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public string OriginalCategoryGroupId { get; set; }
        #endregion

        #region Constructors
        public Category() { }
        #endregion

        #region Public Methods
        public static Category Load(dynamic dyn)
        {
            Func<dynamic, Category> func = (d) =>
            {
                var category = new Category
                {
                    Activity = d.activity,
                    Balance = d.balance,
                    Budgeted = d.budgeted,
                    CategoryGroupId = d.category_group_id,
                    CategoryId = d.id,
                    Deleted = d.deleted,
                    GoalCreationMonth = d.goal_creation_month,
                    GoalPercentageComplete = d.goal_percentage_complete,
                    GoalTarget = 0,
                    GoalTargetMonth = d.goal_target_month,
                    GoalType = d.goal_type,
                    Hidden = d.hidden,
                    Name = d.name,
                    Note = d.note,
                    OriginalCategoryGroupId = d.original_category_group_id,
                };

                // Correct invalid goal target.  Ran into this once.
                if (d.goal_target != null)
                    try
                    {
                        category.GoalTarget = d.goal_target;
                    }
                    catch (Exception ex)
                    {
                        string goalData = string.Empty;
                        try
                        {
                            goalData = d.goal_target.parent.ToString();
                        }
                        catch
                        {
                            goalData = d.goal_target.ToString();
                        }

                        throw new InvalidCastException($"Could not parse category goal_target.\n\nGoal data: ${goalData}", ex);
                    }

                return category;
            };

            return YnabApi.ProcessApiResult(dyn, func);
        }
        #endregion

        #region ISerializable
        public Category(SerializationInfo info, StreamingContext context)
        {
            var reader = SerializationReader.GetReader(info);

            Activity = reader.ReadInt64();
            Balance = reader.ReadInt64();
            Budgeted = reader.ReadInt64();
            CategoryGroupId = reader.ReadString();
            CategoryId = reader.ReadString();
            Deleted = reader.ReadBoolean();
            Hidden = reader.ReadBoolean();
            GoalCreationMonth = reader.ReadString();
            if (reader.ReadBoolean())
                GoalPercentageComplete = reader.ReadInt32();
            GoalTarget = reader.ReadInt64();
            GoalTargetMonth = reader.ReadString();
            if (reader.ReadBoolean())
                GoalType = (GoalType)reader.ReadByte();
            Name = reader.ReadString();
            Note = reader.ReadString();
            OriginalCategoryGroupId = reader.ReadString();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            var writer = SerializationWriter.GetWriter();

            writer.Write(Activity);
            writer.Write(Balance);
            writer.Write(Budgeted);
            writer.WriteString(CategoryGroupId);
            writer.WriteString(CategoryId);
            writer.Write(Deleted);
            writer.Write(Hidden);
            writer.WriteString(GoalCreationMonth);
            writer.Write(GoalPercentageComplete.HasValue);
            if (GoalPercentageComplete.HasValue)
                writer.Write(GoalPercentageComplete.Value);
            writer.Write(GoalTarget);
            writer.WriteString(GoalTargetMonth);
            writer.Write(GoalType.HasValue);
            if (GoalType.HasValue)
                writer.Write((byte)GoalType.Value);
            writer.WriteString(Name);
            writer.WriteString(Note);
            writer.WriteString(OriginalCategoryGroupId);

            writer.AddToInfo(info);
        }
        #endregion
    }
}
