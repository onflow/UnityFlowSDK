using System;
using System.Collections.Generic;
using System.Numerics;
using DapperLabs.Flow.Sdk.Cadence;
using DapperLabs.Flow.Sdk.Cadence.Types;
using Convert = DapperLabs.Flow.Sdk.Cadence.Convert;

namespace DapperLabs.Flow.Sdk.Niftory
{
    internal static class NiftoryExtensions
    {
        internal static object ToNiftoryObject(this CadenceBase src)
        {
            switch (src.Type)
            {
                case "Address": return Convert.FromCadence<string>(src);
                case "String": return Convert.FromCadence<string>(src);
                case "Character": return Convert.FromCadence<char>(src);
                case "Bool": return Convert.FromCadence<bool>(src);
                case "Optional": return ((Cadence.CadenceOptional)src).ToNiftoryCadenceOptional();

                case "Int": return Convert.FromCadence<BigInteger>(src);
                case "UInt": return Convert.FromCadence<BigInteger>(src);
                case "Int8": return Convert.FromCadence<sbyte>(src);
                case "UInt8": return Convert.FromCadence<byte>(src);
                case "Int16": return Convert.FromCadence<Int16>(src);
                case "UInt16": return Convert.FromCadence<UInt16>(src);
                case "Int32": return Convert.FromCadence<Int32>(src);
                case "UInt32": return Convert.FromCadence<UInt32>(src);
                case "Int64": return Convert.FromCadence<Int64>(src);
                case "UInt64": return Convert.FromCadence<UInt64>(src);
                case "Int128": return Convert.FromCadence<BigInteger>(src);
                case "UInt128": return Convert.FromCadence<BigInteger>(src);
                case "Int256": return Convert.FromCadence<BigInteger>(src);
                case "UInt256": return Convert.FromCadence<BigInteger>(src);
                case "Word8": return Convert.FromCadence<byte>(src);
                case "Word16": return Convert.FromCadence<UInt16>(src);
                case "Word32": return Convert.FromCadence<UInt32>(src);
                case "Word64": return Convert.FromCadence<UInt64>(src);
                case "Fix64": return Convert.FromCadence<Decimal>(src).ToString("0.00000000");
                case "UFix64": return Convert.FromCadence<Decimal>(src).ToString("0.00000000");

                case "Array": return ((Cadence.CadenceArray)src).ToNiftoryCadenceArray();
                case "Dictionary": return ((Cadence.CadenceDictionary)src).ToNiftoryCadenceDictionary();

                case "Struct":
                case "Resource":
                case "Event":
                case "Contract":
                case "Enum":
                    return ((Cadence.CadenceComposite)src).ToNiftoryCadenceComposite();

                case "Capability": return ((Cadence.CadenceCapability)src).ToNiftoryCadenceCapability();
                case "Link": return ((Cadence.CadenceLink)src).ToNiftoryCadenceLink();
                case "Path": return ((Cadence.CadencePath)src).ToNiftoryCadencePath();
                case "Type": return ((Cadence.CadenceType)src).ToNiftoryCadenceType();

                case "Void":
                default:
                    throw new System.Exception($"Niftory type converter: unknown type {src.Type}.");
            }
        }

        internal static object[] ToNiftoryCadenceArray(this Cadence.CadenceArray src)
        {
            var values = new object[src.Value.Length];

            for (int i = 0; i < src.Value.Length; i++)
            {
                values[i] = src.Value[i].ToNiftoryObject();
            }

            return values;
        }

        internal static Dictionary<object, object> ToNiftoryCadenceDictionary(this Cadence.CadenceDictionary src)
        {
            var values = new Dictionary<object, object>();

            for (int i = 0; i < src.Value.Length; i++)
            {
                values.Add(src.Value[i].Key.ToNiftoryObject(), src.Value[i].Value.ToNiftoryObject());
            }

            return values;
        }

        internal static Dictionary<string, object> ToNiftoryCadenceComposite(this Cadence.CadenceComposite src)
        {
            Dictionary<string, object> values = new Dictionary<string, object>();

            foreach (var field in src.Value.Fields)
            {
                values.Add(field.Name, field.Value.ToNiftoryObject());
            }

            return values;
        }

