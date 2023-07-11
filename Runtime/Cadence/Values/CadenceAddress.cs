using Newtonsoft.Json;

namespace DapperLabs.Flow.Sdk.Cadence
{
    /// <summary>
    /// Provides support for the Cadence Address type
    /// </summary>
    public class CadenceAddress : CadenceBase
    {
        [JsonProperty("type")]
        public override string Type => "Address";

        [JsonProperty("value")]
        public string Value { get; set; }

        /// <summary>
        /// Constructs an empty Cadence Address
        /// </summary>
        public CadenceAddress() { }

        /// <summary>
        /// Constructs a Cadence Address from the provided string
        /// </summary>
        /// <param name="value">String representation of the Cadence Address</param>
        public CadenceAddress(string value)
        {
            if (value.StartsWith("0x") == false)
            {
                Value = "0x" + value;
            }
            else
            {
                Value = value;
            }
        }

        /// <summary>
        /// Gets a string representation of this cadence value. Useful for debugging. 
        /// </summary>
        /// <returns>A string representation of this cadence value.</returns>
        public override string GetValue()
        {
            return Value;
        }
    }
}
