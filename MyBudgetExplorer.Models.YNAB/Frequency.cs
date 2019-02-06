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
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Runtime.Serialization;

namespace MyBudgetExplorer.Models.YNAB
{
    [JsonConverter(typeof(StringEnumConverter))]
    [Serializable]
    public enum Frequency : byte
    {
        [EnumMember(Value = "never")]
        Never = 0,
        [EnumMember(Value = "daily")]
        Daily = 1,
        [EnumMember(Value = "weekly")]
        Weekly = 2,
        [EnumMember(Value = "everyOtherWeek")]
        EveryOtherWeek = 3,
        [EnumMember(Value = "twiceAMonth")]
        TwiceAMonth = 4,
        [EnumMember(Value = "every4Weeks")]
        Every4Weeks = 5,
        [EnumMember(Value = "monthly")]
        Monthly = 6,
        [EnumMember(Value = "everyOtherMonth")]
        EveryOtherMonth = 7,
        [EnumMember(Value = "every3Months")]
        Every3Months = 8,
        [EnumMember(Value = "every4Months")]
        Every4Months = 9,
        [EnumMember(Value = "twiceAYear")]
        TwiceAYear = 10,
        [EnumMember(Value = "yearly")]
        Yearly = 11,
        [EnumMember(Value = "everyOtherYear ")]
        EveryOtherYear = 12
    }
}
