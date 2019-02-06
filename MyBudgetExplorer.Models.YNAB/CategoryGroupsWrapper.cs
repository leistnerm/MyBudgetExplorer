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
    public class CategoryGroupsWrapper
    {
        #region Properties
        public IList<CategoryGroupWithCategories> CategoryGroups { get; set; } = new List<CategoryGroupWithCategories>();
        #endregion

        #region Public Methods
        public static CategoryGroupsWrapper Load(dynamic d)
        {
            try
            {
                var result = new CategoryGroupsWrapper();
                foreach (var c in d.category_groups)
                    result.CategoryGroups.Add(CategoryGroupWithCategories.Load(c));
                return result;
            }
            catch (Exception e)
            {
                if (!e.Data.Contains("json"))
                    e.Data.Add("json", d.ToString());
                throw e;
            }
        }
        #endregion
    }
}