        internal static Dictionary<string, object> ToNiftoryCadenceCapability(this Cadence.CadenceCapability src)
        {
            return new Dictionary<string, object>()
            {
                ["path"] = src.Value.Path,
                ["address"] = src.Value.Address,
                ["borrowType"] = src.Value.BorrowType.ToNiftoryCadenceType()
            };
        }
        internal static CadenceLinkValue ToNiftoryCadenceLink(this Cadence.CadenceLink src)
        {
            return src.Value;
        }
        internal static CadencePathValue ToNiftoryCadencePath(this Cadence.CadencePath src)
        {
            return src.Value;
        }
        internal static Dictionary<string, object> ToNiftoryCadenceType(this Cadence.CadenceType src)
        {
            return new Dictionary<string, object>()
            {
                ["staticType"] = src.Value.StaticType.ToNiftoryCadenceType()
            };
        }
        internal static object ToNiftoryCadenceOptional(this Cadence.CadenceOptional src)
        {
            return src.Value == null ? null : src.Value.ToNiftoryObject();
        }

        internal static object ToNiftoryCadenceType(this CadenceTypeBase src)
        {
            switch (src.Kind)
            {
                case "Capability": return ((Cadence.Types.CadenceCapabilityType)src).ToNiftoryCadenceCapabilityType();
                case "ConstantSizedArray": return ((Cadence.Types.CadenceConstantSizedArrayType)src).ToNiftoryCadenceConstantSizedArrayType();
                case "Dictionary": return ((Cadence.Types.CadenceDictionaryType)src).ToNiftoryCadenceDictionaryType();
                case "Enum": return ((Cadence.Types.CadenceEnumType)src).ToNiftoryCadenceEnumType();
                case "Function": return ((Cadence.Types.CadenceFunctionType)src).ToNiftoryCadenceFunctionType();
                case "Optional": return ((Cadence.Types.CadenceOptionalType)src).ToNiftoryCadenceOptionalType();
                case "Reference": return ((Cadence.Types.CadenceReferenceType)src).ToNiftoryCadenceReferenceType();
                case "Restriction": return ((Cadence.Types.CadenceRestrictedType)src).ToNiftoryCadenceRestrictedType();
                case "VariableSizedArray": return ((Cadence.Types.CadenceVariableSizedArrayType)src).ToNiftoryCadenceVariableSizedArrayType();
                case "Struct":
                case "Resource":
                case "Event":
                case "Contract":
                case "StructInterface":
                case "ResourceInterface":
                case "ContractInterface":
                    return ((Cadence.Types.CadenceCompositeType)src).ToNiftoryCadenceCompositeType();
            }

            return null;
        }
        internal static IList<object> ToNiftoryCadenceTypes(this IList<CadenceTypeBase> src)
        {
            var ret = new List<object>();

            for (int i = 0; i < src.Count; i++)
            {
                ret.Add(src[i].ToNiftoryCadenceType());
            }

            return ret;
        }
        internal static Dictionary<string, object> ToNiftoryCadenceCompositeType(this Cadence.Types.CadenceCompositeType src)
        {
            var initializers = new List<IList<Dictionary<string, object>>>();

            for (int i = 0; i < src.Initializers.Count; i++)
            {
                initializers.Add(src.Initializers[i].ToNiftoryCadenceInitializerTypes());
            }

            return new Dictionary<string, object>
            {
                ["kind"] = src.Kind,
                ["type"] = src.Type,
                ["typeID"] = src.TypeId,
                ["initializers"] = initializers,
                ["fields"] = src.Fields.ToNiftoryCadenceFieldTypes()
            };
        }

