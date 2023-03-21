using Newtonsoft.Json;

namespace DapperLabs.Flow.Sdk.Cadence
{
    /// <summary>
    /// Provides support for the Cadence Type type
    /// </summary>
    public class CadenceVoid : CadenceBase
    {
        [JsonProperty("type")]
        public override string Type => "Void";

        /// <summary>
        /// Gets a string representation of this cadence value. Useful for debugging. 
        /// </summary>
        /// <returns>A string representation of this cadence value.</returns>
        public override string GetValue()
        {
            return "";
        }
    }
}
