/*
MIT License

Copyright (c) 2021 Tyron Brand

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using DapperLabs.Flow.Sdk.Exceptions;
using System.Linq;

namespace DapperLabs.Flow.Sdk.Cadence
{
    /// <summary>
    /// Provides support for the Cadence Composite type.  This type is used for Structs, Resources, Events, Contracts and Enums
    /// A CadenceComposite contains the data of a Composite structure as a series of cascading name/value pairs.
    /// https://developers.flow.com/cadence/json-cadence-spec#composite-types
    /// </summary>
    public class CadenceComposite : CadenceBase
    {
        [JsonProperty("type")]
        public sealed override string Type { get; set; }

        [JsonProperty("value")]
        public CadenceCompositeValue Value { get; set; }

        readonly string[] validTypes =
        {
            "Struct",
            "Resource",
            "Event",
            "Contract",
            "Enum"
        };

        /// <summary>
        /// Constructs a CadenceComposite of the given type
        /// </summary>
        /// <param name="type">
        /// The string representation of the desired composite type.  Must be "Struct", "Resource", "Event", "Contract", or "Enum"
        /// </param>
        /// <exception cref="FlowException">A FlowException is thrown if the type string isn't a valid Composite type</exception>
        public CadenceComposite(string type) 
        {
            if (Array.IndexOf(validTypes, type) == -1)
            {
                throw new FlowException($"Invalid type for CadenceComposite: {type}");
            }

            Type = type;
        }

        /// <summary>
        /// Constructs a CadenceComposite of the given type with the given value
        /// </summary>
        /// <param name="type">The string representation of the desired composite type</param>
        /// <param name="value">The CadenceCompositeValue represented by this CadenceComposite</param>
        /// <exception cref="FlowException"></exception>
        public CadenceComposite(string type, CadenceCompositeValue value)
        {
            if (Array.IndexOf(validTypes, type) == -1)
            {
                throw new FlowException($"Invalid type for CadenceComposite: {type}");
            }

            Type = type;
            Value = value;
        }

        /// <summary>
        /// Casts the requested field of a CadenceComposite to the desired Cadence type and returns it
        /// </summary>
        /// <param name="fieldName">The name of the field to cast</param>
        /// <typeparam name="T">The Cadence type the field should be cast to</typeparam>
        /// <returns>A value of the requested Cadence type</returns>
        /// <exception cref="FlowException">A FlowException will be thrown if the requested field does not exist</exception>
        public T CompositeFieldAs<T>(string fieldName) where T : CadenceBase
        {
            CadenceBase value = Value.Fields.Where(w => w.Name == fieldName).Select(s => s.Value).FirstOrDefault();

            if (value == null)
            {
                throw new FlowException($"Failed to find fieldName in CadenceComposite: {fieldName}");
            }

            return value.As<T>();
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
    /// Provides a container to hold one or more CadenceCompositeFields that hold the data in a CadenceComposite
    /// </summary>
    public class CadenceCompositeValue
    {
        [JsonProperty("id")]
        public string Type { get; set; }

        [JsonProperty("fields")]
        public IEnumerable<CadenceCompositeField> Fields { get; set; }
    }

    /// <summary>
    /// Provides storage for the name and value of a CadenceCompositeField
    /// </summary>
    public class CadenceCompositeField
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public CadenceBase Value { get; set; }
    }
}
