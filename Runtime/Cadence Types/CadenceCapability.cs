using Newtonsoft.Json;

namespace DapperLabs.Flow.Sdk.Cadence
{
    /// <summary>
    /// Provides support for the Cadence Capability type
    /// </summary>
    public class CadenceCapability : CadenceBase
    {
        [JsonProperty("type")]
        public override string Type => "Capability";

        [JsonProperty("value")]
        public CadenceCapabilityValue Value { get; set; }

        /// <summary>
        /// Constructs an empty CadenceCapability
        /// </summary>
        public CadenceCapability() { }

        /// <summary>
        /// Constructs a CadenceCapabililty from the given value
        /// </summary>
        /// <param name="value">A CadenceCapabilityValue</param>
        public CadenceCapability(CadenceCapabilityValue value)
        {
            Value = value;
        }

        public override string GetValue()
        {
            return JsonConvert.SerializeObject(Value);
        }
    }

    /// <summary>
    /// Represents the value that a CadenceCapability can have
    /// </summary>
    public class CadenceCapabilityValue
    {
        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("borrowType")]
        public string BorrowType { get; set; }
    }
}
