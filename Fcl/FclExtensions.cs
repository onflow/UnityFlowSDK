using DapperLabs.Flow.Sdk.Cadence;
using DapperLabs.Flow.Sdk.Cadence.Types;
using Flow.Net.Sdk.Core.Cadence;
using Flow.Net.Sdk.Core.Cadence.Types;
using System.Collections;
using System.Collections.Generic;

namespace DapperLabs.Flow.Sdk.Fcl
{
    internal static class FclExtensions
    {
        internal static IList<ICadence> ToFclCadenceList(this List<CadenceBase> src)
        {
            List<ICadence> ret = new List<ICadence>();

            for (int i = 0; i < src.Count; i++)
            {
                ret.Add(src[i].ToFclCadence());
            }

            return ret;
        }

        internal static ICadence ToFclCadence(this CadenceBase src)
        {
            switch (src.Type)
            {
                case "Address": return ((Cadence.CadenceAddress)src).ToFclCadenceAddress();
                case "Array": return ((Cadence.CadenceArray)src).ToFclCadenceArray();
                case "Bool": return ((Cadence.CadenceBool)src).ToFclCadenceBool();
                case "Capability": return ((Cadence.CadenceCapability)src).ToFclCadenceCapability();
                case "Dictionary": return ((Cadence.CadenceDictionary)src).ToFclCadenceDictionary();
                case "Link": return ((Cadence.CadenceLink)src).ToFclCadenceLink();
                case "Optional": return ((Cadence.CadenceOptional)src).ToFclCadenceOptional();
                case "Path": return ((Cadence.CadencePath)src).ToFclCadencePath();
                case "String": return ((Cadence.CadenceString)src).ToFclCadenceString();
                case "Character": throw new System.Exception($"Fcl type converter: unknown type {src.Type}.");  // TODO: Implement Char
                case "Type": return ((Cadence.CadenceType)src).ToFclCadenceType();
                case "Void": return ((Cadence.CadenceVoid)src).ToFclCadenceVoid();
                case "Struct":
                case "Resource":
                case "Event":
                case "Contract":
                case "Enum":
                    return ((Cadence.CadenceComposite)src).ToFclCadenceComposite();
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
                    return ((Cadence.CadenceNumber)src).ToFclCadenceNumber();
                default:
                    throw new System.Exception($"Fcl type converter: unknown type {src.Type}.");
            }
        }

        internal static global::Flow.Net.Sdk.Core.Cadence.CadenceAddress ToFclCadenceAddress(this Cadence.CadenceAddress src)
        {
            return new global::Flow.Net.Sdk.Core.Cadence.CadenceAddress(src.Value);
        }

        internal static global::Flow.Net.Sdk.Core.Cadence.CadenceArray ToFclCadenceArray(this Cadence.CadenceArray src)
        {
            var values = new List<ICadence>();
            
            for (int i = 0; i < src.Value.Length; i++)
            {
                values.Add(src.Value[i].ToFclCadence());
            }

            return new global::Flow.Net.Sdk.Core.Cadence.CadenceArray(values);
        }

        internal static global::Flow.Net.Sdk.Core.Cadence.CadenceBool ToFclCadenceBool(this Cadence.CadenceBool src)
        {
            return new global::Flow.Net.Sdk.Core.Cadence.CadenceBool(src.Value);
        }

        internal static global::Flow.Net.Sdk.Core.Cadence.CadenceCapability ToFclCadenceCapability(this Cadence.CadenceCapability src)
        {
            return new global::Flow.Net.Sdk.Core.Cadence.CadenceCapability(src.Value.ToFclCapabilityValue());
        }

        internal static FlowCapabilityValue ToFclCapabilityValue(this CadenceCapabilityValue src)
        {
            return new FlowCapabilityValue
            {
                Address = src.Address,
                BorrowType = src.BorrowType.ToFclCadenceType(),
                Path = src.Path.ToFclCadencePath()
            };
        }

        internal static global::Flow.Net.Sdk.Core.Cadence.CadencePath ToFclCadencePath(this Cadence.CadencePath src)
        {
            return new global::Flow.Net.Sdk.Core.Cadence.CadencePath(src.Value.ToFclCadencePathValue());
        }

