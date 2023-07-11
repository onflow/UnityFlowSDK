using Newtonsoft.Json;

namespace DapperLabs.Flow.Sdk.Cadence.Types
{
    /// <summary>
    /// Represents the cadence static type for a CadenceParameter
    /// </summary>
    public class CadenceParameterType
    {
        /// <summary>
        /// Constructs an empty CadenceParameterType
        /// </summary>
        public CadenceParameterType() { }

        /// <summary>
        /// Constructs a CadenceParameterType from the given parameters
        /// </summary>
        /// <param name="label">A label</param>
        /// <param name="id">An id</param>
        /// <param name="type">Cadence type of the parameter</param>
        public CadenceParameterType(string label, string id, CadenceTypeBase type)
        {
            Label = label;
            Id = id;
            Type = type;
        }

        [JsonProperty("label")]
        public string Label { get; set; }
        
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public CadenceTypeBase Type { get; set; }
    }
}
