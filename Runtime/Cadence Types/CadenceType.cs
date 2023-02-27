using DapperLabs.Flow.Sdk.Exceptions;
using Newtonsoft.Json;
using System;

namespace DapperLabs.Flow.Sdk.Cadence
{
    /// <summary>
    /// Provides support for the Cadence Type type
    /// </summary>
    public class CadenceType : CadenceBase
    {
        [JsonProperty("type")]
        public override string Type => "Type";

        [JsonProperty("value")]
        public CadenceTypeValue Value { get; set; }

        /// <summary>
        /// Constructs an empty CadenceType
        /// </summary>
        public CadenceType() { }

        /// <summary>
        /// Constructs a CadenceType of the given CadenceTypeValue
        /// </summary>
        /// <param name="value">A CadenceTypeValue containing the desired type</param>
        public CadenceType(CadenceTypeValue value)
        {
            Value = value;
        }

        public override string GetValue()
        {
            return JsonConvert.SerializeObject(Value);
        }
    }

    public class CadenceTypeValue
    {
        [JsonProperty("staticType")]
        public string StaticType { get; set; }
    }
}
