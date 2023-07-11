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

        /// <summary>
        /// Gets a string representation of this cadence value. Useful for debugging. 
        /// </summary>
        /// <returns>A string representation of this cadence value.</returns>
        public virtual string GetValue()
        {
            throw new System.NotImplementedException($"GetValue() not implemented for {Type}");
        }
    }
}
