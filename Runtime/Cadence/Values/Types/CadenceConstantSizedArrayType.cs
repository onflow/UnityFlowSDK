using Newtonsoft.Json;

namespace DapperLabs.Flow.Sdk.Cadence.Types
{
    /// <summary>
    /// Represents the cadence static type for a ConstantSizedArray
    /// </summary>
    public class CadenceConstantSizedArrayType : CadenceTypeBase
    {
        /// <summary>
        /// Constructs an empty CadenceConstantSizedArrayType
        /// </summary>
        public CadenceConstantSizedArrayType() { }

        /// <summary>
        /// Constructs a CadenceConstantSizedArrayType from the given parameters
        /// </summary>
        /// <param name="type">The cadence type of the array</param>
        /// <param name="size">The constant size of the array</param>
        public CadenceConstantSizedArrayType(CadenceTypeBase type, long size)
        {
            Type = type;
            Size = size;
        }

        [JsonProperty("kind")]
        public override string Kind => "ConstantSizedArray";

        [JsonProperty("type")]
        public CadenceTypeBase Type { get; set; }

        [JsonProperty("size")]
        public long Size { get; set; }
    }
}
