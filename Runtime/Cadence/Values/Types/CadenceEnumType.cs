using Newtonsoft.Json;
using System.Collections.Generic;

namespace DapperLabs.Flow.Sdk.Cadence.Types
{
    /// <summary>
    /// Represents the cadence static type for a CadenceEnum
    /// </summary>
    public class CadenceEnumType : CadenceTypeBase
    {
        /// <summary>
        /// Constructs an empty CadenceEnumType
        /// </summary>
        public CadenceEnumType() { }

        /// <summary>
        /// Constructs a CadenceEnumType from the given parameters
        /// </summary>
        /// <param name="typeId">Fully qualified type id</param>
        /// <param name="type">The cadence type of the enum</param>
        /// <param name="fields">A list of CadenceFieldType</param>
        public CadenceEnumType(string typeId, CadenceTypeBase type, IList<CadenceFieldType> fields)
        {
            TypeId = typeId;
            Type = type;
            Fields = fields;
        }

        [JsonProperty("kind")]
        public override string Kind => "Enum";

        [JsonProperty("type")]
        public CadenceTypeBase Type { get; set; }

        [JsonProperty("typeID")]
        public string TypeId { get; set; }

        [JsonProperty("initializers")]
        public IList<CadenceInitializerType> Initializers { get; } = new List<CadenceInitializerType>();

        [JsonProperty("fields")]
        public IList<CadenceFieldType> Fields { get; set; } = new List<CadenceFieldType>();
    }
}
