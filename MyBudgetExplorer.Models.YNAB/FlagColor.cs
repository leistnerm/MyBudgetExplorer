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
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Runtime.Serialization;

namespace MyBudgetExplorer.Models.YNAB
{
    [Serializable]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum FlagColor : byte
    {
        [EnumMember(Value = "red")]
        Red = 0,
        [EnumMember(Value = "orange")]
        Orange = 1,
        [EnumMember(Value = "yellow")]
        Yellow = 2,
        [EnumMember(Value = "green")]
        Green = 3,
        [EnumMember(Value = "blue")]
        Blue = 4,
        [EnumMember(Value = "purple")]
        Purple = 5
    }
}
