using Newtonsoft.Json;

namespace DapperLabs.Flow.Sdk.Cadence.Types
{
    /// <summary>
    /// Represents the cadence static type for a CadenceDictionary
    /// </summary>
    public class CadenceDictionaryType : CadenceTypeBase
    {
        /// <summary>
        /// Constructs an empty CadenceDictionaryType
        /// </summary>
        public CadenceDictionaryType() { }

        /// <summary>
        /// Constructs a CadenceDictionaryType from the given parameters
        /// </summary>
        /// <param name="key">The cadence type of the keys</param>
        /// <param name="value">The cadence type of the values</param>
        public CadenceDictionaryType(CadenceTypeBase key, CadenceTypeBase value)
        {
            Key = key;
            Value = value;
        }

        [JsonProperty("kind")]
        public override string Kind => "Dictionary";

        [JsonProperty("key")]
        public CadenceTypeBase Key { get; set; }

        [JsonProperty("value")]
        public CadenceTypeBase Value { get; set; }
    }
}
