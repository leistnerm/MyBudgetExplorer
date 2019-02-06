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

namespace MyBudgetExplorer.Models.YNAB
{
    [Serializable]
    public class CurrencyFormat
    {
        #region Properties
        public string CurrencySymbol { get; set; }
        public int DecimalDigits { get; set; }
        public string DecimalSeparator { get; set; }
        public bool DisplaySymbol { get; set; }
        public string ExampleFormat { get; set; }
        public string GroupSeparator { get; set; }
        public string IsoCode { get; set; }
        public bool SymbolFirst { get; set; }
        #endregion

        #region Public Methods
        public static CurrencyFormat Load(dynamic d)
        {
            return new CurrencyFormat
            {
                CurrencySymbol = d.currency_symbol,
                DecimalDigits = d.decimal_digits,
                DecimalSeparator = d.decimal_separator,
                DisplaySymbol = d.display_symbol,
                ExampleFormat = d.example_format,
                GroupSeparator = d.group_separator,
                IsoCode = d.iso_code,
                SymbolFirst = d.symbol_first,
            };
        }
        #endregion
    }
}
