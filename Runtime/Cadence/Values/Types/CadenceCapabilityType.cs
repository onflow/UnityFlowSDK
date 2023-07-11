using Newtonsoft.Json;

namespace DapperLabs.Flow.Sdk.Cadence.Types
{
    /// <summary>
    /// Represents the cadence static type Capability
    /// </summary>
    public class CadenceCapabilityType : CadenceTypeBase
    {
        /// <summary>
        /// Constructs an empty CadenceCapabilityType
        /// </summary>
        public CadenceCapabilityType() { }

        /// <summary>
        /// Constructs a CadenceCapabilityType from the given type
        /// </summary>
        /// <param name="type">A CadenceTypeBase</param>
        public CadenceCapabilityType(CadenceTypeBase type)
        {
            Type = type;
        }

        [JsonProperty("kind")]
        public override string Kind => "Capability";

        [JsonProperty("type")]
        public CadenceTypeBase Type { get; set; }
    }
}
