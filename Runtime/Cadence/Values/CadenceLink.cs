using Newtonsoft.Json;

namespace DapperLabs.Flow.Sdk.Cadence
{
    /// <summary>
    /// Provides support for the Cadence Link type
    /// </summary>
    public class CadenceLink : CadenceBase
    {
        [JsonProperty("type")]
        public override string Type => "Link";

        [JsonProperty("value")]
        public CadenceLinkValue Value { get; set; }

        /// <summary>
        /// Constructs an empty CadenceLink
        /// </summary>
        public CadenceLink() { }

        /// <summary>
        /// Constructs a CadenceLink containing the passed value
        /// </summary>
        /// <param name="value">The CadenceLinkValue that the CadenceLink should represent</param>
        public CadenceLink(CadenceLinkValue value)
        {
            Value = value;
        }

        /// <summary>
        /// Gets a string representation of this cadence value. Useful for debugging. 
        /// </summary>
        /// <returns>A string representation of this cadence value.</returns>
        public override string GetValue()
        {
            return JsonConvert.SerializeObject(Value);
        }
    }

    /// <summary>
    /// Represents the value that a CadenceLink can have
    /// </summary>
    public class CadenceLinkValue
    {
        [JsonProperty("targetPath")]
        public CadencePath TargetPath { get; set; }

        [JsonProperty("borrowType")]
        public string BorrowType { get; set; }
    }
}
