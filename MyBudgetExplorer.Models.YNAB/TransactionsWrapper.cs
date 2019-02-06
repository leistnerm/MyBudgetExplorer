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
    public class TransactionsWrapper
    {
        #region Properties
        public int ServerKnowledge { get; set; }
        public IList<TransactionDetail> Transactions { get; set; } = new List<TransactionDetail>();
        #endregion

        #region Public Methods
        public static TransactionsWrapper Load(dynamic d)
        {
            var result = new TransactionsWrapper
            {
                ServerKnowledge = d.server_knowledge
            };
            foreach (var t in d.transactions)
                result.Transactions.Add(TransactionDetail.Load(t));
            return result;
        }
        #endregion
    }
}