        internal static global::Flow.Net.Sdk.Core.Cadence.CadencePathValue ToFclCadencePathValue(this Cadence.CadencePathValue src)
        {
            return new global::Flow.Net.Sdk.Core.Cadence.CadencePathValue
            {
                Domain = src.Domain,
                Identifier = src.Identifier
            };
        }

        internal static ICadenceType ToFclCadenceType(this CadenceTypeBase src)
        {
            switch (src.Kind)
            {
                case "Capability": return ((Cadence.Types.CadenceCapabilityType)src).ToFclCadenceCapabilityType();
                case "ConstantSizedArray": return ((Cadence.Types.CadenceConstantSizedArrayType)src).ToFclCadenceConstantSizedArrayType();
                case "Dictionary": return ((Cadence.Types.CadenceDictionaryType)src).ToFclCadenceDictionaryType();
                case "Enum": return ((Cadence.Types.CadenceEnumType)src).ToFclCadenceEnumType();
                case "Function": return ((Cadence.Types.CadenceFunctionType)src).ToFclCadenceFunctionType();
                case "Optional": return ((Cadence.Types.CadenceOptionalType)src).ToFclCadenceOptionalType();
                case "Reference": return ((Cadence.Types.CadenceReferenceType)src).ToFclCadenceReferenceType();
                case "Restriction": return ((Cadence.Types.CadenceRestrictedType)src).ToFclCadenceRestrictedType();
                case "VariableSizedArray": return ((Cadence.Types.CadenceVariableSizedArrayType)src).ToFclCadenceVariableSizedArrayType();
                case "Struct":
                case "Resource":
                case "Event":
                case "Contract":
                case "StructInterface":
                case "ResourceInterface":
                case "ContractInterface":
                    return ((Cadence.Types.CadenceCompositeType)src).ToFclCadenceCompositeType();
            }

            return null;
        }

        internal static global::Flow.Net.Sdk.Core.Cadence.CadenceComposite ToFclCadenceComposite(this Cadence.CadenceComposite src)
        {
            global::Flow.Net.Sdk.Core.Cadence.CadenceCompositeType type = global::Flow.Net.Sdk.Core.Cadence.CadenceCompositeType.Struct;
            switch (src.Type)
            {
                case "Struct": type = global::Flow.Net.Sdk.Core.Cadence.CadenceCompositeType.Struct; break;
                case "Resource": type = global::Flow.Net.Sdk.Core.Cadence.CadenceCompositeType.Resource; break;
                case "Event": type = global::Flow.Net.Sdk.Core.Cadence.CadenceCompositeType.Event; break;
                case "Contract": type = global::Flow.Net.Sdk.Core.Cadence.CadenceCompositeType.Contract; break;
                case "Enum": type = global::Flow.Net.Sdk.Core.Cadence.CadenceCompositeType.Enum; break;
            }

            return new global::Flow.Net.Sdk.Core.Cadence.CadenceComposite(type, src.Value.ToFclCadenceCompositeItem());
        }

        internal static CadenceCompositeItem ToFclCadenceCompositeItem(this CadenceCompositeValue src)
        {
            return new CadenceCompositeItem
            {
                Id = src.Type,
                Fields = src.Fields.ToFclCadenceCompositeItemValues()
            };
        }

        internal static IEnumerable<CadenceCompositeItemValue> ToFclCadenceCompositeItemValues(this IEnumerable<CadenceCompositeField> src)
        {
            var ret = new List<CadenceCompositeItemValue>();

            foreach (CadenceCompositeField field in src)
            {
                ret.Add(new CadenceCompositeItemValue
                {
                    Name = field.Name,
                    Value = field.Value.ToFclCadence()
                });
            }

            return ret;
        }

        internal static global::Flow.Net.Sdk.Core.Cadence.CadenceDictionary ToFclCadenceDictionary(this Cadence.CadenceDictionary src)
        {
            var values = new List<CadenceDictionaryKeyValue>();

            for (int i = 0; i < src.Value.Length; i++)
            {
                values.Add(new CadenceDictionaryKeyValue
                {
                    Key = src.Value[i].Key.ToFclCadence(),
                    Value = src.Value[i].Value.ToFclCadence()
                });
            }

            return new global::Flow.Net.Sdk.Core.Cadence.CadenceDictionary(values);
        }

