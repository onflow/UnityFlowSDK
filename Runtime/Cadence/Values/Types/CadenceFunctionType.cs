using Newtonsoft.Json;
using System.Collections.Generic;

namespace DapperLabs.Flow.Sdk.Cadence.Types
{
    /// <summary>
    /// Represents the cadence static type for a CadenceFunction
    /// </summary>
    public class CadenceFunctionType : CadenceTypeBase
    {
        /// <summary>
        /// Constructs an empty CadenceFunctionType
        /// </summary>
        public CadenceFunctionType() { }

        /// <summary>
        /// Constructs a CadenceFunctionType from the given parameters
        /// </summary>
        /// <param name="typeId">Name of the function</param>
        /// <param name="parameters">A list of CadenceParameterType</param>
        /// <param name="return">Cadence type of the return value</param>
        public CadenceFunctionType(string typeId, IList<CadenceParameterType> parameters, CadenceTypeBase @return)
        {
            TypeId = typeId;
            Parameters = parameters;
            Return = @return;
        }

        [JsonProperty("kind")]
        public override string Kind => "Function";

        [JsonProperty("typeID")]
        public string TypeId { get; set; }

        [JsonProperty("parameters")]
        public IList<CadenceParameterType> Parameters { get; set; }

        [JsonProperty("return")]
        public CadenceTypeBase Return { get; set; }
    }
}
