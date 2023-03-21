using Newtonsoft.Json;
using DapperLabs.Flow.Sdk.Exceptions;
using System;

namespace DapperLabs.Flow.Sdk.Cadence
{
    /// <summary>
    /// Provides support for the Cadence Path type
    /// </summary>
    public class CadencePath : CadenceBase
    {
        [JsonProperty("type")]
        public override string Type => "Path";

        [JsonProperty("value")]
        public CadencePathValue Value { get; set; }

        /// <summary>
        /// Constructs an empty CadencePath
        /// </summary>
        public CadencePath() { }

        /// <summary>
        /// Constructs a CadencePath from the given value
        /// </summary>
        /// <param name="value">A CadencePathValue</param>
        public CadencePath(CadencePathValue value)
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
    /// Represents a path in a Flow account
    /// </summary>
    public class CadencePathValue
    {
        [JsonProperty("domain")]
        public string Domain { get; set; }

        [JsonProperty("identifier")]
        public string Identifier { get; set; }

        /// <summary>
        /// Array of the valid values for path domains
        /// </summary>
        readonly string[] validDomains =
        {
            "storage",
            "private",
            "public"
        };

        /// <summary>
        /// Constructs an empty CadencePathValue
        /// </summary>
        public CadencePathValue() { }

        /// <summary>
        /// Constructs a CadencePathValue from the given domain and identifier
        /// </summary>
        /// <param name="domain">The domain of the path.  Must be "storage", "private", or "public"</param>
        /// <param name="identifier">A string identifier for this path</param>
        /// <exception cref="FlowException">Throws a FlowException if an invalid domain is given.</exception>
        public CadencePathValue(string domain, string identifier)
        {
            if (Array.IndexOf(validDomains, domain) == -1)
            {
                throw new FlowException("Invalid domain for CadencePath.");
            }

            Domain = domain;
            Identifier = identifier;
        }
    }
}
