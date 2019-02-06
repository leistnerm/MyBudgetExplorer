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
    public class BudgetDetail : ISerializable
    {
        #region Properties
        public IList<Account> Accounts { get; set; } = new List<Account>();
        public string BudgetId { get; set; }
        public IList<Category> Categories { get; set; } = new List<Category>();
        public IList<CategoryGroup> CategoryGroups { get; set; } = new List<CategoryGroup>();
        public CurrencyFormat CurrencyFormat { get; set; }
        public DateFormat DateFormat { get; set; }
        public string FirstMonth { get; set; }
        public DateTime LastModifiedOn { get; set; }
        public string LastMonth { get; set; }
        public IList<MonthDetail> Months { get; set; } = new List<MonthDetail>();
        public string Name { get; set; }
        public IList<PayeeLocation> PayeeLocations { get; set; } = new List<PayeeLocation>();
        public IList<Payee> Payees { get; set; } = new List<Payee>();
        public IList<ScheduledSubTransaction> ScheduledSubTransactions { get; set; } = new List<ScheduledSubTransaction>();
        public IList<ScheduledTransactionSummary> ScheduledTransactions { get; set; } = new List<ScheduledTransactionSummary>();
        public IList<SubTransaction> SubTransactions { get; set; } = new List<SubTransaction>();
        public IList<TransactionSummary> Transactions { get; set; } = new List<TransactionSummary>();
        #endregion

        #region Constructors
        public BudgetDetail() { }
        #endregion

        #region Public Methods
        public static BudgetDetail Load(dynamic d)
        {
            var result = new BudgetDetail
            {
                BudgetId = d.id,
                CurrencyFormat = CurrencyFormat.Load(d.currency_format),
                DateFormat = DateFormat.Load(d.date_format),
                FirstMonth = d.first_month,
                LastModifiedOn = d.last_modified_on,
                LastMonth = d.last_month,
                Name = d.name,
            };

            foreach (var a in d.accounts)
                result.Accounts.Add(Account.Load(a));

            foreach (var p in d.payees)
                result.Payees.Add(Payee.Load(p));

            foreach (var l in d.payee_locations)
                result.PayeeLocations.Add(PayeeLocation.Load(l));

            foreach (var g in d.category_groups)
                result.CategoryGroups.Add(CategoryGroup.Load(g));

            foreach (var c in d.categories)
                result.Categories.Add(Category.Load(c));

            foreach (var m in d.months)
                result.Months.Add(MonthDetail.Load(m));

            foreach (var t in d.transactions)
                result.Transactions.Add(TransactionSummary.Load(t));

            foreach (var s in d.subtransactions)
                result.SubTransactions.Add(SubTransaction.Load(s));

            foreach (var s in d.scheduled_transactions)
                result.ScheduledTransactions.Add(ScheduledTransactionSummary.Load(s));

            foreach (var s in d.scheduled_subtransactions)
                result.ScheduledSubTransactions.Add(ScheduledSubTransaction.Load(s));

            return result;
        }
        #endregion

        #region ISerializable
        public BudgetDetail(SerializationInfo info, StreamingContext context)
        {
            var reader = SerializationReader.GetReader(info);

            Accounts = reader.ReadList<Account>();
            BudgetId = reader.ReadString();
            Categories = reader.ReadList<Category>();
            CategoryGroups = reader.ReadList<CategoryGroup>();
            CurrencyFormat = (CurrencyFormat)reader.ReadObject();
            DateFormat = (DateFormat)reader.ReadObject();
            FirstMonth = reader.ReadString();
            LastModifiedOn = reader.ReadDateTime();
            LastMonth = reader.ReadString();
            Months = reader.ReadList<MonthDetail>();
            Name = reader.ReadString();
            PayeeLocations = reader.ReadList<PayeeLocation>();
            Payees = reader.ReadList<Payee>();
            ScheduledSubTransactions = reader.ReadList<ScheduledSubTransaction>();
            ScheduledTransactions = reader.ReadList<ScheduledTransactionSummary>();
            SubTransactions = reader.ReadList<SubTransaction>();
            Transactions = reader.ReadList<TransactionSummary>();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            var writer = SerializationWriter.GetWriter();

            writer.Write(Accounts);
            writer.WriteString(BudgetId);
            writer.Write(Categories);
            writer.Write(CategoryGroups);
            writer.WriteObject(CurrencyFormat);
            writer.WriteObject(DateFormat);
            writer.WriteString(FirstMonth);
            writer.Write(LastModifiedOn);
            writer.WriteString(LastMonth);
            writer.Write(Months);
            writer.WriteString(Name);
            writer.Write(PayeeLocations);
            writer.Write(Payees);
            writer.Write(ScheduledSubTransactions);
            writer.Write(ScheduledTransactions);
            writer.Write(SubTransactions);
            writer.Write(Transactions);

            writer.AddToInfo(info);
        }
        #endregion
    }
}