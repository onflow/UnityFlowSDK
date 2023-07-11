using Newtonsoft.Json;

namespace DapperLabs.Flow.Sdk.Cadence
{
    /// <summary>
    /// Provides support for the Cadence Optional type
    /// </summary>
    public class CadenceOptional : CadenceBase
    {
        [JsonProperty("type")]
        public override string Type => "Optional";

        [JsonProperty("value")]
        public CadenceBase Value { get; set; }

        /// <summary>
        /// Constructs a CadenceOptional with a null value
        /// </summary>
        public CadenceOptional() 
        {
            Value = null;
        }

        /// <summary>
        /// Constructs a CadenceOptional with the given value
        /// </summary>
        /// <param name="value">A CadenceBase derived value to store</param>
        public CadenceOptional(CadenceBase value)
        {
            Value = value;
        }

        /// <summary>
        /// Gets a string representation of this cadence value. Useful for debugging. 
        /// </summary>
        /// <returns>A string representation of this cadence value.</returns>
        public override string GetValue()
        {
            if (Value == null)
            {
                return "null";
            }

            return Value.GetValue();
        }
    }
}
