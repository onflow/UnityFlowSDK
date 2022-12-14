using Newtonsoft.Json;

namespace DapperLabs.Flow.Sdk.Cadence
{
    /// <summary>
    /// Base class for all Cadence types
    /// </summary>
    public class CadenceBase
    {
        [JsonProperty("type")]
        public virtual string Type { get; set; }

        /// <summary>
        /// Converts a Cadence type into a type derived from CadenceBase
        /// </summary>
        /// <typeparam name="T">The Cadence Type the value should be cast to</typeparam>
        /// <returns>A Cadence value of the requested type</returns>
        public T As<T>()
            where T : CadenceBase
        {
            return (T)this;
        }
    }
}
