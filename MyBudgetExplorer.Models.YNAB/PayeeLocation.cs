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
using MyBudgetExplorer.Models.BinarySerialization;
using System;
using System.Runtime.Serialization;

namespace MyBudgetExplorer.Models.YNAB
{
    [Serializable]
    public class PayeeLocation : ISerializable
    {
        #region Properties
        public bool Deleted { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string PayeeId { get; set; }
        public string PayeeLocationId { get; set; }
        #endregion

        #region Constructors
        public PayeeLocation() { }
        #endregion

        #region Public Methods
        public static PayeeLocation Load(dynamic dyn)
        {
            Func<dynamic, PayeeLocation> func = (d) =>
            {
                var location = new PayeeLocation
                {
                    Deleted = d.deleted,
                    Latitude = d.latitude,
                    Longitude = d.longitude,
                    PayeeId = d.payee_id,
                    PayeeLocationId = d.id,
                };
                return location;
            };

            return YnabApi.ProcessApiResult(dyn, func);
        }
        #endregion

        #region ISerializable
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            var writer = SerializationWriter.GetWriter();

            writer.Write(Deleted);
            writer.WriteString(Latitude);
            writer.WriteString(Longitude);
            writer.WriteString(PayeeId);
            writer.WriteString(PayeeLocationId);

            writer.AddToInfo(info);
        }
        public PayeeLocation(SerializationInfo info, StreamingContext context)
        {
            var reader = SerializationReader.GetReader(info);

            Deleted = reader.ReadBoolean();
            Latitude = reader.ReadString();
            Longitude = reader.ReadString();
            PayeeId = reader.ReadString();
            PayeeLocationId = reader.ReadString();
        }
        #endregion
    }
}