        internal static global::Flow.Net.Sdk.Core.Cadence.CadenceLink ToFclCadenceLink(this Cadence.CadenceLink src)
        {
            return new global::Flow.Net.Sdk.Core.Cadence.CadenceLink(src.Value.ToFclCadenceLinkValue());
        }

        internal static global::Flow.Net.Sdk.Core.Cadence.CadenceLinkValue ToFclCadenceLinkValue(this Cadence.CadenceLinkValue src)
        {
            return new global::Flow.Net.Sdk.Core.Cadence.CadenceLinkValue
            {
                BorrowType = src.BorrowType,
                TargetPath = src.TargetPath.ToFclCadencePath()
            };
        }

        internal static global::Flow.Net.Sdk.Core.Cadence.CadenceNumber ToFclCadenceNumber(this Cadence.CadenceNumber src)
        {
            global::Flow.Net.Sdk.Core.Cadence.CadenceNumberType type = global::Flow.Net.Sdk.Core.Cadence.CadenceNumberType.Int;
            switch (src.Type)
            {
                case "Int": type = global::Flow.Net.Sdk.Core.Cadence.CadenceNumberType.Int; break;
                case "UInt": type = global::Flow.Net.Sdk.Core.Cadence.CadenceNumberType.UInt; break;
                case "Int8": type = global::Flow.Net.Sdk.Core.Cadence.CadenceNumberType.Int8; break;
                case "UInt8": type = global::Flow.Net.Sdk.Core.Cadence.CadenceNumberType.UInt8; break;
                case "Int16": type = global::Flow.Net.Sdk.Core.Cadence.CadenceNumberType.Int16; break;
                case "UInt16": type = global::Flow.Net.Sdk.Core.Cadence.CadenceNumberType.UInt16; break;
                case "Int32": type = global::Flow.Net.Sdk.Core.Cadence.CadenceNumberType.Int32; break;
                case "UInt32": type = global::Flow.Net.Sdk.Core.Cadence.CadenceNumberType.UInt32; break;
                case "Int64": type = global::Flow.Net.Sdk.Core.Cadence.CadenceNumberType.Int64; break;
                case "UInt64": type = global::Flow.Net.Sdk.Core.Cadence.CadenceNumberType.UInt64; break;
                case "Int128": type = global::Flow.Net.Sdk.Core.Cadence.CadenceNumberType.Int128; break;
                case "UInt128": type = global::Flow.Net.Sdk.Core.Cadence.CadenceNumberType.UInt128; break;
                case "Int256": type = global::Flow.Net.Sdk.Core.Cadence.CadenceNumberType.Int256; break;
                case "UInt256": type = global::Flow.Net.Sdk.Core.Cadence.CadenceNumberType.UInt256; break;
                case "Word8": type = global::Flow.Net.Sdk.Core.Cadence.CadenceNumberType.Word8; break;
                case "Word16": type = global::Flow.Net.Sdk.Core.Cadence.CadenceNumberType.Word16; break;
                case "Word32": type = global::Flow.Net.Sdk.Core.Cadence.CadenceNumberType.Word32; break;
                case "Word64": type = global::Flow.Net.Sdk.Core.Cadence.CadenceNumberType.Word64; break;
                case "Fix64": type = global::Flow.Net.Sdk.Core.Cadence.CadenceNumberType.Fix64; break;
                case "UFix64": type = global::Flow.Net.Sdk.Core.Cadence.CadenceNumberType.UFix64; break;
            }

            return new global::Flow.Net.Sdk.Core.Cadence.CadenceNumber(type, src.Value);
        }

        internal static global::Flow.Net.Sdk.Core.Cadence.CadenceOptional ToFclCadenceOptional(this Cadence.CadenceOptional src)
        {
            return new global::Flow.Net.Sdk.Core.Cadence.CadenceOptional(src.Value == null ? null : src.Value.ToFclCadence());
        }

