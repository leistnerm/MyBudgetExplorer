﻿/* 
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
using MyBudgetExplorer.Models.YNAB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyBudgetExplorer.Models
{
    [Serializable]
    public class Settings
    {
        public List<ScheduledTransactionScenario> ScheduledTransactionScenarios { get; set; } = new List<ScheduledTransactionScenario>();
        public List<ScheduledSubTransactionScenario> ScheduledSubTransactionScenarios { get; set; } = new List<ScheduledSubTransactionScenario>();
        public List<ProjectedSpendingScenario> ProjectedSpendingScenarios { get; set; } = new List<ProjectedSpendingScenario>();

        public Settings()
        {
            // Grocery
            ProjectedSpendingScenarios.Add(new ProjectedSpendingScenario
            {
                Amount = -900000,
                Days = new[] { 1, 8, 15, 22 },
                IsEnabled = true,
                IsExactAmount = true,
                ScenarioId = Guid.NewGuid().ToString(),
                CategoryId = "9e6ac45f-2b3d-469f-9b4e-ece904d480fd",
                AccountId = "0688cd93-c997-4c8f-ac5e-9da26f40c4cd"
            });

            // Gas
            ProjectedSpendingScenarios.Add(new ProjectedSpendingScenario
            {
                Amount = -300000,
                Days = new[] { 1, 8, 15, 22 },
                IsEnabled = true,
                IsExactAmount = true,
                ScenarioId = Guid.NewGuid().ToString(),
                CategoryId = "fe7fec51-e0f1-40f3-9e54-8d7ce56f83d4",
                AccountId = "0688cd93-c997-4c8f-ac5e-9da26f40c4cd"
            });
        }
    }
}
