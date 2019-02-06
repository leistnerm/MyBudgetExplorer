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
    [Serializable]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AccountType : byte
    {
        [EnumMember(Value = "checking")]
        Checking = 0,
        [EnumMember(Value = "savings")]
        Savings = 1,
        [EnumMember(Value = "cash")]
        Cash = 2,
        [EnumMember(Value = "creditCard")]
        CreditCard = 3,
        [EnumMember(Value = "lineOfCredit")]
        LineOfCredit = 4,
        [EnumMember(Value = "otherAsset")]
        OtherAsset = 5,
        [EnumMember(Value = "otherLiability")]
        OtherLiability = 6,
        [Obsolete, EnumMember(Value = "paypal")]
        PayPal = 7,
        [Obsolete, EnumMember(Value = "merchantAccount")]
        MerchantAccount = 8,
        [Obsolete, EnumMember(Value = "investmentAccount")]
        InvestmentAccount = 9,
        [Obsolete, EnumMember(Value = "mortgage")]
        Mortgage = 10
    }
}
