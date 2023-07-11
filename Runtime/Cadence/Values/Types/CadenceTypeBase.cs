using Newtonsoft.Json;

namespace DapperLabs.Flow.Sdk.Cadence.Types
{
    /// <summary>
    /// Base class for all Cadence types
    /// </summary>
    public class CadenceTypeBase
    {
        [JsonProperty("kind")]
        public virtual string Kind { get; set; }
    }
}
