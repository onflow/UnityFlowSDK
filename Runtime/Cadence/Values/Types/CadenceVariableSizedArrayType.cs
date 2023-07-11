using Newtonsoft.Json;

namespace DapperLabs.Flow.Sdk.Cadence.Types
{
    /// <summary>
    /// Represents the cadence static type for a CadenceVariableSizedArray
    /// </summary>
    public class CadenceVariableSizedArrayType : CadenceTypeBase
    {
        /// <summary>
        /// Constructs an empty CadenceVariableSizedArrayType
        /// </summary>
        public CadenceVariableSizedArrayType() { }

        /// <summary>
        /// Constructs a CadenceVariableSizedArrayType from the given parameter
        /// </summary>
        /// <param name="type">Cadence type of the array</param>
        public CadenceVariableSizedArrayType(CadenceTypeBase type)
        {
            Type = type;
        }

        [JsonProperty("kind")]
        public override string Kind => "VariableSizedArray";

        [JsonProperty("type")]
        public CadenceTypeBase Type { get; set; }
    }
}
