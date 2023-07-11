using Newtonsoft.Json;

namespace DapperLabs.Flow.Sdk.Cadence
{
    /// <summary>
    /// Provides support for the Cadence Character type
    /// </summary>
    public class CadenceCharacter : CadenceBase
    {
        [JsonProperty("type")]
        public override string Type => "Character";

        [JsonProperty("value")]
        public string Value { get; set; }

        /// <summary>
        /// Constructs an empty CadenceCharacter
        /// </summary>
        public CadenceCharacter() {}

        /// <summary>
        /// Constructs a CadenceCharacter containing the passed value
        /// </summary>
        /// <param name="value">The string that the CadenceCharacter should represent</param>
        public CadenceCharacter(char value)
        {
            Value = value.ToString();
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
