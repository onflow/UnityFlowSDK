using Newtonsoft.Json;
using System.Collections.Generic;

namespace DapperLabs.Flow.Sdk.Cadence.Types
{
    /// <summary>
    /// Represents the cadence static types Struct, Resource, Event, Contract,
    /// StructInterface, ResourceInterface, ContractInterface
    /// </summary>
    public class CadenceCompositeType : CadenceTypeBase
    {
        /// <summary>
        /// Constructs a CadenceCompositeType from the given kind
        /// </summary>
        /// <param name="kind">A CadenceCompositeTypeKind</param>
        public CadenceCompositeType(CadenceCompositeTypeKind kind)
        {
            Kind = kind.ToString();
        }

        /// <summary>
        /// Constructs a CadenceCompositeType from the given parameters
        /// </summary>
        /// <param name="kind">A CadenceCompositeTypeKind</param>
        /// <param name="typeId">Fully qualified type id</param>
        /// <param name="initializers">A list containing lists of CadenceInitializerType</param>
        /// <param name="fields">A list of CadenceFieldType</param>
        public CadenceCompositeType(CadenceCompositeTypeKind kind, string typeId, IList<IList<CadenceInitializerType>> initializers, IList<CadenceFieldType> fields)
        {
            Kind = kind.ToString();
            TypeId = typeId;
            Initializers = initializers;
            Fields = fields;
        }

        [JsonProperty("kind")]
        public sealed override string Kind { get; set; }

        [JsonProperty("type")]
        public string Type { get; } = "";

        [JsonProperty("typeID")]
        public string TypeId { get; set; }

        [JsonProperty("initializers")]
        public IList<IList<CadenceInitializerType>> Initializers { get; set; } = new List<IList<CadenceInitializerType>>();

        [JsonProperty("fields")]
        public IList<CadenceFieldType> Fields { get; set; } = new List<CadenceFieldType>();

    }

    /// <summary>
    /// Supported cadence composite kinds
    /// </summary>
    public enum CadenceCompositeTypeKind
    {
        Struct,
        Resource,
        Event,
        Contract,
        StructInterface,
        ResourceInterface,
        ContractInterface
    }
}
