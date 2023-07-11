using Newtonsoft.Json;

namespace DapperLabs.Flow.Sdk.Cadence
{
    /// <summary>
    /// Provides support for the Cadence numeric types
    /// </summary>
    public class CadenceNumber : CadenceBase
    {
        [JsonProperty("type")]
        public sealed override string Type { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        /// <summary>
        /// Constructs an empty CadenceNumber of the given CadenceNumberType 
        /// </summary>
        /// <param name="type"></param>
        public CadenceNumber(CadenceNumberType type)
        {
            Type = type.ToString();
        }

        /// <summary>
        /// Constructs a CadenceNumber of the given CadenceNumberType with the given value
        /// </summary>
        /// <param name="type">A CadenceNumberType</param>
        /// <param name="value">The string representation of the desired number</param>
        public CadenceNumber(CadenceNumberType type, string value)
        {
            Type = type.ToString();
            Value = value;
        }

        /// <summary>
        /// Gets a string representation of this cadence value. Useful for debugging. 
        /// </summary>
        /// <returns>A string representation of this cadence value.</returns>
        public override string GetValue()
        {
            return Value;
        }
    }

    /// <summary>
    /// Supported Cadence numeric types
    /// </summary>
    public enum CadenceNumberType
    {
        Int,
        UInt,
        Int8,
        UInt8,
        Int16,
        UInt16,
        Int32,
        UInt32,
        Int64,
        UInt64,
        Int128,
        UInt128,
        Int256,
        UInt256,
        Word8,
        Word16,
        Word32,
        Word64,
        Fix64,
        UFix64
    }
}
