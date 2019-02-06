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
        public IList<ForecastItem> ForecastItems { get; set; } = new List<ForecastItem>();
        public DateTime ForecastUntil { get; set; }
        public DateTime FutureMonthStart { get; private set; }
        private IDictionary<string, List<FundItem>> IncomeFunding { get; } = new Dictionary<string, List<FundItem>>();
        public DateTime LastModifiedOn { get; set; }
        public string LastMonth { get; set; }
        public IDictionary<string, List<FundStatus>> MonthFundStatus { get; } = new Dictionary<string, List<FundStatus>>();
        public IList<MonthDetail> Months { get; set; } = new List<MonthDetail>();
        public string Name { get; set; }
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
        private Forecast(BudgetDetail budget, int forecastMonths)
        {
            AWSXRayRecorder.Instance.BeginSubsegment("MyBudgetExplorer.Models.Forecast.Forecast()");
            try
            {
                if (budget == null)
                    throw new ArgumentNullException("budget");


                if (forecastMonths < 1)
                    forecastMonths = 1;

                CurrentDate = DateTime.Now;
                ForecastUntil = CurrentMonthStart.AddMonths(forecastMonths);

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
                CategoryGroups.Insert(0, new CategoryGroup
                {
                    CategoryGroupId = ProgramCategoryGroupId,
                    Name = "My Budget Explorer for YNAB"
                });

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
        public List<FundItem> GetIncomeFunding(string transactionId)
        {
            return IncomeFunding[transactionId];
        }
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
        private void ApplyScenarios()
        {
            AWSXRayRecorder.Instance.BeginSubsegment("MyBudgetExplorer.Models.Forecast.ApplyScenarios()");
            try
            {
                if (ForecastItems.Count == 0)
                    return;

                var maxDate = ForecastItems.Max(f => f.Date).AddDays(1);
                foreach (var scenario in Settings.ScheduledTransactionScenarios.OrderBy(s => s.BeginDate))
                {
                    if (!scenario.IsEnabled)
                        continue;
                    var sDate = scenario.BeginDate;
                    while (sDate < maxDate)
                    {
                        foreach (var item in ForecastItems)
                        {
                            if (scenario.ScheduledTransactionId != item.ScheduledTransactionId)
                                continue;
                            if (item.Date < sDate)
                                continue;
                            if (!scenario.IsExactAmount)
                            {
                                item.Amount = Convert.ToInt64(decimal.Multiply(item.Amount, decimal.Divide(100M + decimal.Divide(scenario.Amount, 1000), 100)));
                            }
                            else
                            {
                                item.Amount += scenario.Amount;
                            }
                        }
                        sDate = AdvanceDate(sDate, scenario.Frequency);
                    }
                }
                foreach (var scenario in Settings.ScheduledSubTransactionScenarios)
                {
                    if (!scenario.IsEnabled)
                        continue;
                    var sDate = scenario.BeginDate;
                    while (sDate < maxDate)
                    {
                        foreach (var item in ForecastItems)
                        {
                            if (scenario.ScheduledSubTransactionId != item.ScheduledSubTransactionId)
                                continue;
                            if (item.Date < sDate)
                                continue;
                            if (!scenario.IsExactAmount)
                            {
                                var previous = item.Amount;
                                item.Amount = Convert.ToInt64(decimal.Multiply(item.Amount, decimal.Divide(100M + decimal.Divide(scenario.Amount, 1000), 100)));
                                var diff = item.Amount - previous;
                                var parent = ForecastItems.Single(f => f.TransactionId == item.TransactionId && f.SubTransactionId == null);
                                parent.Amount += diff;
                            }
                            else
                            {
                                item.Amount += scenario.Amount;
                                var parent = ForecastItems.Single(f => f.TransactionId == item.TransactionId && f.SubTransactionId == null);
                                parent.Amount += scenario.Amount;
                            }
                        }
                        sDate = AdvanceDate(sDate, scenario.Frequency);
                    }
                }
                // Projected Spending is handled in the CreateForecastItems and ExecuteForecast methods.
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
        private void CreateForecastItems()
        {
            AWSXRayRecorder.Instance.BeginSubsegment("MyBudgetExplorer.Models.Forecast.CreateForecastItems()");
            try
            {
                var incomeEnd = ForecastUntil;
                var expenseEnd = incomeEnd.AddMonths(1);
                var results = new List<ForecastItem>();

                // Get a list of all scheduled transactions until the end date.
                var ses = new List<ScheduledTransactionSummary>();
                foreach (var st in ScheduledTransactions)
                {
                    if (st.Deleted)
                        continue;
                    if (st.Amount >= 0 && st.DateNext >= incomeEnd)
                        continue;
                    if (st.Amount < 0 && st.DateNext >= expenseEnd)
                        continue;

                    ses.Add(new ScheduledTransactionSummary
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

                // Sort the scheduled transactions
                ses = ses.OrderBy(se => se.DateNext).ThenBy(se => se.Amount).ToList();

                // Expand out the scheduled transactions to all their dates until the end date.
                while (ses.Count > 0)
                {
                    var se = ses.First();
                    if (se.DateNext < CurrentDate)
                    {
                        se.DateNext = AdvanceDate(se.DateNext, se.Frequency);
                        if ((se.Amount >= 0 && se.DateNext >= incomeEnd) || (se.Amount < 0 && se.DateNext >= expenseEnd))
                        {
                            ses = ses
                                .Where(e => (e.Amount >= 0 && e.DateNext < incomeEnd) || (e.Amount < 0 && e.DateNext < expenseEnd))
                                .ToList();
                            continue;
                        }
                    }

                    var seMonth = Months.SingleOrDefault(m => m.Month == new DateTime(se.DateNext.Year, se.DateNext.Month, 1));

                    var forecastItem = new ForecastItem
                    {
                        ForecastItemType = ForecastItemType.ScheduledTransaction,
                        AccountId = se.AccountId,
                        FlagColor = se.FlagColor,
                        Memo = se.Memo,
                        TransferAccountId = se.TransferAccountId,
                        IsSplit = false,
                        PayeeId = se.PayeeId,
                        CategoryId = se.CategoryId,
                        CategoryName = "Split",
                        Date = se.DateNext,
                        ScheduledTransactionId = se.ScheduledTransactionId,
                        TransactionId = $"{se.ScheduledTransactionId}_{se.DateNext.ToString("yyyy-MM-dd")}",
                        Amount = se.Amount,
                        Funded = 0
                    };
                    var payee = Payees.SingleOrDefault(p => p.PayeeId == se.PayeeId);
                    if (payee != null)
                        forecastItem.PayeeName = payee.Name;
                    else
                        forecastItem.PayeeName = "[Unknown Payee]";

                    // Get the category for the scheduled transaction.  Split transactions will return null.
                    var seCategory = seMonth.Categories.SingleOrDefault(c => c.CategoryId == se.CategoryId);

                    // If this is not a split transaction, add it.
                    if (seCategory != null)
                    {
                        forecastItem.CategoryName = seCategory.Name;
                        results.Add(forecastItem);
                    }
                    // For split transactions, add each sub transaction.
                    else
                    {
                        forecastItem.IsSplit = true;
                        results.Add(forecastItem);
                        foreach (var sse in ScheduledSubTransactions)
                        {
                            if (sse.ScheduledTransactionId == se.ScheduledTransactionId)
                            {
                                var sseCategory = seMonth.Categories.SingleOrDefault(c => c.CategoryId == sse.CategoryId);
                                results.Add(new ForecastItem
                                {
                                    ForecastItemType = ForecastItemType.ScheduledSubTransaction,
                                    AccountId = se.AccountId,
                                    FlagColor = se.FlagColor,
                                    Memo = se.Memo,
                                    TransferAccountId = se.TransferAccountId,
                                    IsSplit = false,
                                    PayeeId = se.PayeeId,
                                    PayeeName = forecastItem.PayeeName,
                                    CategoryId = sse.CategoryId,
                                    CategoryName = sseCategory.Name,
                                    Date = se.DateNext,
                                    ScheduledTransactionId = se.ScheduledTransactionId,
                                    TransactionId = forecastItem.TransactionId,
                                    ScheduledSubTransactionId = sse.ScheduledSubTransactionId,
                                    SubTransactionId = $"{sse.ScheduledSubTransactionId}_{se.DateNext.ToString("yyyy-MM-dd")}",
                                    Amount = sse.Amount,
                                    Funded = 0
                                });
                            }
                        }
                    }

                    se.DateNext = AdvanceDate(se.DateNext, se.Frequency);
                    ses = ses
                        .Where(e => (e.Amount >= 0 && e.DateNext < incomeEnd) || (e.Amount < 0 && e.DateNext < expenseEnd))
                        .ToList();
                }

                foreach (var month in Months.Where(m => m.Month >= CurrentMonthStart && m.Month < expenseEnd).OrderBy(m => m.Month))
                {
                    foreach (var cat in month.Categories)
                    {
                        var dates = new[] { 1, 15 };
                        //if (month.Month.Year == currentDate.Year && month.Month.Month == currentDate.Month)
                        //    dates = dates.Where(d => d >= currentDate.Day).ToArray();
                        //if (dates.Length == 0)
                        //    dates = new[] { currentDate.Day };
                        if (cat.GoalType.HasValue && !string.IsNullOrWhiteSpace(cat.GoalCreationMonth) && DateTime.Parse(cat.GoalCreationMonth) <= month.Month)
                        {
                            switch (cat.GoalType)
                            {
                                case GoalType.MF:
                                    var remaining = cat.GoalTarget;
                                    if (remaining > 0)
                                    {
                                        remaining *= -1;
                                        var perTime = Convert.ToInt64(Math.Floor(Decimal.Divide(remaining, dates.Length)));
                                        foreach (var day in dates)
                                        {
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
                                            if (perTime < remaining)
                                            {
                                                result.Amount = remaining;
                                                remaining = 0;
                                            }
                                            else
                                            {
                                                result.Amount = perTime;
                                                remaining -= perTime;
                                            }
                                            results.Add(result);
                                        }
                                    }

                                    var projected = Settings.ProjectedSpendingScenarios.SingleOrDefault(p => p.CategoryId == cat.CategoryId);
                                    if (projected != null)
                                    {
                                        remaining = cat.GoalTarget;
                                        if (remaining > 0)
                                        {
                                            remaining *= -1;
                                            var perTime = Convert.ToInt64(Math.Floor(Decimal.Divide(remaining, dates.Length)));
                                            foreach (var day in projected.Days)
                                            {
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
                                                if (perTime < remaining)
                                                {
                                                    result.Amount = remaining;
                                                    remaining = 0;
                                                }
                                                else
                                                {
                                                    result.Amount = perTime;
                                                    remaining -= perTime;
                                                }
                                                results.Add(result);
                                            }
                                        }
                                    }
                                    break;
                                case GoalType.TB:
                                    // TODO: Implment support
                                    break;
                                case GoalType.TBD:
                                    // TODO: Implment support
                                    break;
                                default:
                                    throw new NotImplementedException();
                            }
                        }
                    }
                }

                var days = results.Select(r => r.Date).Distinct().OrderBy(r => r.Date).ToList();
                foreach (var day in days)
                {
                    var initialCount = ForecastItems.Count;

                    var all = results.Where(r => r.Date == day).ToList();
                    // Add Income Scheduled Transactions
                    var incomes = all.Where(r => r.Amount >= 0 && r.ScheduledSubTransactionId == null).OrderByDescending(r => r.Amount);
                    foreach (var income in incomes)
                    {
                        ForecastItems.Add(income);
                        // Add sub transactions
                        foreach (var sub in all.Where(r => r.ScheduledTransactionId == income.ScheduledTransactionId && r.ScheduledSubTransactionId != null).OrderByDescending(r => r.Amount))
                            ForecastItems.Add(sub);
                    }

                    // Add Expense Scheduled Transactions
                    var expenses = all.Where(r => r.Amount < 0 && r.ScheduledSubTransactionId == null).OrderBy(r => r.Amount);
                    foreach (var expense in expenses)
                    {
                        ForecastItems.Add(expense);
                        // // Add sub transactions
                        foreach (var item in all.Where(r => r.ScheduledTransactionId == expense.ScheduledTransactionId && r.ScheduledSubTransactionId != null).OrderBy(r => r.Amount))
                            ForecastItems.Add(item);
                    }

                    if (ForecastItems.Count != initialCount + all.Count)
                        throw new ApplicationException("Failed creating forecast items.");
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
        private void ExecuteForecast()
        {
            AWSXRayRecorder.Instance.BeginSubsegment("MyBudgetExplorer.Models.Forecast.Forecast()");
            try
            {
                #region Set initial funding
                // Get current category available.
                var balances = new List<Tuple<DateTime, string, long>>();
                foreach (var month in Months.Where(m => m.Month >= CurrentMonthStart))
                    balances.AddRange(month.Categories.Select(c => new Tuple<DateTime, string, long>(month.Month, c.CategoryId, c.Balance)));
                // Get current category budgeted.
                var budgeted = new List<Tuple<DateTime, string, long>>();
                foreach (var month in Months.Where(m => m.Month >= CurrentMonthStart))
                    budgeted.AddRange(month.Categories.Select(c => new Tuple<DateTime, string, long>(month.Month, c.CategoryId, c.Budgeted)));
                // Set funding.
                foreach (var item in ForecastItems)
                {
                    if (item.Remaining >= 0)
                        continue;

                    if (item.ForecastItemType == ForecastItemType.GoalFunding)
                    {
                        var monthDate = new DateTime(item.Date.Year, item.Date.Month, 1);
                        var budget = budgeted.SingleOrDefault(b => b.Item1 == monthDate && b.Item2 == item.CategoryId);
                        if (budget == null)
                            continue;
                        var adjustment = 0L;
                        if (budget.Item3 + item.Remaining >= 0)
                        {
                            adjustment += item.Remaining;
                            item.Funded -= item.Remaining;
                        }
                        else
                        {
                            item.Funded += budget.Item3;
                            adjustment -= budget.Item3;
                        }
                        for (int i = 0; i < budgeted.Count; i++)
                        {
                            var tuple = budgeted[i];
                            if (tuple.Item1 == monthDate && tuple.Item2 == item.CategoryId)
                                budgeted[i] = new Tuple<DateTime, string, long>(tuple.Item1, tuple.Item2, tuple.Item3 + adjustment);
                        }
                    }
                    else if (item.ForecastItemType != ForecastItemType.ProjectedSpending)
                    {
                        var monthDate = new DateTime(item.Date.Year, item.Date.Month, 1);
                        var balance = balances.SingleOrDefault(b => b.Item1 == monthDate && b.Item2 == item.CategoryId);
                        if (balance == null)
                            continue;
                        var adjustment = 0L;
                        if (balance.Item3 + item.Remaining >= 0)
                        {
                            adjustment += item.Remaining;
                            item.Funded -= item.Remaining;
                        }
                        else
                        {
                            item.Funded += balance.Item3;
                            adjustment -= balance.Item3;
                        }
                        for (int i = 0; i < balances.Count; i++)
                        {
                            var tuple = balances[i];
                            if (tuple.Item1 >= monthDate && tuple.Item2 == item.CategoryId)
                                balances[i] = new Tuple<DateTime, string, long>(tuple.Item1, tuple.Item2, tuple.Item3 + adjustment);
                        }
                    }
                }
                #endregion

                MonthFundStatus.Add(CurrentMonthStart.ToShortDateString(), GetFundStatus(CurrentMonthStart));

                foreach (var item in ForecastItems)
                {
                    if (item.ForecastItemType == ForecastItemType.GoalFunding)
                        continue;
                    if (item.ForecastItemType == ForecastItemType.ScheduledSubTransaction)
                        continue;

                    var itemMonthDate = new DateTime(item.Date.Year, item.Date.Month, 1);

                    // Create and add the transaction
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
                        // TODO: Determine what to do here.
                        TransferTransactionId = string.Empty
                    };

                    // Modify projected balance transactions
                    if (item.ForecastItemType == ForecastItemType.ProjectedSpending)
                    {
                        t.ImportId = "projected";
                        var availableAmount = Months.Single(m => m.Month == itemMonthDate).Categories.Single(c => c.CategoryId == item.CategoryId).Balance;
                        var numberRemaining = ForecastItems.Where(f => f.ForecastItemType == ForecastItemType.ProjectedSpending && f.CategoryId == item.CategoryId && f.Date >= item.Date && f.Date < itemMonthDate.AddMonths(1)).Count();
                        t.Amount = Convert.ToInt64(decimal.Divide(availableAmount, numberRemaining)) * -1;
                    }
                    Transactions.Insert(0, t);

                    // Adjust category balances
                    AdjustCategoryBalances(itemMonthDate, t.CategoryId, t.Amount);

                    var subItems = ForecastItems.Where(f => f.TransactionId == item.TransactionId && f.SubTransactionId != null).ToList();
                    foreach (var subItem in subItems)
                    {
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
                        SubTransactions.Insert(0, s);

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

                        AdjustCategoryBalances(itemMonthDate, s.CategoryId, s.Amount);
                    }

                    // If this was income, budget it.
                    if (t.Amount > 0)
                    {
                        var month = Months.Single(m => m.Month == itemMonthDate);
                        var nextMonth = Months.Single(m => m.Month == itemMonthDate.AddMonths(1));
                        var tbb = month.ToBeBudgeted - Months.Where(m => m.Month > itemMonthDate).Sum(m => m.Budgeted);
                        if (tbb > 0)
                        {
                            var upcoming = ForecastItems
                                .Where(f => f.Date < nextMonth.Month.AddMonths(1))
                                .Where(f => !f.IsSplit)
                                .Where(f => f.Remaining < 0)
                                .Where(f => f.ForecastItemType != ForecastItemType.ProjectedSpending)
                                .ToList();

                            // Budget for what we can
                            foreach (var next in upcoming)
                            {
                                if (tbb <= 0)
                                    break;

                                var budget = 0L;
                                if (tbb + next.Remaining >= 0)
                                {
                                    tbb += next.Remaining;
                                    budget = Math.Abs(next.Remaining);
                                    next.Funded += budget;
                                }
                                else
                                {
                                    next.Funded += tbb.Value;
                                    budget = tbb.Value;
                                    tbb = 0;
                                }

                                if (budget > 0)
                                {
                                    // todo the budget, and record information about what, where and when we budgeted
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

                            if (tbb > 0)
                            {
                                // todo the budget, and record information about what, where and when we budgeted
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

                        // Setup tracking for current and next month funding.
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
        private void ExpandMonths()
        {
            AWSXRayRecorder.Instance.BeginSubsegment("MyBudgetExplorer.Models.Forecast.ExpandMonths()");
            try
            {
                DateTime budgetUntil = ForecastUntil.AddMonths(1);
                var currentMonth = Months[0];
                while (currentMonth.Month < budgetUntil)
                {
                    var month = new MonthDetail
                    {
                        Activity = currentMonth.Activity,
                        AgeOfMoney = currentMonth.AgeOfMoney,
                        Budgeted = currentMonth.Budgeted,
                        Income = currentMonth.Income,
                        Month = currentMonth.Month.AddMonths(1),
                        Note = currentMonth.Note,
                        ToBeBudgeted = currentMonth.ToBeBudgeted
                    };
                    foreach (var category in currentMonth.Categories)
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
                    Months.Insert(0, month);
                    currentMonth = Months[0];
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
        private List<FundStatus> GetFundStatus(DateTime date)
        {
            date = new DateTime(date.Year, date.Month, 1);

            var results = new List<FundStatus>();

            var transactions = ForecastItems.Where(f => date <= f.Date && f.Date < date.AddMonths(2))
                .Where(f => f.ForecastItemType != ForecastItemType.ProjectedSpending)
                .Where(f => f.Amount < 0);

            return transactions.Select(t => new FundStatus
            {
                Amount = t.Amount,
                CategoryName = Categories.Single(c => c.CategoryId == t.CategoryId).Name,
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
            ForecastItems = reader.ReadList<ForecastItem>();
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
            writer.Write(ForecastItems);
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
