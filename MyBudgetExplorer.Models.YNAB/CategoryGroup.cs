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
    public class CategoryGroup : ISerializable
    {
        #region Properties
        public string CategoryGroupId { get; set; }
        public bool Deleted { get; set; }
        public bool Hidden { get; set; }
        public string Name { get; set; }
        #endregion

        #region Constructors
        public CategoryGroup() { }
        #endregion

        #region Public Methods
        public static CategoryGroup Load(dynamic d)
        {
            return new CategoryGroup
            {
                CategoryGroupId = d.id,
                Deleted = d.deleted,
                Hidden = d.hidden,
                Name = d.name,
            };
        }
        #endregion

        #region ISerializable
        public CategoryGroup(SerializationInfo info, StreamingContext context)
        {
            var reader = SerializationReader.GetReader(info);

            CategoryGroupId = reader.ReadString();
            Deleted = reader.ReadBoolean();
            Hidden = reader.ReadBoolean();
            Name = reader.ReadString();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            var writer = SerializationWriter.GetWriter();

            writer.WriteString(CategoryGroupId);
            writer.Write(Deleted);
            writer.Write(Hidden);
            writer.WriteString(Name);

            writer.AddToInfo(info);
        }
        #endregion
    }
}
