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

        /// <summary>
        /// Gets a string representation of this cadence value. Useful for debugging. 
        /// </summary>
        /// <returns>A string representation of this cadence value.</returns>
        public override string GetValue()
        {
            string ret = "[";
            for (int i = 0; i < Value.Length; i++)
            {
                ret += JsonConvert.SerializeObject(Value[i]);
                if (i < Value.Length - 1)
                {
                    ret += ", ";
                }
            }
            ret += "]";

            return ret;
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
