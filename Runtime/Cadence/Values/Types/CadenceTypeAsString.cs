namespace DapperLabs.Flow.Sdk.Cadence.Types
{
    /// <summary>
    /// Represents a cadence type as a string
    /// </summary>
    public class CadenceTypeAsString : CadenceTypeBase
    {
        /// <summary>
        /// Constructs an empty CadenceTypeAsString
        /// </summary>
        public CadenceTypeAsString() { }

        /// <summary>
        /// Constructs a CadenceTypeAsString from the given value
        /// </summary>
        /// <param name="value">The string value</param>
        public CadenceTypeAsString(string value)
        {
            Value = value;
        }

        public override string Kind => "String Value";

        public string Value { get; set; }
    }
}
