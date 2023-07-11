using Newtonsoft.Json;

namespace DapperLabs.Flow.Sdk.Cadence
{
    /// <summary>
    /// Provides support for the Cadence Bool type
    /// </summary>
    public class CadenceBool : CadenceBase
    {
        [JsonProperty("type")]
        public override string Type => "Bool";

        [JsonProperty("value")]
        public bool Value { get; set; }

        /// <summary>
        /// Constructs an empty CadenceBool
        /// </summary>
        public CadenceBool() { }

        /// <summary>
        /// Constructs a CadenceBool from the given bool value
        /// </summary>
        /// <param name="value">The boolean value of the returned CadenceBool</param>
        public CadenceBool(bool value)
        {
            Value = value;
        }

        /// <summary>
        /// Gets a string representation of this cadence value. Useful for debugging. 
        /// </summary>
        /// <returns>A string representation of this cadence value.</returns>
        public override string GetValue()
        {
            return Value.ToString();
        }
    }
}
