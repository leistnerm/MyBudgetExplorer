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
using Amazon.XRay.Recorder.Core;
using MyBudgetExplorer.Models.BinarySerialization;
using MyBudgetExplorer.Models.YNAB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace MyBudgetExplorer.Models
{
    [Serializable]
    public class Forecast : ISerializable
    {
        #region Fields
        private DateTime _CurrentDate;
        #endregion

        #region Properties
        public IList<Account> Accounts { get; set; } = new List<Account>();
        public string BudgetId { get; set; }
        public IList<Category> Categories { get; set; } = new List<Category>();
        public IList<CategoryGroup> CategoryGroups { get; set; } = new List<CategoryGroup>();
        public CurrencyFormat CurrencyFormat { get; set; }
        public DateTime CurrentDate
        {
            get { return _CurrentDate; }
            set
            {
                _CurrentDate = new DateTime(value.Year, value.Month, value.Day);
                CurrentMonthStart = new DateTime(value.Year, value.Month, 1);
                FutureMonthStart = CurrentMonthStart.AddMonths(1);
            }
        }
        public DateTime CurrentMonthStart { get; private set; }
        public DateFormat DateFormat { get; set; }
        public string FirstMonth { get; set; }
        private IList<ForecastItem> ForecastItems { get; set; } = new List<ForecastItem>();
        public DateTime ForecastUntil { get; set; }
        public DateTime FutureMonthStart { get; private set; }
        private IDictionary<string, List<FundItem>> IncomeFunding { get; } = new Dictionary<string, List<FundItem>>();
        public DateTime LastModifiedOn { get; set; }
        public string LastMonth { get; set; }
        public IDictionary<string, List<FundStatus>> MonthFundStatus { get; } = new Dictionary<string, List<FundStatus>>();
        public IList<MonthDetail> Months { get; set; } = new List<MonthDetail>();
        public string Name { get; set; }
        /// <summary>
        /// Holds the amount originally budgeted in each category on the initial budget.
        /// </summary>
        private IDictionary<DateTime, Dictionary<string, long>> OriginalBudgeted { get; set; } = new Dictionary<DateTime, Dictionary<string, long>>();
        public IList<PayeeLocation> PayeeLocations { get; set; } = new List<PayeeLocation>();
        public IList<Payee> Payees { get; set; } = new List<Payee>();
        public string ProgramCategoryGroupId { get { return "4faac58a-7a62-448c-b56c-6c722c6cb6b7"; } }
        public string ProjectedSpendingPayeeId { get { return "d32f86cf-f480-451f-80c8-8106dc4ecc46"; } }
        public string RemainingFundsCategoryId { get { return "03a612f6-5d66-4e77-807d-123cad5956e9"; } }
        public IList<ScheduledSubTransaction> ScheduledSubTransactions { get; set; } = new List<ScheduledSubTransaction>();
        public IList<ScheduledTransactionSummary> ScheduledTransactions { get; set; } = new List<ScheduledTransactionSummary>();
        public Settings Settings { get; set; }
        public IList<SubTransaction> SubTransactions { get; set; } = new List<SubTransaction>();
        public IList<TransactionSummary> Transactions { get; set; } = new List<TransactionSummary>();
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a forecasted budget for teh specified number of months.
        /// </summary>
        /// <param name="budget">The base budget to forecast.</param>
        /// <param name="forecastMonths">The number of months to forecast.</param>
        private Forecast(BudgetDetail budget, int forecastMonths)
        {
            AWSXRayRecorder.Instance.BeginSubsegment("MyBudgetExplorer.Models.Forecast.Forecast()");
            try
            {
                if (budget == null)
                    throw new ArgumentNullException("budget");

                // We need to forecast out at least 1 month, and it can not be negative.
                if (forecastMonths < 1)
                    forecastMonths = 1;
                // Limit to 10 years, as the longer we forecast the larger/slower it gets.
                if (forecastMonths > 120)
                    forecastMonths = 120;

                CurrentDate = DateTime.Now;
                ForecastUntil = CurrentMonthStart.AddMonths(forecastMonths);

                // TODO: Settings should be passed in similar to budget once we have an interface and can keep them around.
                Settings = new Settings();

                #region Set initial values to supplied budget
                Accounts = budget.Accounts;
                BudgetId = budget.BudgetId;
                Categories = budget.Categories;
                CategoryGroups = budget.CategoryGroups;
                CurrencyFormat = budget.CurrencyFormat;
                DateFormat = budget.DateFormat;
                FirstMonth = budget.FirstMonth;
                LastModifiedOn = budget.LastModifiedOn;
                LastMonth = budget.LastMonth;
                Months = budget.Months;
                Name = budget.Name;
                PayeeLocations = budget.PayeeLocations;
                Payees = budget.Payees;
                ScheduledSubTransactions = budget.ScheduledSubTransactions;
                ScheduledTransactions = budget.ScheduledTransactions;
                SubTransactions = budget.SubTransactions;
                Transactions = budget.Transactions;
                #endregion

                #region Record original amounts budgeted
                foreach (var month in Months)
                {
                    OriginalBudgeted.Add(month.Month, new Dictionary<string, long>());
                    foreach (var cat in month.Categories)
                        OriginalBudgeted[month.Month].Add(cat.CategoryId, cat.Budgeted);
                }
                #endregion

                #region Setup Program Categories
                // Create a category group to hold the new categories.
                CategoryGroups.Insert(0, new CategoryGroup
                {
                    CategoryGroupId = ProgramCategoryGroupId,
                    Name = "My Budget Explorer for YNAB"
                });

                // Create a category to hold remaining money left over after budgeting ahead.
                // Note that we are creating a new object for each insert, if we create one 
                // object and insert it multiple times it can have unintended consequences.
                Categories.Insert(0, new Category
                {
                    CategoryGroupId = ProgramCategoryGroupId,
                    CategoryId = RemainingFundsCategoryId,
                    Name = "Remaining Money",
                    Activity = 0,
                    Balance = 0,
                    Budgeted = 0,
                    Deleted = false,
                    GoalCreationMonth = null,
                    GoalPercentageComplete = 0,
                    GoalTarget = 0,
                    GoalTargetMonth = null,
                    GoalType = null,
                    Hidden = false,
                    Note = "This category collects money that was remaining after fully funding the future categories.  It does not exist in your budget, it's just added on here for easier tracking of extra money.",
                    OriginalCategoryGroupId = null
                });
                foreach (var month in Months)
                {
                    month.Categories.Insert(0, new Category
                    {
                        CategoryGroupId = ProgramCategoryGroupId,
                        CategoryId = RemainingFundsCategoryId,
                        Name = "Remaining Money",
                        Activity = 0,
                        Balance = 0,
                        Budgeted = 0,
                        Deleted = false,
                        GoalCreationMonth = null,
                        GoalPercentageComplete = 0,
                        GoalTarget = 0,
                        GoalTargetMonth = null,
                        GoalType = null,
                        Hidden = false,
                        Note = "This category collects money that was remaining after fully funding the future categories.  It does not exist in your budget, it's just added on here for easier tracking of extra money.",
                        OriginalCategoryGroupId = null
                    });
                }
                #endregion

                #region Setup Program Payees
                // Create and insert a payee for projected spending.
                var payee = new Payee
                {
                    Deleted = false,
                    Name = "Projected Spending",
                    PayeeId = ProjectedSpendingPayeeId,
                    TransferAccountId = null
                };
                Payees.Insert(0, payee);
                #endregion

                ExpandMonths();

                CreateForecastItems();

                ApplyScenarios();

                ExecuteForecast();

                // Clear the forecast items property, it should not be needed any longer.
                ForecastItems.Clear();
            }
            catch (Exception e)
            {
                AWSXRayRecorder.Instance.AddException(e);
                throw;
            }
            finally
            {
                AWSXRayRecorder.Instance.EndSubsegment();
            }
        }
        #endregion

        #region Public Methods
        public static Forecast Create(BudgetDetail budget, int forecastMonths)
        {
            return new Forecast(budget, forecastMonths);
        }
        public static Forecast Create(string token, string userId, int forecastMonths = 60)
        {
            var budget = Cache.GetBudget(token, userId);
            return Forecast.Create(budget, forecastMonths);
        }
        /// <summary>
        /// Get the list of items funded with the specified transaction.
        /// </summary>
        /// <param name="transactionId">The transaction id.</param>
        /// <returns>Items funded with this transaction.</returns>
        public List<FundItem> GetIncomeFunding(string transactionId)
        {
            if (!IncomeFunding.ContainsKey(transactionId))
                return new List<FundItem>();

            return IncomeFunding[transactionId];
        }
        /// <summary>
        /// Get the original amount budgeted for the specified month and category, before forecasting.
        /// </summary>
        /// <param name="month"></param>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public long GetOriginalBudgeted(DateTime month, string categoryId)
        {
            if (!OriginalBudgeted.ContainsKey(month))
                return 0;
            if (!OriginalBudgeted[month].ContainsKey(categoryId))
                return 0;
            return OriginalBudgeted[month][categoryId];
        }
        #endregion

        #region Private Methods
        private void AddIncomeFunding(string transactionId, FundItem funding)
        {
            if (!IncomeFunding.ContainsKey(transactionId))
                IncomeFunding.Add(transactionId, new List<FundItem>());

            IncomeFunding[transactionId].Add(funding);
        }
        private void AdjustCategoryBalances(DateTime currentMonth, string categoryId, long amount)
        {
            var masterCategory = Categories.SingleOrDefault(c => c.CategoryId == categoryId);
            if (masterCategory != null)
            {
                foreach (var month in Months)
                {
                    if (month.Month < currentMonth)
                        continue;
                    if (masterCategory.Name == "To be Budgeted")
                    {
                        // Adjust the income in the current month only.
                        if (currentMonth == month.Month)
                        {
                            month.Income += amount;
                        }
                        month.ToBeBudgeted += amount;
                        // Recalculate TBB
                        // month.ToBeBudgeted = CalculateApiTBB(month.Month);

                    }
                    else
                    {
                        var monthCategory = month.Categories.SingleOrDefault(c => c.CategoryId == categoryId);
                        if (monthCategory != null)
                        {
                            monthCategory.Balance += amount;
                            // Adjust the activity in the current month only.
                            if (currentMonth == month.Month)
                            {
                                if (amount < 0)
                                {
                                    month.Activity += amount;
                                    monthCategory.Activity += amount;
                                }
                                else
                                {
                                    month.Income += amount;
                                    month.Budgeted += amount;
                                    monthCategory.Budgeted += amount;
                                }
                            }
                        }
                    }
                }
            }
        }
        private DateTime AdvanceDate(DateTime current, Frequency frequency)
        {
            switch (frequency)
            {
                case Frequency.EveryOtherYear:
                    current = current.AddYears(2);
                    break;
                case Frequency.Yearly:
                    current = current.AddYears(1);
                    break;
                case Frequency.TwiceAYear:
                    current = current.AddMonths(6);
                    break;
                case Frequency.Every4Months:
                    current = current.AddMonths(4);
                    break;
                case Frequency.Every3Months:
                    current = current.AddMonths(3);
                    break;
                case Frequency.Monthly:
                    current = current.AddMonths(1);
                    break;
                case Frequency.EveryOtherMonth:
                    current = current.AddMonths(2);
                    break;
                case Frequency.Every4Weeks:
                    current = current.AddDays(28);
                    break;
                case Frequency.TwiceAMonth:
                    // Calculation seems to go:
                    // If current day is <= 15, add 15 days.
                    // If that date is in the next month, go to the last day of the current month.
                    //
                    // If current day is > 15, move to the next month and subtract 15 days.
                    if (current.Day <= 15)
                    {
                        var month = current.Month;
                        current = current.AddDays(15);
                        while (current.Month != month)
                            current = current.AddDays(-1);
                    }
                    else
                    {
                        current = current.AddMonths(1).AddDays(-15);
                    }
                    break;
                case Frequency.EveryOtherWeek:
                    current = current.AddDays(14);
                    break;
                case Frequency.Weekly:
                    current = current.AddDays(7);
                    break;
                case Frequency.Daily:
                    current = current.AddDays(1);
                    break;
                case Frequency.Never:
                    current = DateTime.MaxValue;
                    break;
                default:
                    throw new ApplicationException(frequency.ToString());
            }

            return current;
        }
        /// <summary>
        /// Adjust the forecast items based on setup scenarios.
        /// </summary>
        private void ApplyScenarios()
        {
            AWSXRayRecorder.Instance.BeginSubsegment("MyBudgetExplorer.Models.Forecast.ApplyScenarios()");
            try
            {
                // If nothing has been forecasted, don't try to apply scenarios.
                if (ForecastItems.Count == 0)
                    return;

                // Get the maximum date of the forecasted items.  (Adding 1 day for a less than comparison)
                var maxDate = ForecastItems.Max(f => f.Date).AddDays(1);
                // Apply each scheduled transaction scenario, ordered by the begining date.
                foreach (var scenario in Settings.ScheduledTransactionScenarios.OrderBy(s => s.BeginDate))
                {
                    // TODO: Figure out what to do with scenarios that have already passed their begining date.
                    // If the scenario is not enabled, don't apply it.
                    if (!scenario.IsEnabled)
                        continue;
                    // Get the begining date for the scenario.
                    var sDate = scenario.BeginDate;
                    // While the scenario date is less than the max date, apply it to all applicable forecasted items.
                    while (sDate < maxDate)
                    {
                        foreach (var item in ForecastItems)
                        {
                            // If the scenario does not match the forecast item scheduled transaction id, move to the next forecast item.
                            if (scenario.ScheduledTransactionId != item.ScheduledTransactionId)
                                continue;
                            // If the forecast item date is less than the scenario date, move to the next forecast item.
                            if (item.Date < sDate)
                                continue;
                            // If the scenario is a percentage, adjust the forecast item amount.
                            if (!scenario.IsExactAmount)
                            {
                                item.Amount = Convert.ToInt64(decimal.Multiply(item.Amount, decimal.Divide(100M + decimal.Divide(scenario.Amount, 1000), 100)));
                            }
                            // Otherwise adjust by the exact amount specified.
                            else
                            {
                                item.Amount += scenario.Amount;
                            }
                        }
                        // Advance the scenario date based on the specified frequency.
                        sDate = AdvanceDate(sDate, scenario.Frequency);
                    }
                }
                // Apply each scheduled sub transaction scenario, ordered by the begining date.
                foreach (var scenario in Settings.ScheduledSubTransactionScenarios.OrderBy(s => s.BeginDate))
                {
                    // TODO: Figure out what to do with scenarios that have already passed their begining date.
                    // If the scenario is not enabled, don't apply it.
                    if (!scenario.IsEnabled)
                        continue;
                    // Get the begining date for the scenario.
                    var sDate = scenario.BeginDate;
                    // While the scenario date is less than the max date, apply it to all applicable forecasted items.
                    while (sDate < maxDate)
                    {
                        foreach (var item in ForecastItems)
                        {
                            // If the scenario does not match the forecast item scheduled subtransaction id, move to the next forecast item.
                            if (scenario.ScheduledSubTransactionId != item.ScheduledSubTransactionId)
                                continue;
                            // If the forecast item date is less than the scenario date, move to the next forecast item.
                            if (item.Date < sDate)
                                continue;
                            // If the scenario is a percentage, adjust the forecast item amount.  Also adjust the parent forecast item amount.
                            if (!scenario.IsExactAmount)
                            {
                                var previous = item.Amount;
                                item.Amount = Convert.ToInt64(decimal.Multiply(item.Amount, decimal.Divide(100M + decimal.Divide(scenario.Amount, 1000), 100)));
                                var diff = item.Amount - previous;
                                var parent = ForecastItems.Single(f => f.TransactionId == item.TransactionId && f.SubTransactionId == null);
                                parent.Amount += diff;
                            }
                            // Otherwise adjust by the exact amount specified.  Also adjust the parent forecast item amount.
                            else
                            {
                                item.Amount += scenario.Amount;
                                var parent = ForecastItems.Single(f => f.TransactionId == item.TransactionId && f.SubTransactionId == null);
                                parent.Amount += scenario.Amount;
                            }
                        }
                        // Advance the scenario date based on the specified frequency.
                        sDate = AdvanceDate(sDate, scenario.Frequency);
                    }
                }
                // Projected Spending scenarios are handled in the CreateForecastItems and ExecuteForecast methods.
            }
            catch (Exception e)
            {
                AWSXRayRecorder.Instance.AddException(e);
                throw;
            }
            finally
            {
                AWSXRayRecorder.Instance.EndSubsegment();
            }
        }
        private long CalculateOverspent(DateTime month)
        {
            var currentMonth = Months.Single(m => m.Month == month);
            var previousMonth = Months.SingleOrDefault(m => m.Month == month.AddMonths(-1));
            var perviousTBB = previousMonth == null ? 0 : previousMonth.ToBeBudgeted;
            var overspent = currentMonth.Income - currentMonth.Budgeted - currentMonth.ToBeBudgeted + perviousTBB;
            return overspent.Value;
        }
        /// <summary>
        /// Create the forecasted items to be added as transactions and budgeted amounts.
        /// </summary>
        private void CreateForecastItems()
        {
            AWSXRayRecorder.Instance.BeginSubsegment("MyBudgetExplorer.Models.Forecast.CreateForecastItems()");
            try
            {
                // Forecast income until the end of the forecast period.
                var incomeEnd = ForecastUntil;
                // Forecast expenses one month beyond the forecast period.
                var expenseEnd = incomeEnd.AddMonths(1);

                var results = new List<ForecastItem>();

                // Get a list of all scheduled transactions until the end date.
                var scheduledTransactions = new List<ScheduledTransactionSummary>();
                foreach (var st in ScheduledTransactions)
                {
                    // Do not process deleted scheduled transactions
                    if (st.Deleted)
                        continue;
                    // Do not process income beyond the forecast period.
                    if (st.Amount >= 0 && st.DateNext >= incomeEnd)
                        continue;
                    // Do not process expenses beyond the forecast period + one month.
                    if (st.Amount < 0 && st.DateNext >= expenseEnd)
                        continue;

                    // Add the scheduled transaction to the list.
                    scheduledTransactions.Add(new ScheduledTransactionSummary
                    {
                        AccountId = st.AccountId,
                        Amount = st.Amount,
                        CategoryId = st.CategoryId,
                        DateFirst = st.DateFirst,
                        DateNext = st.DateNext,
                        Deleted = st.Deleted,
                        FlagColor = st.FlagColor,
                        Frequency = st.Frequency,
                        Memo = st.Memo,
                        PayeeId = st.PayeeId,
                        ScheduledTransactionId = st.ScheduledTransactionId,
                        TransferAccountId = st.TransferAccountId
                    });
                }

                // Sort the scheduled transactions by date and then amount.
                scheduledTransactions = scheduledTransactions.OrderBy(se => se.DateNext).ThenBy(se => se.Amount).ToList();

                // Expand out the scheduled transactions to all their dates in the forecast period.
                // Each time a a new forecast item is created, advance the date based on the frequency.
                // Repeat until we have processed all the scheduled transactions.
                while (scheduledTransactions.Count > 0)
                {
                    // Get the first scheduled transaction
                    var st = scheduledTransactions.First();
                    // If the scheduled transaction is less than the current date, advance it based on the specified frequency.
                    if (st.DateNext < CurrentDate)
                    {
                        // Advance the date.
                        st.DateNext = AdvanceDate(st.DateNext, st.Frequency);
                        // If the scheduled transaction is now outside the forecast period, remove it, and continue to the
                        // next scheduled transaction.
                        if ((st.Amount >= 0 && st.DateNext >= incomeEnd) || (st.Amount < 0 && st.DateNext >= expenseEnd))
                        {
                            scheduledTransactions = scheduledTransactions
                                .Where(e => (e.Amount >= 0 && e.DateNext < incomeEnd) || (e.Amount < 0 && e.DateNext < expenseEnd))
                                .ToList();
                            continue;
                        }
                    }

                    // Get the month of the scheduled transaction.
                    var stMonth = Months.SingleOrDefault(m => m.Month == new DateTime(st.DateNext.Year, st.DateNext.Month, 1));
                    // If the month was not found, something went wrong.  Record the details and throw an exception.
                    if (stMonth == null)
                    {
                        var ex = new ApplicationException("Could not find scheduled transaction month.");
                        ex.Data.Add("Id", st.ScheduledTransactionId);
                        ex.Data.Add("Next Date", st.DateNext.ToShortDateString());
                        ex.Data.Add("Frequency", st.Frequency.ToString());
                        // Add the account information
                        var stAccount = Accounts.SingleOrDefault(a => a.AccountId == st.AccountId);
                        if (stAccount != null)
                        {
                            ex.Data.Add("Account Id", stAccount.AccountId);
                            ex.Data.Add("Account Name", stAccount.Name);
                        }
                        else
                            ex.Data.Add("Account", $"{st.AccountId} was not found.");
                        // Add the category information.
                        var stCategory = Categories.SingleOrDefault(c => c.CategoryId == st.CategoryId);
                        if (stCategory != null)
                        {
                            ex.Data.Add("Category Id", stCategory.CategoryId);
                            ex.Data.Add("Category Name", stCategory.Name);
                            // Add the category group information.
                            var stCategoryGroup = CategoryGroups.SingleOrDefault(g => g.CategoryGroupId == stCategory.CategoryGroupId);
                            if (stCategoryGroup != null)
                                ex.Data.Add("Category Group Name", stCategoryGroup.Name);
                            else
                                ex.Data.Add("Category Group", $"{stCategory.CategoryGroupId} was not found.");
                        }
                        else
                            ex.Data.Add("Category", $"{st.CategoryId} was not found.");
                        // Add the payee information.
                        var stPayee = Payees.SingleOrDefault(p => p.PayeeId == st.PayeeId);
                        if (stPayee != null)
                        {
                            ex.Data.Add("Payee Id", stPayee.PayeeId);
                            ex.Data.Add("Payee Name", stPayee.Name);
                        }
                        else
                            ex.Data.Add("Payee", $"{st.PayeeId} was not found.");

                        throw ex;
                    }

                    // Create the forecast item based on the scheduled transaction.
                    var forecastItem = new ForecastItem
                    {
                        ForecastItemType = ForecastItemType.ScheduledTransaction,
                        AccountId = st.AccountId,
                        FlagColor = st.FlagColor,
                        Memo = st.Memo,
                        TransferAccountId = st.TransferAccountId,
                        IsSplit = false,
                        PayeeId = st.PayeeId,
                        CategoryId = st.CategoryId,
                        CategoryName = "Split",
                        Date = st.DateNext,
                        ScheduledTransactionId = st.ScheduledTransactionId,
                        TransactionId = $"{st.ScheduledTransactionId}_{st.DateNext.ToString("yyyy-MM-dd")}",
                        Amount = st.Amount,
                        Funded = 0
                    };

                    // Set the payee name.
                    var payee = Payees.SingleOrDefault(p => p.PayeeId == st.PayeeId);
                    if (payee != null)
                        forecastItem.PayeeName = payee.Name;
                    else
                        forecastItem.PayeeName = "[Unknown Payee]";

                    // Get the category for the scheduled transaction.  Split transactions will return null.
                    var seCategory = stMonth.Categories.SingleOrDefault(c => c.CategoryId == st.CategoryId);

                    // If this is not a split transaction, add it.
                    if (seCategory != null)
                    {
                        forecastItem.CategoryName = seCategory.Name;
                        results.Add(forecastItem);
                    }
                    // For split transactions, add each sub transaction.
                    else
                    {
                        // Set a value indicating this is a split transaction.
                        forecastItem.IsSplit = true;
                        results.Add(forecastItem);

                        // Add each scheduled sub transaction.
                        foreach (var sse in ScheduledSubTransactions)
                        {
                            if (sse.ScheduledTransactionId == st.ScheduledTransactionId)
                            {
                                var sseCategory = stMonth.Categories.SingleOrDefault(c => c.CategoryId == sse.CategoryId);
                                results.Add(new ForecastItem
                                {
                                    ForecastItemType = ForecastItemType.ScheduledSubTransaction,
                                    AccountId = st.AccountId,
                                    FlagColor = st.FlagColor,
                                    Memo = st.Memo,
                                    TransferAccountId = st.TransferAccountId,
                                    IsSplit = false,
                                    PayeeId = st.PayeeId,
                                    PayeeName = forecastItem.PayeeName,
                                    CategoryId = sse.CategoryId,
                                    CategoryName = sseCategory.Name,
                                    Date = st.DateNext,
                                    ScheduledTransactionId = st.ScheduledTransactionId,
                                    TransactionId = forecastItem.TransactionId,
                                    ScheduledSubTransactionId = sse.ScheduledSubTransactionId,
                                    SubTransactionId = $"{sse.ScheduledSubTransactionId}_{st.DateNext.ToString("yyyy-MM-dd")}",
                                    Amount = sse.Amount,
                                    Funded = 0
                                });
                            }
                        }
                    }

                    // Advance the scheduled transaction date based on the specified frequency.
                    st.DateNext = AdvanceDate(st.DateNext, st.Frequency);
                    // If the scheduled transaction is now outside the forecast period, remove it, and continue to the
                    // next scheduled transaction.
                    scheduledTransactions = scheduledTransactions
                        .Where(e => (e.Amount >= 0 && e.DateNext < incomeEnd) || (e.Amount < 0 && e.DateNext < expenseEnd))
                        .ToList();
                }

                // Add forecast items for categories with goals.
                foreach (var month in Months.Where(m => m.Month >= CurrentMonthStart && m.Month < expenseEnd).OrderBy(m => m.Month))
                {
                    foreach (var cat in month.Categories)
                    {
                        // TODO: Make the dates configurable for each category.
                        var dates = new[] { 1, 15 };
                        // If this does not have a goal, or the creation month is greater than the current month, move on to the next category.
                        if (!cat.GoalType.HasValue || string.IsNullOrWhiteSpace(cat.GoalCreationMonth) || DateTime.Parse(cat.GoalCreationMonth) > month.Month)
                            continue;

                        // Create the forecast items for the goal.
                        switch (cat.GoalType)
                        {
                            // Monthly Funding goal.
                            case GoalType.MF:
                                // Get the target for the goal.
                                var remaining = cat.GoalTarget;
                                if (remaining > 0)
                                {
                                    // Turn it negative.
                                    remaining *= -1;
                                    // Calculate the amount to add for each day.
                                    var perTime = Convert.ToInt64(Math.Floor(Decimal.Divide(remaining, dates.Length)));
                                    // Add a forecast item for each day.
                                    foreach (var day in dates)
                                    {
                                        // Create the forecast item.
                                        var result = new ForecastItem
                                        {
                                            ForecastItemType = ForecastItemType.GoalFunding,
                                            IsSplit = false,
                                            PayeeName = "Monthly Funding Goal",
                                            CategoryId = cat.CategoryId,
                                            CategoryName = cat.Name,
                                            Date = new DateTime(month.Month.Year, month.Month.Month, day),
                                            Amount = 0,
                                            Funded = 0
                                        };
                                        // Set the amount of the forecast item.
                                        // If the amount per time is less than the amount remaining,
                                        // set it to the amount remaining.
                                        if (perTime < remaining)
                                        {
                                            result.Amount = remaining;
                                            remaining = 0;
                                        }
                                        // Otherwise, set it to the amount per day and adjust the amount remaining.
                                        else
                                        {
                                            result.Amount = perTime;
                                            remaining -= perTime;
                                        }
                                        // TODO: Verify this works for something like $100 target, split over 3 days.  Should add 33.33, 33.33, 33.34.

                                        // Add the forecast item.
                                        results.Add(result);
                                    }
                                }

                                // Get any projected spending setup for this category.
                                var projected = Settings.ProjectedSpendingScenarios.SingleOrDefault(p => p.CategoryId == cat.CategoryId);
                                // If this category has projected spending, add the appropriate forecast items.
                                if (projected != null)
                                {
                                    // Get the target for the goal.
                                    remaining = cat.GoalTarget;
                                    if (remaining > 0)
                                    {
                                        // Turn it negative.
                                        remaining *= -1;
                                        // Calculate the amount to add for each day.
                                        var perTime = Convert.ToInt64(Math.Floor(Decimal.Divide(remaining, dates.Length)));
                                        // Add a forecast item for each day.
                                        foreach (var day in projected.Days)
                                        {
                                        // Create the forecast item.
                                            var result = new ForecastItem
                                            {
                                                ForecastItemType = ForecastItemType.ProjectedSpending,
                                                IsSplit = false,
                                                PayeeName = "Projected Spending",
                                                AccountId = projected.AccountId,
                                                CategoryId = cat.CategoryId,
                                                CategoryName = cat.Name,
                                                Date = new DateTime(month.Month.Year, month.Month.Month, day),
                                                Amount = 0,
                                                Funded = 0,
                                                PayeeId = ProjectedSpendingPayeeId
                                            };
                                            // Set the amount of the forecast item.
                                            // If the amount per time is less than the amount remaining,
                                            // set it to the amount remaining.
                                            if (perTime < remaining)
                                            {
                                                result.Amount = remaining;
                                                remaining = 0;
                                            }
                                            // Otherwise, set it to the amount per day and adjust the amount remaining.
                                            else
                                            {
                                                result.Amount = perTime;
                                                remaining -= perTime;
                                            }
                                            // TODO: Verify this works for something like $100 target, split over 3 days.  Should add 33.33, 33.33, 33.34.

                                        // Add the forecast item.
                                            results.Add(result);
                                        }
                                    }
                                }
                                break;
                            // Target Budget goal.
                            case GoalType.TB:
                                // TODO: Implment support
                                break;
                            // Target Budget by Date goal.
                            case GoalType.TBD:
                                // TODO: Implment support
                                break;
                            // If we find a different goal type, throw an exception.
                            default:
                                throw new NotImplementedException($"Goal Type: {cat.GoalType.ToString()} is not supported.");
                        }
                    }
                }

                // Add the newly created forecast items, in a specific order.
                // Get the list of each day with at least 1 item.
                var days = results.Select(r => r.Date).Distinct().OrderBy(r => r.Date).ToList();
                // Add the items for that day.
                foreach (var day in days)
                {
                    // Get the number of items alreadey added.
                    var initialCount = ForecastItems.Count;

                    // Get a list of all the items for this day.
                    var all = results.Where(r => r.Date == day).ToList();
                    // Add Income Scheduled Transactions, ordered largest amount (biggest income) to smallest.
                    var incomes = all.Where(r => r.Amount >= 0 && r.ScheduledSubTransactionId == null).OrderByDescending(r => r.Amount);
                    foreach (var income in incomes)
                    {
                        ForecastItems.Add(income);
                        // Add sub transactions, ordered largest amount (biggest income) to smallest.
                        foreach (var sub in all.Where(r => r.ScheduledTransactionId == income.ScheduledTransactionId && r.ScheduledSubTransactionId != null).OrderByDescending(r => r.Amount))
                            ForecastItems.Add(sub);
                    }

                    // Add Expense Scheduled Transactions, ordered smallest amount (biggest expense) to largest.
                    var expenses = all.Where(r => r.Amount < 0 && r.ScheduledSubTransactionId == null).OrderBy(r => r.Amount);
                    foreach (var expense in expenses)
                    {
                        ForecastItems.Add(expense);
                        // // Add sub transactions, ordered smallest amount (biggest expense) to largest.
                        foreach (var item in all.Where(r => r.ScheduledTransactionId == expense.ScheduledTransactionId && r.ScheduledSubTransactionId != null).OrderBy(r => r.Amount))
                            ForecastItems.Add(item);
                    }

                    // Throw an error if not all forecast items were added.
                    if (ForecastItems.Count != initialCount + all.Count)
                        throw new ApplicationException($"Failed creating forecast items on {day.ToShortDateString()}.  Total items created {ForecastItems.Count.ToString("N0")}, expected {(initialCount + all.Count).ToString("N0")}.");
                }
            }
            catch (Exception e)
            {
                AWSXRayRecorder.Instance.AddException(e);
                throw;
            }
            finally
            {
                AWSXRayRecorder.Instance.EndSubsegment();
            }
        }
        /// <summary>
        /// Create scheduled transactions based on the forecasted items.
        /// </summary>
        private void ExecuteForecast()
        {
            AWSXRayRecorder.Instance.BeginSubsegment("MyBudgetExplorer.Models.Forecast.Forecast()");
            try
            {
                #region Set initial funding
                // Get current category available for each month.
                var balances = new List<Tuple<DateTime, string, long>>();
                foreach (var month in Months.Where(m => m.Month >= CurrentMonthStart))
                    balances.AddRange(month.Categories.Select(c => new Tuple<DateTime, string, long>(month.Month, c.CategoryId, c.Balance)));
                // Get current category budgeted for each month.
                var budgeted = new List<Tuple<DateTime, string, long>>();
                foreach (var month in Months.Where(m => m.Month >= CurrentMonthStart))
                    budgeted.AddRange(month.Categories.Select(c => new Tuple<DateTime, string, long>(month.Month, c.CategoryId, c.Budgeted)));
                // Set funding for forecasted items based on current budgeted amounts.
                foreach (var item in ForecastItems)
                {
                    // If the forecast item has already been funded, move to the next.
                    if (item.Remaining >= 0)
                        continue;

                    // If this is a monthly goal funding item.
                    if (item.ForecastItemType == ForecastItemType.GoalFunding)
                    {
                        // Get the date of the forecast item month.
                        var monthDate = new DateTime(item.Date.Year, item.Date.Month, 1);
                        // Get the budgeted amount for the month.
                        var budget = budgeted.SingleOrDefault(b => b.Item1 == monthDate && b.Item2 == item.CategoryId);
                        // If we can't find one, continue to the next forecast item.
                        if (budget == null)
                            continue;
                        // Set the budget adjustment to zero.
                        var adjustment = 0L;
                        // If the amount budgeted is more than the amount of funding needed
                        // mark the item as fully funded and add the amount to the budget adjustment
                        if (budget.Item3 + item.Remaining >= 0)
                        {
                            adjustment += item.Remaining;
                            item.Funded -= item.Remaining;
                        }
                        // Otherwise increase the funding by the available amount in the budget
                        // and add the amount to the budget adjustment.
                        else
                        {
                            item.Funded += budget.Item3;
                            adjustment -= budget.Item3;
                        }
                        // Adjust the budgeted amount for the item month (and only that month)
                        for (int i = 0; i < budgeted.Count; i++)
                        {
                            var tuple = budgeted[i];
                            if (tuple.Item1 == monthDate && tuple.Item2 == item.CategoryId)
                                budgeted[i] = new Tuple<DateTime, string, long>(tuple.Item1, tuple.Item2, tuple.Item3 + adjustment);
                        }
                    }
                    // If this is not projected spending, fund based on the available balance.
                    else if (item.ForecastItemType != ForecastItemType.ProjectedSpending)
                    {
                        // Get the date of the forecast item month.
                        var monthDate = new DateTime(item.Date.Year, item.Date.Month, 1);
                        // Get the available balance for the month.
                        var balance = balances.SingleOrDefault(b => b.Item1 == monthDate && b.Item2 == item.CategoryId);
                        // If we can't find one, continue to the next forecast item.
                        if (balance == null)
                            continue;
                        // Set the balance adjustment to zero.
                        var adjustment = 0L;
                        // If the available balance is more than the amount of funding needed
                        // mark the item as fully funded and add the amount to the balance adjustment.
                        if (balance.Item3 + item.Remaining >= 0)
                        {
                            adjustment += item.Remaining;
                            item.Funded -= item.Remaining;
                        }
                        // Otherwise increast the funding by the available amount and add the amount to
                        // the available balance adjustment.
                        else
                        {
                            item.Funded += balance.Item3;
                            adjustment -= balance.Item3;
                        }
                        // Adjust the available amount for the item month and all subsequent months. 
                        for (int i = 0; i < balances.Count; i++)
                        {
                            var tuple = balances[i];
                            if (tuple.Item1 >= monthDate && tuple.Item2 == item.CategoryId)
                                balances[i] = new Tuple<DateTime, string, long>(tuple.Item1, tuple.Item2, tuple.Item3 + adjustment);
                        }
                    }
                }
                #endregion

                // Record the current funding status after funding from the initial budget.
                MonthFundStatus.Add(CurrentMonthStart.ToShortDateString(), GetFundStatus(CurrentMonthStart));

                // Add each forecast item as a transaction.
                foreach (var item in ForecastItems)
                {
                    // Do not add goal funding forecast items as transactions.
                    if (item.ForecastItemType == ForecastItemType.GoalFunding)
                        continue;
                    // Do not add scheduled sub transactions as transactions, they will be added with the parent transaction.
                    if (item.ForecastItemType == ForecastItemType.ScheduledSubTransaction)
                        continue;

                    // Get the month date for the forecast item.
                    var itemMonthDate = new DateTime(item.Date.Year, item.Date.Month, 1);

                    // Create the transaction for this forecast item.
                    var t = new TransactionSummary
                    {
                        AccountId = item.AccountId,
                        Amount = item.Amount,
                        Approved = true,
                        CategoryId = item.CategoryId,
                        Cleared = TransactionStatus.Uncleared,
                        Date = item.Date,
                        Deleted = false,
                        FlagColor = item.FlagColor,
                        ImportId = "scheduled",
                        Memo = item.Memo,
                        PayeeId = item.PayeeId,
                        TransactionId = item.TransactionId,
                        TransferAccountId = item.TransferAccountId,
                        // TODO: Determine what to do to support transfers.
                        TransferTransactionId = string.Empty
                    };

                    // Modify projected balance transactions based on the amount of spending left to reach the projection.
                    // IE: If you project 500 in grocery spending this month, and have already spent 300 we should only
                    //     project 200 in additional spending.
                    if (item.ForecastItemType == ForecastItemType.ProjectedSpending)
                    {
                        // Set the import id to "projected" so we can identify these transactions later if needed.
                        t.ImportId = "projected";
                        // Get the available amount this month
                        var availableAmount = Months.Single(m => m.Month == itemMonthDate).Categories.Single(c => c.CategoryId == item.CategoryId).Balance;
                        // Get the number of remaining projected spending items this month for the category.
                        var numberRemaining = ForecastItems.Where(f => f.ForecastItemType == ForecastItemType.ProjectedSpending && f.CategoryId == item.CategoryId && f.Date >= item.Date && f.Date < itemMonthDate.AddMonths(1)).Count();
                        // Adjust the amount of this item.
                        t.Amount = Convert.ToInt64(decimal.Divide(availableAmount, numberRemaining)) * -1;
                    }
                    // Add the transaction
                    Transactions.Insert(0, t);

                    // Adjust the category balances
                    AdjustCategoryBalances(itemMonthDate, t.CategoryId, t.Amount);

                    // See if there are any scheduled sub transactions for this transaction.
                    var subItems = ForecastItems.Where(f => f.TransactionId == item.TransactionId && f.SubTransactionId != null).ToList();
                    // Add each one
                    foreach (var subItem in subItems)
                    {
                        // Create the sub transaction.
                        var s = new SubTransaction
                        {
                            Amount = subItem.Amount,
                            CategoryId = subItem.CategoryId,
                            Deleted = false,
                            Memo = subItem.Memo,
                            PayeeId = subItem.PayeeId,
                            SubTransactionId = subItem.SubTransactionId,
                            TransactionId = subItem.TransactionId,
                            TransferAccountId = subItem.TransferAccountId
                        };
                        // Add the sub transaction
                        SubTransactions.Insert(0, s);
                        // If this is an income transaction, and a income sub transaction
                        // that is not TBB, add an income funding record to the transaction
                        // as a manual funding item.
                        //
                        // IE if you have a sub transaction in a scheduled income to fund a 
                        //    category, recored it as manual funding vs the normal suggested
                        //    funding to a category.
                        if (t.Amount > 0 && subItem.Amount > 0)
                        {
                            var cat = Categories.Single(c => c.CategoryId == subItem.CategoryId);
                            if (cat.Name != "To be Budgeted")
                                AddIncomeFunding(t.TransactionId,
                                    new FundItem
                                    {
                                        CategoryId = cat.CategoryId,
                                        CategoryName = $"{cat.Name}",
                                        Date = t.Date,
                                        Payee = "Manual Funding",
                                        Amount = subItem.Amount
                                    });
                        }

                        // Adjust the category balances
                        AdjustCategoryBalances(itemMonthDate, s.CategoryId, s.Amount);
                    }

                    // If this was income, budget it.
                    if (t.Amount > 0)
                    {
                        // Get the month of the transaction
                        var month = Months.Single(m => m.Month == itemMonthDate);
                        // Get the next month following the transaction (for our goal of budgeting ahead 1 month)
                        var nextMonth = Months.Single(m => m.Month == itemMonthDate.AddMonths(1));
                        // Get the TBB
                        var tbb = month.ToBeBudgeted - Months.Where(m => m.Month > itemMonthDate).Sum(m => m.Budgeted);
                        // If there is money TBB, budget with it.
                        if (tbb > 0)
                        {
                            // Get a list of upcoming forecast items
                            var upcoming = ForecastItems
                                // That are prior to the end of next month
                                .Where(f => f.Date < nextMonth.Month.AddMonths(1))
                                // That are not split transactions
                                .Where(f => !f.IsSplit)
                                // That are not fully funded
                                .Where(f => f.Remaining < 0)
                                // That are not projected spending
                                .Where(f => f.ForecastItemType != ForecastItemType.ProjectedSpending)
                                .ToList();

                            // Budget for each upcoming item until we run out of TBB
                            foreach (var next in upcoming)
                            {
                                // If there is no TBB, stop checking items.
                                if (tbb <= 0)
                                    break;

                                // Set the amount budgeted to zero.
                                var budget = 0L;
                                // If the TBB is more than the amount of funding still needed on the item
                                // fully fund the item and adjust the TBB.
                                if (tbb + next.Remaining >= 0)
                                {
                                    tbb += next.Remaining;
                                    budget = Math.Abs(next.Remaining);
                                    next.Funded += budget;
                                }
                                // Otherwise fund the item with the remaining TBB and zero out TBB.
                                else
                                {
                                    next.Funded += tbb.Value;
                                    budget = tbb.Value;
                                    tbb = 0;
                                }

                                // If we have budgeted something, adjust the available balance in the
                                // current month and subsequent months.
                                if (budget > 0)
                                {
                                    var md = new DateTime(next.Date.Year, next.Date.Month, 1);
                                    foreach (var nm in Months.Where(_ => _.Month >= md))
                                    {
                                        nm.ToBeBudgeted -= budget;

                                        var mc = nm.Categories.Single(_ => _.CategoryId == next.CategoryId);
                                        mc.Balance += budget;

                                        if (md == nm.Month)
                                        {
                                            nm.Budgeted += budget;
                                            mc.Budgeted += budget;
                                        }
                                    }

                                    // Add a funding record for where this income was budgeted.
                                    var cat = Categories.Single(c => c.CategoryId == next.CategoryId);
                                    AddIncomeFunding(t.TransactionId,
                                        new FundItem
                                        {
                                            CategoryId = cat.CategoryId,
                                            CategoryName = $"{cat.Name}",
                                            Date = next.Date,
                                            Payee = next.PayeeName,
                                            Amount = budget
                                        });
                                }
                            }

                            // If we have funded everything and still have money left, assign it to the remaining
                            // funds category.
                            // TODO: Support funding target balance goals and other items (emergency fund, income
                            //       replacement fund, car/home mainteance, etc).
                            if (tbb > 0)
                            {
                                // Adjust the TBB for the current and subsequent months.
                                foreach (var nm in Months.Where(_ => _.Month >= itemMonthDate))
                                {
                                    nm.ToBeBudgeted -= tbb;

                                    var mc = nm.Categories.Single(_ => _.CategoryId == RemainingFundsCategoryId);
                                    mc.Balance += tbb.Value;

                                    if (itemMonthDate == nm.Month)
                                    {
                                        nm.Budgeted += tbb;
                                        mc.Budgeted += tbb.Value;
                                    }
                                }
                                
                                // Add a funding record for where this income was budgeted.
                                var cat = Categories.Single(c => c.CategoryId == RemainingFundsCategoryId);
                                AddIncomeFunding(t.TransactionId,
                                        new FundItem
                                        {
                                            CategoryId = cat.CategoryId,
                                            CategoryName = $"{cat.Name}",
                                            Date = t.Date,
                                            Payee = "",
                                            Amount = tbb.Value
                                        });

                                tbb = 0;
                            }
                        }

                        // Setup tracking for current and next month funding status as of this income.
                        MonthFundStatus.Add(t.TransactionId, GetFundStatus(itemMonthDate));
                    }
                }
            }
            catch (Exception e)
            {
                AWSXRayRecorder.Instance.AddException(e);
                throw;
            }
            finally
            {
                AWSXRayRecorder.Instance.EndSubsegment();
            }
        }
        /// <summary>
        /// Expand the months in the budget to hold our forecasted data.
        /// </summary>
        private void ExpandMonths()
        {
            AWSXRayRecorder.Instance.BeginSubsegment("MyBudgetExplorer.Models.Forecast.ExpandMonths()");
            try
            {
                // Add one additional month beyond the forecast period.
                DateTime budgetUntil = ForecastUntil.AddMonths(1);
                // Get the latest month in the budget, it should be
                // the first month in the list.
                var latestMonth = Months[0];
                // Add a new month until we have hit our target.
                while (latestMonth.Month < budgetUntil)
                {
                    // Create the new month, with the appropriate date.
                    var month = new MonthDetail
                    {
                        Activity = latestMonth.Activity,
                        AgeOfMoney = latestMonth.AgeOfMoney,
                        Budgeted = latestMonth.Budgeted,
                        Income = latestMonth.Income,
                        Month = latestMonth.Month.AddMonths(1),
                        Note = latestMonth.Note,
                        ToBeBudgeted = latestMonth.ToBeBudgeted
                    };
                    // Add the categories.  Note that again we are adding
                    // new categories instead of copying them, otherwise
                    // we may run into issues.
                    foreach (var category in latestMonth.Categories)
                    {
                        month.Categories.Add(new Category
                        {
                            Activity = category.Activity,
                            Balance = category.Balance,
                            Budgeted = category.Budgeted,
                            CategoryGroupId = category.CategoryGroupId,
                            CategoryId = category.CategoryId,
                            Deleted = category.Deleted,
                            GoalCreationMonth = category.GoalCreationMonth,
                            GoalPercentageComplete = category.GoalPercentageComplete,
                            GoalTarget = category.GoalTarget,
                            GoalTargetMonth = category.GoalTargetMonth,
                            GoalType = category.GoalType,
                            Hidden = category.Hidden,
                            Name = category.Name,
                            Note = category.Note,
                            OriginalCategoryGroupId = category.OriginalCategoryGroupId
                        });
                    }
                    // Insert the new month.
                    Months.Insert(0, month);
                    // Set the latest month to the one we just inserted.
                    latestMonth = Months[0];
                }
            }
            catch (Exception e)
            {
                AWSXRayRecorder.Instance.AddException(e);
                throw;
            }
            finally
            {
                AWSXRayRecorder.Instance.EndSubsegment();
            }
        }
        /// <summary>
        /// Get the current funding status for all forecast items for the specified month and the following month.
        /// </summary>
        /// <param name="date">The date to check funding status on.</param>
        /// <returns>The funding status of each forecast item.</returns>
        private List<FundStatus> GetFundStatus(DateTime date)
        {
            date = new DateTime(date.Year, date.Month, 1);

            var results = new List<FundStatus>();

            var transactions = ForecastItems.Where(f => date <= f.Date && f.Date < date.AddMonths(2))
                .Where(f => f.ForecastItemType != ForecastItemType.ProjectedSpending)
                .Where(f => f.Amount < 0);

            var categories = Categories.ToDictionary(k => k.CategoryId, v => v.Name);

            return transactions.Select(t => new FundStatus
            {
                Amount = t.Amount,
                CategoryName = categories.ContainsKey(t.CategoryId) ? categories[t.CategoryId] : "[Category Not Found]",
                Date = t.Date,
                Funded = t.Funded,
                Id = t.CategoryId,
                PayeeName = t.PayeeName
            }).ToList();
        }
        #endregion

        #region ISerializable
        public Forecast(SerializationInfo info, StreamingContext context)
        {
            var reader = SerializationReader.GetReader(info);

            Accounts = reader.ReadList<Account>();
            BudgetId = reader.ReadString();
            Categories = reader.ReadList<Category>();
            CategoryGroups = reader.ReadList<CategoryGroup>();
            CurrencyFormat = (CurrencyFormat)reader.ReadObject();
            CurrentDate = reader.ReadDateTime();
            DateFormat = (DateFormat)reader.ReadObject();
            FirstMonth = reader.ReadString();
            //ForecastItems = reader.ReadList<ForecastItem>();
            ForecastUntil = reader.ReadDateTime();
            IncomeFunding = reader.ReadDictionary<string, List<FundItem>>();
            LastModifiedOn = reader.ReadDateTime();
            LastMonth = reader.ReadString();
            MonthFundStatus = reader.ReadDictionary<string, List<FundStatus>>();
            Months = reader.ReadList<MonthDetail>();
            Name = reader.ReadString();
            OriginalBudgeted = reader.ReadDictionary<DateTime, Dictionary<string, long>>();
            PayeeLocations = reader.ReadList<PayeeLocation>();
            Payees = reader.ReadList<Payee>();
            ScheduledSubTransactions = reader.ReadList<ScheduledSubTransaction>();
            ScheduledTransactions = reader.ReadList<ScheduledTransactionSummary>();
            Settings = (Settings)reader.ReadObject();
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
            writer.Write(CurrentDate);
            writer.WriteObject(DateFormat);
            writer.WriteString(FirstMonth);
            //writer.Write(ForecastItems);
            writer.Write(ForecastUntil);
            writer.Write(IncomeFunding);
            writer.Write(LastModifiedOn);
            writer.WriteString(LastMonth);
            writer.Write(MonthFundStatus);
            writer.Write(Months);
            writer.WriteString(Name);
            writer.Write(OriginalBudgeted);
            writer.Write(PayeeLocations);
            writer.Write(Payees);
            writer.Write(ScheduledSubTransactions);
            writer.Write(ScheduledTransactions);
            writer.WriteObject(Settings);
            writer.Write(SubTransactions);
            writer.Write(Transactions);

            writer.AddToInfo(info);
        }
        #endregion
    }
}
