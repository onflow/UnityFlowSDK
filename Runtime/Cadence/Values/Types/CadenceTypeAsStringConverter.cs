using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DapperLabs.Flow.Sdk.Cadence.Types
{
    internal class CadenceTypeAsStringConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(CadenceTypeAsString);
        }

        public override bool CanRead => false;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {            
            writer.WriteValue(((CadenceTypeAsString)value).Value);
        }
    }
}
