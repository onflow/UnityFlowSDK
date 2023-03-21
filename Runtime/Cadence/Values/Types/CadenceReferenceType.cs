using Newtonsoft.Json;

namespace DapperLabs.Flow.Sdk.Cadence.Types
{
    /// <summary>
    /// Represents the cadence static type for a CadenceReference
    /// </summary>
    public class CadenceReferenceType : CadenceTypeBase
    {
        /// <summary>
        /// Constructs an empty CadenceReferenceType
        /// </summary>
        public CadenceReferenceType() { }

        /// <summary>
        /// Constructs a CadenceReferenceType from the given parameters
        /// </summary>
        /// <param name="authorized">Whether the reference is authorized or not</param>
        /// <param name="type">Cadence type of the reference</param>
        public CadenceReferenceType(bool authorized, CadenceTypeBase type)
        {
            Authorized = authorized;
            Type = type;
        }

        [JsonProperty("kind")]
        public override string Kind => "Reference";

        [JsonProperty("authorized")]
        public bool Authorized { get; set; }

        [JsonProperty("type")]
        public CadenceTypeBase Type { get; set; }
    }
}
