using Newtonsoft.Json;

namespace DapperLabs.Flow.Sdk.Cadence.Types
{
    /// <summary>
    /// Represents the cadence static type for a CadenceField
    /// </summary>
    public class CadenceFieldType
    {
        /// <summary>
        /// Constructs an empty CadenceFieldType
        /// </summary>
        public CadenceFieldType() { }

        /// <summary>
        /// Constructs a CadenceFieldType from the given parameters
        /// </summary>
        /// <param name="id">Name of the field</param>
        /// <param name="type">Cadence type of the field</param>
        public CadenceFieldType(string id, CadenceTypeBase type)
        {
            Id = id;
            Type = type;
        }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public CadenceTypeBase Type { get; set; }
    }
}
