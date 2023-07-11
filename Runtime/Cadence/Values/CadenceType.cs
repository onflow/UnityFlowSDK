using DapperLabs.Flow.Sdk.Cadence.Types;
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

        /// <summary>
        /// Gets a string representation of this cadence value. Useful for debugging. 
        /// </summary>
        /// <returns>A string representation of this cadence value.</returns>
        public override string GetValue()
        {
            return JsonConvert.SerializeObject(Value);
        }
    }

    /// <summary>
    /// Value containing the cadence static type. These are all listed under the Types subfolder. 
    /// </summary>
    public class CadenceTypeValue
    {
        [JsonProperty("staticType")]
        public CadenceTypeBase StaticType { get; set; }
    }
}