        internal static global::Flow.Net.Sdk.Core.Cadence.CadenceString ToFclCadenceString(this Cadence.CadenceString src)
        {
            return new global::Flow.Net.Sdk.Core.Cadence.CadenceString(src.Value);
        }

        internal static global::Flow.Net.Sdk.Core.Cadence.CadenceTypeValue ToFclCadenceType(this Cadence.CadenceType src)
        {
            return new global::Flow.Net.Sdk.Core.Cadence.CadenceTypeValue(new CadenceTypeValueValue
            {
                StaticType = src.Value.StaticType.ToFclCadenceType()
            });
        }

        internal static global::Flow.Net.Sdk.Core.Cadence.CadenceVoid ToFclCadenceVoid(this Cadence.CadenceVoid src)
        {
            return new global::Flow.Net.Sdk.Core.Cadence.CadenceVoid();
        }

        internal static global::Flow.Net.Sdk.Core.Cadence.Types.CadenceCapabilityType ToFclCadenceCapabilityType(this Cadence.Types.CadenceCapabilityType src)
        {
            return new global::Flow.Net.Sdk.Core.Cadence.Types.CadenceCapabilityType(src.Type.ToFclCadenceType());
        }

        internal static global::Flow.Net.Sdk.Core.Cadence.Types.CadenceCompositeType ToFclCadenceCompositeType(this Cadence.Types.CadenceCompositeType src)
        {
            var kind = global::Flow.Net.Sdk.Core.Cadence.Types.CadenceCompositeTypeKind.Struct;

            switch (src.Kind)
            {
                case "Struct": kind = global::Flow.Net.Sdk.Core.Cadence.Types.CadenceCompositeTypeKind.Struct; break;
                case "Resource": kind = global::Flow.Net.Sdk.Core.Cadence.Types.CadenceCompositeTypeKind.Resource; break;
                case "Event": kind = global::Flow.Net.Sdk.Core.Cadence.Types.CadenceCompositeTypeKind.Event; break;
                case "Contract": kind = global::Flow.Net.Sdk.Core.Cadence.Types.CadenceCompositeTypeKind.Contract; break;
                case "StructInterface": kind = global::Flow.Net.Sdk.Core.Cadence.Types.CadenceCompositeTypeKind.StructInterface; break;
                case "ResourceInterface": kind = global::Flow.Net.Sdk.Core.Cadence.Types.CadenceCompositeTypeKind.ResourceInterface; break;
                case "ContractInterface": kind = global::Flow.Net.Sdk.Core.Cadence.Types.CadenceCompositeTypeKind.ContractInterface; break;
            }

            return new global::Flow.Net.Sdk.Core.Cadence.Types.CadenceCompositeType(kind, src.TypeId, src.Initializers.ToFclCadenceInitializerTypes(), src.Fields.ToFclCadenceFieldTypes());
        }

        internal static IList<IList<global::Flow.Net.Sdk.Core.Cadence.Types.CadenceInitializerType>> ToFclCadenceInitializerTypes(this IList<IList<Cadence.Types.CadenceInitializerType>> src)
        {
            var ret = new List<IList<global::Flow.Net.Sdk.Core.Cadence.Types.CadenceInitializerType>>();

            for (int i = 0; i < src.Count; i++)
            {
                var newList = new List<global::Flow.Net.Sdk.Core.Cadence.Types.CadenceInitializerType>();

                for (int j = 0; i < src[i].Count; j++)
                {
                    newList.Add(new global::Flow.Net.Sdk.Core.Cadence.Types.CadenceInitializerType
                    {
                        Id = src[i][j].Id,
                        Label = src[i][j].Label,
                        Type = src[i][j].Type.ToFclCadenceType()
                    });
                }

                ret.Add(newList);
            }

            return ret;
        }

        internal static IList<global::Flow.Net.Sdk.Core.Cadence.Types.CadenceFieldType> ToFclCadenceFieldTypes(this IList<Cadence.Types.CadenceFieldType> src)
        {
            var ret = new List<global::Flow.Net.Sdk.Core.Cadence.Types.CadenceFieldType>();

            for (int i = 0; i < src.Count; i++)
            {
                ret.Add(new global::Flow.Net.Sdk.Core.Cadence.Types.CadenceFieldType
                {
                    Id = src[i].Id,
                    Type = src[i].Type.ToFclCadenceType()
                });

            }

            return ret;
        }

