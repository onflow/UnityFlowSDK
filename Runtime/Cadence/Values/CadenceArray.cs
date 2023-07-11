using Newtonsoft.Json;

namespace DapperLabs.Flow.Sdk.Cadence
{
    /// <summary>
    /// Provides support for the Cadence Array type
    /// </summary>
    public class CadenceArray : CadenceBase
    {
        [JsonProperty("type")]
        public override string Type => "Array";

        [JsonProperty("value")]
        public CadenceBase[] Value { get; set; }

        /// <summary>
        /// Constructs an empty CadenceArray
        /// </summary>
        public CadenceArray() {}

        /// <summary>
        /// Constructs a CadenceArray from an array of CadenceBase values
        /// </summary>
        /// <param name="value">An array of CadenceBase values</param>
        public CadenceArray(CadenceBase[] value)
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
                ret += Value[i].GetValue();
                if (i < Value.Length - 1)
                {
                    ret += ", ";
                }
            }
            ret += "]";

            return ret;
        }
    }
}
