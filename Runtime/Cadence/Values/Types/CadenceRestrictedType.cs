using Newtonsoft.Json;
using System.Collections.Generic;

namespace DapperLabs.Flow.Sdk.Cadence.Types
{
    /// <summary>
    /// Represents the cadence static type for a CadenceRestricted
    /// </summary>
    public class CadenceRestrictedType : CadenceTypeBase
    {
        /// <summary>
        /// Constructs an empty CadenceRestrictedType
        /// </summary>
        public CadenceRestrictedType() { }

        /// <summary>
        /// Constructs a CadenceRestrictedType from the given parameters
        /// </summary>
        /// <param name="typeId">Fully qualified type id</param>
        /// <param name="type">Cadence type of the restriction</param>
        /// <param name="restrictions">A list of cadence types</param>
        public CadenceRestrictedType(string typeId, CadenceTypeBase type, IList<CadenceTypeBase> restrictions)
        {
            TypeId = typeId;
            Type = type;
            Restrictions = restrictions;
        }

        [JsonProperty("kind")]
        public override string Kind => "Restriction";

        [JsonProperty("typeID")]
        public string TypeId { get; set; }

        [JsonProperty("type")]
        public CadenceTypeBase Type { get; set; }

        [JsonProperty("restrictions")]
        public IList<CadenceTypeBase> Restrictions { get; set; }
    }
}
