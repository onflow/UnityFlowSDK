using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace DapperLabs.Flow.Sdk.Cadence.Types
{
    internal class CadenceTypeConverter : CustomCreationConverter<CadenceTypeBase>
    {
        private static readonly Dictionary<string, CadenceTypeBase> _compositeDictionary = new Dictionary<string, CadenceTypeBase>();

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            try
            {
                if ((reader.Path.Contains("type") || reader.Path == "value.staticType") && reader.TokenType == JsonToken.String)
                {
                    if (_compositeDictionary.TryGetValue((string)reader.Value, out var composite))
                        return composite;

                    return new CadenceTypeAsString(reader.Value.ToString());
                }                    

                var jObject = JObject.Load(reader);
                var target = Create(jObject);
                HandleCadenceCompositeType(jObject, target);
                serializer.Populate(jObject.CreateReader(), target);
                return target;
            }
            catch(Exception ex)
            {
                var t = ex;
                throw new Exception("", ex);
            }                       
        }

        private static CadenceTypeBase Create(JObject jObject)
        {
            var kind = (string)jObject.Property("kind");

            switch (kind)
            {
                case "VariableSizedArray":
                    return new CadenceVariableSizedArrayType();
                case "ConstantSizedArray":
                    return new CadenceConstantSizedArrayType();
                case "Dictionary":
                    return new CadenceDictionaryType();
                case "Struct":
                case "Resource":
                case "Event":
                case "Contract":
                case "StructInterface":
                case "ResourceInterface":
                case "ContractInterface":
                    return new CadenceCompositeType((CadenceCompositeTypeKind)Enum.Parse(typeof(CadenceCompositeTypeKind), kind));
                case "Function":
                    return new CadenceFunctionType();
                case "Reference":
                    return new CadenceReferenceType();
                case "Restriction":
                    return new CadenceRestrictedType();
                case "Capability":
                    return new CadenceCapabilityType();
                case "Enum":
                    return new CadenceEnumType();
                case "Optional":
                    return new CadenceOptionalType();
                default:
                    return new CadenceTypeBase();
            }
        }

        private static void HandleCadenceCompositeType(JObject jObject, CadenceTypeBase cadenceType)
        {
            switch (cadenceType)
            {
                case CadenceEnumType enumType:
                    enumType.TypeId = (string)jObject.Property("typeID");
                    _compositeDictionary[enumType.TypeId] = enumType;
                    break;
                case CadenceCompositeType compositeType:
                    compositeType.TypeId = (string)jObject.Property("typeID");
                    _compositeDictionary[compositeType.TypeId] = compositeType;
                    break;
            }
        }

        public override CadenceTypeBase Create(Type objectType) => throw new NotImplementedException();
    }
}
