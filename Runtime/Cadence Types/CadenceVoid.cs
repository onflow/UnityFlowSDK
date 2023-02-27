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

        public override string GetValue()
        {
            return "";
        }
    }
}
