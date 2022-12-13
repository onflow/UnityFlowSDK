using Newtonsoft.Json;

namespace DapperLabs.Flow.Sdk.Cadence
{
    /// <summary>
    /// Provides support for the Cadence Dictionary type
    /// </summary>
    public class CadenceDictionary : CadenceBase
    {
        [JsonProperty("type")]
        public override string Type => "Dictionary";

        [JsonProperty("value")]
        public CadenceDictionaryItem[] Value { get; set; }

        /// <summary>
        /// Constructs an empty CadenceDictionary
        /// </summary>
        public CadenceDictionary() { }

        /// <summary>
        /// Constructs a CadenceDictionary from the passed items
        /// </summary>
        /// <param name="value">An array of CadenceDictionaryItems</param>
        public CadenceDictionary(CadenceDictionaryItem[] value)
        {
            Value = value;
        }
    }

    /// <summary>
    /// Represents a single item in a CadenceDictionary
    /// </summary>
    public class CadenceDictionaryItem
    {
        [JsonProperty("key")]
        public CadenceBase Key { get; set; }

        [JsonProperty("value")]
        public CadenceBase Value { get; set; }
    }
}