        internal static global::Flow.Net.Sdk.Core.Cadence.Types.CadenceConstantSizedArrayType ToFclCadenceConstantSizedArrayType(this Cadence.Types.CadenceConstantSizedArrayType src)
        {
            return new global::Flow.Net.Sdk.Core.Cadence.Types.CadenceConstantSizedArrayType(src.Type.ToFclCadenceType(), src.Size);
        }

        internal static global::Flow.Net.Sdk.Core.Cadence.Types.CadenceDictionaryType ToFclCadenceDictionaryType(this Cadence.Types.CadenceDictionaryType src)
        {
            return new global::Flow.Net.Sdk.Core.Cadence.Types.CadenceDictionaryType(src.Key.ToFclCadenceType(), src.Value.ToFclCadenceType());
        }

        internal static global::Flow.Net.Sdk.Core.Cadence.Types.CadenceEnumType ToFclCadenceEnumType(this Cadence.Types.CadenceEnumType src)
        {
            return new global::Flow.Net.Sdk.Core.Cadence.Types.CadenceEnumType(src.TypeId, src.Type.ToFclCadenceType(), src.Fields.ToFclCadenceFieldTypes());
        }

        internal static global::Flow.Net.Sdk.Core.Cadence.Types.CadenceFunctionType ToFclCadenceFunctionType(this Cadence.Types.CadenceFunctionType src)
        {
            return new global::Flow.Net.Sdk.Core.Cadence.Types.CadenceFunctionType(src.TypeId, src.Parameters.ToFclCadenceParameterTypes(), src.Return.ToFclCadenceType());
        }

        internal static IList<global::Flow.Net.Sdk.Core.Cadence.Types.CadenceParameterType> ToFclCadenceParameterTypes(this IList<Cadence.Types.CadenceParameterType> src)
        {
            var ret = new List<global::Flow.Net.Sdk.Core.Cadence.Types.CadenceParameterType>();

            for (int i = 0; i < src.Count; i++)
            {
                ret.Add(new global::Flow.Net.Sdk.Core.Cadence.Types.CadenceParameterType
                {
                    Id = src[i].Id,
                    Label = src[i].Label,
                    Type = src[i].Type.ToFclCadenceType()
                });
            }

            return ret;
        }

        internal static global::Flow.Net.Sdk.Core.Cadence.Types.CadenceOptionalType ToFclCadenceOptionalType(this Cadence.Types.CadenceOptionalType src)
        {
            return new global::Flow.Net.Sdk.Core.Cadence.Types.CadenceOptionalType(src.Type.ToFclCadenceType());
        }

        internal static global::Flow.Net.Sdk.Core.Cadence.Types.CadenceReferenceType ToFclCadenceReferenceType(this Cadence.Types.CadenceReferenceType src)
        {
            return new global::Flow.Net.Sdk.Core.Cadence.Types.CadenceReferenceType(src.Authorized, src.Type.ToFclCadenceType());
        }

        internal static global::Flow.Net.Sdk.Core.Cadence.Types.CadenceRestrictedType ToFclCadenceRestrictedType(this Cadence.Types.CadenceRestrictedType src)
        {
            return new global::Flow.Net.Sdk.Core.Cadence.Types.CadenceRestrictedType(src.TypeId, src.Type.ToFclCadenceType(), src.Restrictions.ToFclCadenceTypes());
        }

        internal static IList<ICadenceType> ToFclCadenceTypes(this IList<CadenceTypeBase> src)
        {
            var ret = new List<ICadenceType>();

            for (int i = 0; i < src.Count; i++)
            {
                ret.Add(src[i].ToFclCadenceType());
            }

            return ret;
        }

        internal static global::Flow.Net.Sdk.Core.Cadence.Types.CadenceVariableSizedArrayType ToFclCadenceVariableSizedArrayType(this Cadence.Types.CadenceVariableSizedArrayType src)
        {
            return new global::Flow.Net.Sdk.Core.Cadence.Types.CadenceVariableSizedArrayType(src.Type.ToFclCadenceType());
        }
    }
}
