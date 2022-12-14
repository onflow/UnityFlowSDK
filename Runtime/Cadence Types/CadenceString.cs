using Newtonsoft.Json;

namespace DapperLabs.Flow.Sdk.Cadence
{
    /// <summary>
    /// Provides support for the Cadence String type
    /// </summary>
    public class CadenceString : CadenceBase
    {
        [JsonProperty("type")]
        public override string Type => "String";

        [JsonProperty("value")]
        public string Value { get; set; }

        /// <summary>
        /// Constructs an empty CadenceString
        /// </summary>
        public CadenceString() {}

        /// <summary>
        /// Constructs a CadenceString containing the passed value
        /// </summary>
        /// <param name="value">The string that the CadenceString should represent</param>
        public CadenceString(string value)
        {
            Value = value;
        }
    }
}