        internal static Dictionary<string, object> ToNiftoryCadenceCapabilityType(this Cadence.Types.CadenceCapabilityType src)
        {
            return new Dictionary<string, object>
            {
                ["kind"] = src.Kind,
                ["type"] = src.Type.ToNiftoryCadenceType()
            };
        }
        internal static Dictionary<string, object> ToNiftoryCadenceConstantSizedArrayType(this Cadence.Types.CadenceConstantSizedArrayType src)
        {
            return new Dictionary<string, object>
            {
                ["kind"] = src.Kind,
                ["size"] = src.Size,
                ["type"] = src.Type.ToNiftoryCadenceType()
            };
        }
        internal static Dictionary<string, object> ToNiftoryCadenceDictionaryType(this Cadence.Types.CadenceDictionaryType src)
        {
            return new Dictionary<string, object>
            {
                ["kind"] = src.Kind,
                ["size"] = src.Key.ToNiftoryCadenceType(),
                ["type"] = src.Value.ToNiftoryCadenceType()
            };
        }
        internal static Dictionary<string, object> ToNiftoryCadenceEnumType(this Cadence.Types.CadenceEnumType src)
        {
            return new Dictionary<string, object>
            {
                ["kind"] = src.Kind,
                ["size"] = src.Type.ToNiftoryCadenceType(),
                ["type"] = src.TypeId,
                ["initializers"] = src.Initializers.ToNiftoryCadenceInitializerTypes(),
                ["fields"] = src.Fields.ToNiftoryCadenceFieldTypes()
            };
        }
        internal static Dictionary<string, object> ToNiftoryCadenceFunctionType(this Cadence.Types.CadenceFunctionType src)
        {
            return new Dictionary<string, object>
            {
                ["kind"] = src.Kind,
                ["typeID"] = src.TypeId,
                ["parameters"] = src.Parameters.ToNiftoryCadenceParameterTypes(),
                ["return"] = src.Return.ToNiftoryCadenceType()
            };
        }
        internal static Dictionary<string, object> ToNiftoryCadenceOptionalType(this Cadence.Types.CadenceOptionalType src)
        {
            return new Dictionary<string, object>
            {
                ["kind"] = src.Kind,
                ["type"] = src.Type.ToNiftoryCadenceType()
            };
        }
        internal static Dictionary<string, object> ToNiftoryCadenceReferenceType(this Cadence.Types.CadenceReferenceType src)
        {
            return new Dictionary<string, object>
            {
                ["kind"] = src.Kind,
                ["authorized"] = src.Authorized,
                ["type"] = src.Type.ToNiftoryCadenceType()
            };
        }
        internal static Dictionary<string, object> ToNiftoryCadenceRestrictedType(this Cadence.Types.CadenceRestrictedType src)
        {
            return new Dictionary<string, object>
            {
                ["kind"] = src.Kind,
                ["type"] = src.Type.ToNiftoryCadenceType(),
                ["typeID"] = src.TypeId,
                ["restrictions"] = src.Restrictions.ToNiftoryCadenceTypes()
            };
        }
        internal static Dictionary<string, object> ToNiftoryCadenceVariableSizedArrayType(this Cadence.Types.CadenceVariableSizedArrayType src)
        {
            return new Dictionary<string, object>
            {
                ["kind"] = src.Kind,
                ["type"] = src.Type.ToNiftoryCadenceType()
            };
        }

        internal static IList<Dictionary<string, object>> ToNiftoryCadenceInitializerTypes(this IList<Cadence.Types.CadenceInitializerType> src)
        {
            var ret = new List<Dictionary<string, object>>();

            for (int i = 0; i < src.Count; i++)
            {
                ret.Add(new Dictionary<string, object>()
                {
                    ["id"] = src[i].Id,
                    ["label"] = src[i].Label,
                    ["type"] = src[i].Type.ToNiftoryCadenceType()
                });
            }

            return ret;
        }
        internal static IList<Dictionary<string, object>> ToNiftoryCadenceFieldTypes(this IList<Cadence.Types.CadenceFieldType> src)
        {
            var ret = new List<Dictionary<string, object>>();

            for (int i = 0; i < src.Count; i++)
            {
                ret.Add(new Dictionary<string, object>
                {
                    ["id"] = src[i].Id,
                    ["type"] = src[i].Type.ToNiftoryCadenceType()
                });
            }

            return ret;
        }
        internal static IList<Dictionary<string, object>> ToNiftoryCadenceParameterTypes(this IList<Cadence.Types.CadenceParameterType> src)
        {
            var ret = new List<Dictionary<string, object>>();

            for (int i = 0; i < src.Count; i++)
            {
                ret.Add(new Dictionary<string, object>
                {
                    ["label"] = src[i].Label,
                    ["id"] = src[i].Id,
                    ["type"] = src[i].Type.ToNiftoryCadenceType()
                });
            }

            return ret;
        }
    }
}
