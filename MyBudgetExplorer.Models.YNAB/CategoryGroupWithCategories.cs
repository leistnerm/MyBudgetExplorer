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
using System;
using System.Collections.Generic;

namespace MyBudgetExplorer.Models.YNAB
{
    [Serializable]
    public class CategoryGroupWithCategories
    {
        #region Properties
        public IList<Category> Categories { get; set; } = new List<Category>();
        public string CategoryGroupId { get; set; }
        public bool Deleted { get; set; }
        public bool Hidden { get; set; }
        public string Name { get; set; }
        #endregion

        #region Public Methods
        public static CategoryGroupWithCategories Load(dynamic dyn)
        {
            Func<dynamic, CategoryGroupWithCategories> func = (d) =>
            {
                var result = new CategoryGroupWithCategories
                {
                    CategoryGroupId = d.id,
                    Name = d.name,
                    Hidden = d.hidden,
                    Deleted = d.deleted
                };

                foreach (var c in d.categories)
                    result.Categories.Add(Category.Load(c));

                return result;
            };

            return YnabApi.ProcessApiResult(dyn, func);
        }
        #endregion
    }
}
