using Newtonsoft.Json;

namespace DapperLabs.Flow.Sdk.Cadence.Types
{
    /// <summary>
    /// Represents the cadence static type for a CadenceOptional
    /// </summary>
    public class CadenceOptionalType : CadenceTypeBase
    {
        /// <summary>
        /// Constructs an empty CadenceOptionalType
        /// </summary>
        public CadenceOptionalType() { }

        /// <summary>
        /// Constructs a CadenceOptionalType from the given parameter
        /// </summary>
        /// <param name="type">Cadence type of the optional</param>
        public CadenceOptionalType(CadenceTypeBase type)
        {
            Type = type;
        }

        [JsonProperty("kind")]
        public override string Kind => "Optional";

        [JsonProperty("type")]
        public CadenceTypeBase Type { get; set; }
    }
}
