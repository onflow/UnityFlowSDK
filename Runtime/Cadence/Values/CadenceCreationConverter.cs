/*
MIT License

Copyright (c) 2021 Tyron Brand

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using DapperLabs.Flow.Sdk.Exceptions;

namespace DapperLabs.Flow.Sdk.Cadence
{
    internal class CadenceCreationConverter : CustomCreationConverter<CadenceBase>
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);
            CadenceBase target = Create(jObject);

            switch (target.Type)
            {
                case "Optional":
                    {
                        var value = jObject.Property("value");
                        if (value.Value.Type == JTokenType.Null)
                        {
                            return new CadenceOptional();
                        }
                    }
                    break;
            }

            serializer.Populate(jObject.CreateReader(), target);

            return target;
        }

        private static CadenceBase Create(JObject jObject)
        {
            string type = (string)jObject.Property("type");

            switch (type)
            {
                case "String":
                    return new CadenceString();
                case "Array":
                    return new CadenceArray();
                case "Bool":
                    return new CadenceBool();
                case "Address":
                    return new CadenceAddress();
                case "Void":
                    return new CadenceVoid();
                case "Dictionary":
                    return new CadenceDictionary();
                case "Capability":
                    return new CadenceCapability();
                case "Type":
                    return new CadenceType();
                case "Optional":
                    return new CadenceOptional();
                case "Path":
                    return new CadencePath();
                case "Link":
                    return new CadenceLink();
                case "Struct":
                case "Resource":
                case "Event":
                case "Contract":
                case "Enum":
                    return new CadenceComposite(type);
                case "Int":
                case "UInt":
                case "Int8":
                case "UInt8":
                case "Int16":
                case "UInt16":
                case "Int32":
                case "UInt32":
                case "Int64":
                case "UInt64":
                case "Int128":
                case "UInt128":
                case "Int256":
                case "UInt256":
                case "Word8":
                case "Word16":
                case "Word32":
                case "Word64":
                case "Fix64":
                case "UFix64":
                    return new CadenceNumber((CadenceNumberType)Enum.Parse(typeof(CadenceNumberType), type));
            }

            throw new FlowException($"The type {type} is not supported!");
        }

        public override CadenceBase Create(Type objectType)
        {
            throw new NotImplementedException();
        }
    }
}
