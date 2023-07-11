using System.Numerics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Org.BouncyCastle.Crypto.Digests;

namespace DapperLabs.Flow.Sdk.Cadence
{

    /// <summary>
    /// The Convert class exposes two functions - ToCadence and FromCadence.  These convert between Cadence and C#
    /// datatypes.
    /// </summary>
    /// <example>
    /// <code>
    /// using DapperLabs.Flow.Sdk.Cadence;
    /// Int16 i = Convert.FromCadence&lt;Int16&gt;(cadence);
    /// CadenceBase cb = Convert.ToCadence(i, "Int16");
    /// </code>
    /// </example>
    /// To convert from a C# class/struct to a CadenceStruct, you must annotate your C# struct.
    /// <example>
    /// <code>
    /// [Cadence(CadenceType = "A.XXX.ContractName.StructName")]
    /// public class TestClass
    /// {
    ///    [Cadence(CadenceType = "String")]
    ///    public String s;
    ///    [Cadence(CadenceType = "[Int16]?")]
    ///    public List&lt;Int16&gt; li;
    ///    [Cadence(CadenceType = "Struct"]
    ///    public OtherClass other;
    /// }
    /// </code>
    /// Where XXX is the address (without a leading 0x) of the account that contains the contract.
    /// </example>
    /// C# types *must* match the Cadence type being converted from/to or an exception will be thrown.  The following are the required types:
    /// Cadence|C#
    /// ---|---
    /// Int|BigInteger
    /// UInt|BigInteger
    /// Int8|SByte
    /// Int16|Int16
    /// Int32|Int32
    /// Int64|Int64
    /// Int128|BigInteger
    /// Int256|BigInteger
    /// UInt8|Byte
    /// UInt16|UInt16
    /// UInt32|UInt32
    /// UInt64|UInt64
    /// UInt128|BigInteger
    /// UInt256|BigInteger
    /// Word8|Byte
    /// Word16|UInt16
    /// Word32|UInt32
    /// Word64|UInt64
    /// Fix64|Decimal
    /// UFix64|Decimal
    /// Address|String
    /// String|String
    /// Bool|Boolean
    /// Path|CadencePathValue
    /// Capability|CadenceCapabilityValue
    /// Array ([T])|List<T>
    /// Dictionary ({T:R})|Dictionary&lt;T,R&gt;
    /// Struct|Class or Struct
    /// Optional (?)|Reference type or Nullable&lt;T&gt;>
    ///
    /// Cadence Optionals must be either a C# reference type or be wrapped in a Nullable.
    /// <example>
    /// A cadence type of [String]? would be a C# type of List&lt;String&gt; because List is a reference type.
    /// A cadence type of [Int16?] would be a C# type of List&lt;Int16?&gt; because Int16 is a value type and must be marked as Nullable by appending a ?
    /// </example>
    public static class Convert
    {
        /// <summary>
        /// A mapping of Cadence types to C# types.  Trying to convert to a different type will result in an Exception 
        /// </summary>
        private static Dictionary<string, Type> CadenceToCSharpTypeConversions = new Dictionary<string, Type>
        {
            ["Int"] = typeof(BigInteger),
            ["UInt"] = typeof(BigInteger),
            ["Int8"] = typeof(SByte),
            ["Int16"] = typeof(Int16),
            ["Int32"] = typeof(Int32),
            ["Int64"] = typeof(Int64),
            ["Int128"] = typeof(BigInteger),
            ["Int256"] = typeof(BigInteger),
            ["UInt8"] = typeof(Byte),
            ["UInt16"] = typeof(UInt16),
            ["UInt32"] = typeof(UInt32),
            ["UInt64"] = typeof(UInt64),
            ["UInt128"] = typeof(BigInteger),
            ["UInt256"] = typeof(BigInteger),
            ["Word8"] = typeof(Byte),
            ["Word16"] = typeof(UInt16),
            ["Word32"] = typeof(UInt32),
            ["Word64"] = typeof(UInt64),
            ["Fix64"] = typeof(Decimal),
            ["UFix64"] = typeof(Decimal),
            ["Address"] = typeof(String),
            ["String"] = typeof(String),
            ["Bool"] = typeof(Boolean),
            ["Path"] = typeof(CadencePathValue),
            ["Capability"] = typeof(CadenceCapabilityValue),
        };

#if UNITY_IOS
        private static bool done_aot_parse = false;
#endif

        /// <summary>
        /// Converts a primitive Cadence type into a C# type
        /// </summary>
        /// <param name="cadence">The CadenceBase to convert</param>
        /// <typeparam name="T">The Type to convert to</typeparam>
        /// <returns>A T representing the passed CadenceBase value</returns>
        /// <exception cref="Exception">Thrown if an error occurs</exception>
        private static T FromCadencePrimitive<T>(CadenceBase cadence)
        {
#if UNITY_IOS
            // This is required on iOS so that IL2CPP generates code for these Parse functions.
            // It does nothing at runtime and is only required for AOT compilation. 
            if (!done_aot_parse)
            {
                var test1 = BigInteger.Parse("1");
                var test2 = SByte.Parse("1");
                var test3 = Int16.Parse("1");
                var test4 = Int32.Parse("1");
                var test5 = Int64.Parse("1");
                var test6 = Byte.Parse("1");
                var test7 = UInt16.Parse("1");
                var test8 = UInt32.Parse("1");
                var test9 = UInt64.Parse("1");
                var test10 = Decimal.Parse("1.0");
                done_aot_parse = true;
            }
#endif

            //CadenceComposites are not a primitive type.  Error.
            if (cadence.GetType() == typeof(CadenceComposite))
            {
                throw new System.Exception("Attempt to convert CadenceComposite as primitive");
            }

            //No mapping for this cadence type, throw and error.
            if (!CadenceToCSharpTypeConversions.ContainsKey(cadence.Type))
            {
                throw new System.Exception($"Unknown cadence type {cadence.Type}");
            }
            
            //No valid type mapping found, throw an error.
            if (typeof(T) != CadenceToCSharpTypeConversions[cadence.Type])
            {
                throw new System.Exception($"Attempt to convert cadence to invalid type.  Cadence type {cadence.Type} expects {CadenceToCSharpTypeConversions[cadence.Type]}, got {typeof(T)}");
            }

            //Handle the different types of cadence numbers by invoking the destination type's Parse method
            if (cadence.GetType() == typeof(CadenceNumber))
            {
                return (T)CadenceToCSharpTypeConversions[cadence.Type].GetMethods().First(x => x.GetParameters().Length == 1 && x.Name == "Parse").Invoke(null, new object[] { cadence.As<CadenceNumber>().Value });
            }

            //CadenceString conversion
            if (cadence.GetType() == typeof(CadenceString))
            {
                return System.Convert.ChangeType(cadence.As<CadenceString>().Value, CadenceToCSharpTypeConversions[cadence.Type]) is T ? (T)System.Convert.ChangeType(cadence.As<CadenceString>().Value, CadenceToCSharpTypeConversions[cadence.Type]) : default;
            }

            //CadenceAddress conversion
            if (cadence.GetType() == typeof(CadenceAddress))
            {
                return System.Convert.ChangeType(cadence.As<CadenceAddress>().Value, CadenceToCSharpTypeConversions[cadence.Type]) is T ? (T)System.Convert.ChangeType(cadence.As<CadenceAddress>().Value, CadenceToCSharpTypeConversions[cadence.Type]) : default;
            }

            //CadenceBool conversion
            if (cadence.GetType() == typeof(CadenceBool))
            {
                return System.Convert.ChangeType(cadence.As<CadenceBool>().Value, CadenceToCSharpTypeConversions[cadence.Type]) is T ? (T)System.Convert.ChangeType(cadence.As<CadenceBool>().Value, CadenceToCSharpTypeConversions[cadence.Type]) : default;
            }

            //CadencePath conversion.  Will return a CadencePathValue
            if (cadence.GetType() == typeof(CadencePath))
            {
                return (T)(object)cadence.As<CadencePath>().Value;
            }

            //CadenceCapability conversion.  Will return a CadenceCapabilityValue
            if (cadence.GetType() == typeof(CadenceCapability))
            {
                return (T)(object)cadence.As<CadenceCapability>().Value;
            }

            //Throw an exception if we don't know how to handle the given Cadence type
            throw new System.Exception($"Unknown cadence primitive type {cadence.Type}");
        }

        /// <summary>
        /// Converts a CadenceBase type into a C# type
        /// </summary>
        /// <param name="cadence">The CadenceBase value to convert</param>
        /// <typeparam name="T">The C# type to convert it to</typeparam>
        /// <returns>The Cadence value as an instance of type T</returns>
        /// <exception cref="Exception">Thrown if an error occurs</exception>
        public static T FromCadence<T>(CadenceBase cadence)
        {
            //See if this is a CadenceOptional
            if (cadence.GetType() == typeof(CadenceOptional))
            {
                //Ensure the requested type is nullable, throw if not
                if (!IsNullable(typeof(T)))
                {
                    throw new System.Exception($"CadenceOptional requires a nullable type, got: {typeof(T)}.");
                }

                //If the Optional is nil, return null
                if (cadence.As<CadenceOptional>().Value == null)
                {
                    return default;
                }

                //The Optional is not nil so convert the value to the requested C# type and return it
                //Handle if it's a generic Nullable (value type)
                if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    Type propertyType = typeof(T).GetGenericArguments()[0];
                    MethodInfo method = typeof(Convert).GetMethods().First(m => m.Name == "FromCadence").MakeGenericMethod(propertyType);
                    return (T)method.Invoke(null, new object[] { cadence.As<CadenceOptional>().Value });
                }
                //Handle if it's a reference type
                else
                {
                    return FromCadence<T>(cadence.As<CadenceOptional>().Value);
                }
            }

            //Handle CadenceArrays.  Converts into a List
            if (cadence.GetType() == typeof(CadenceArray))
            {
                //Ensure the requested type is a List
                if (!typeof(T).IsGenericType || typeof(T).GetGenericTypeDefinition() != typeof(List<>))
                {
                    throw new System.Exception($"Cadence type is array, passed type is {typeof(T)}.  Must be List<>.");
                }

                //Create a generic method version of FromCadence that returns the desired type
                MethodInfo method = typeof(Convert).GetMethods().First(m => m.Name == "FromCadence").MakeGenericMethod(typeof(T).GetGenericArguments().Single());

                //Create a type to hold the converted data
                Type listType = typeof(List<>).MakeGenericType(typeof(T).GetGenericArguments().Single());

                //Create an instance of the type to hold the data
                IList newList = (IList)Activator.CreateInstance(listType);

                //Convert each element into the desired type and add it to a List
                IEnumerable<object> a = cadence.As<CadenceArray>().Value.Select(x => method.Invoke(null, new[] { x }));
                foreach (object obj in a)
                {
                    newList.Add(obj);
                }

                return (T)newList;
            }

            //Handle CadenceDictionary inputs
            if (cadence.GetType() == typeof(CadenceDictionary))
            {
                //Ensure the target type is a Dictionary
                if (typeof(T).GetGenericTypeDefinition() != typeof(Dictionary<,>))
                {
                    throw new System.Exception($"Cadence type is dictionary, passed type is {typeof(T)}.  Must be Dictionary<,>.");
                }

                //Get the requested key and value types
                Type keyType = typeof(T).GetGenericArguments()[0];
                Type valueType = typeof(T).GetGenericArguments()[1];

                //Create generic methods of the appropriate types for the key and value portions of the CadenceDictionary
                MethodInfo keyMethod = typeof(Convert).GetMethods().First(m => m.Name == "FromCadence").MakeGenericMethod(keyType);
                MethodInfo valueMethod = typeof(Convert).GetMethods().First(m => m.Name == "FromCadence").MakeGenericMethod(valueType);

                //Create a Dictionary type and instance of the required type
                Type dictionaryType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
                IDictionary newDictionary = (IDictionary)Activator.CreateInstance(dictionaryType);

                //Convert each key/value pair into the C# equivalents
                IEnumerable<object> keys = cadence.As<CadenceDictionary>().Value.Select(x => keyMethod.Invoke(null, new[] { x.Key }));
                IEnumerable<object> values = cadence.As<CadenceDictionary>().Value.Select(x => valueMethod.Invoke(null, new[] { x.Value }));

                //Populate the C# dictionary
                for (int i = 0; i < keys.ToList().Count; i++)
                {
                    newDictionary[keys.ToList()[i]] = values.ToList()[i];
                }

                return (T)newDictionary;
            }

            //Handle CadenceComposite types
            if (cadence.GetType() == typeof(CadenceComposite))
            {
                CadenceAttribute cadenceAttribute = typeof(T).GetCustomAttribute<CadenceAttribute>();

                //Create an instance of the requested type to populate later
                T target = (T)Activator.CreateInstance(typeof(T));

                //Iterate through all the fields in the requested type
                FieldInfo[] targetFields = typeof(T).GetFields();
                foreach (FieldInfo field in targetFields)
                {
                    //Use the field's name unless a CadenceAttribute indicates a different name
                    string name = field.Name;
                    CadenceAttribute f = field.GetCustomAttribute<CadenceAttribute>();
                    if (f != null && f.Name != null)
                    {
                        name = f.Name;
                    }

                    //Iterate through all the fields on the CadenceComposite to find one with a matching name
                    foreach (CadenceCompositeField cadenceField in cadence.As<CadenceComposite>().Value.Fields)
                    {
                        //Name doesn't match, continue to next field
                        if (cadenceField.Name != name) continue;

                        //Match found, convert from CadenceBase to C#
                        MethodInfo method = typeof(Convert).GetMethods().First(m => m.Name == "FromCadence").MakeGenericMethod(field.FieldType);
                        object value = method.Invoke(null, new[] { cadenceField.Value });
                        object targetObject = target;
                        field.SetValue(targetObject, value);
                        target = (T)targetObject;
                    }
                }

                return target;
            }

            //No other matches found, try and process as a CadenceBase primitive
            return FromCadencePrimitive<T>(cadence);
        }

        private static CadenceBase ToCadencePrimitive(object source, string destinationType)
        {
            destinationType = destinationType.Trim();

            //Make sure the source is compatible with the destination
            if (source.GetType() != CadenceToCSharpTypeConversions[destinationType])
            {
                throw new System.Exception($"Can not convert {source.GetType()} into {destinationType}.  Requires source to be {CadenceToCSharpTypeConversions[destinationType]}.");
            }

            if (destinationType == "Fix64")
            {
                return new CadenceNumber(CadenceNumberType.Fix64, ((Decimal)source).ToString("0.00000000"));
            }

            if (destinationType == "UFix64")
            {
                return new CadenceNumber(CadenceNumberType.UFix64, ((Decimal)source).ToString("0.00000000"));
            }


            //See if this is a CadenceNumber type (string matches string version of a type in CadenceNumberType enum)
            if (((CadenceNumberType[])Enum.GetValues(typeof(CadenceNumberType))).Select(x => x.ToString()).Contains(destinationType))
            {
                CadenceNumberType parsedType;
                if (!CadenceNumberType.TryParse(destinationType, out parsedType))
                {
                    throw new System.Exception("Couldn't parse CadenceNumberType.  This shouldn't happen");
                }

                return new CadenceNumber(parsedType, $"{source}");
            }

            //Handle other primitive types
            return destinationType switch
            {
                "Address" => new CadenceAddress($"{source}"),
                "String" => new CadenceString($"{source}"),
                "Bool" => new CadenceBool((bool)source),
                "Path" => new CadencePath((CadencePathValue)source),
                "Capability" => new CadenceCapability((CadenceCapabilityValue)source),
                _ => throw new System.Exception("Couldn't create Cadence type from given source.")
            };
        }

        /// <summary>
        /// Converts from a C# datatype to a Cadence datatype
        /// </summary>
        /// <param name="source">The C# object to convert</param>
        /// <param name="destinationType">A string indicating the Cadence datatype to conver to.</param>
        /// <returns>A CadenceBase object</returns>
        /// <exception cref="Exception">Throws an exception if the source can not be converted.</exception>
        public static CadenceBase ToCadence(object source, string destinationType)
        {
            //Call private version that maintains the field name of the field that's currently being processed.
            return ToCadence(source, destinationType, "Source");
        }
        
        /// <summary>
        /// Converts from a C# datatype to a Cadence datatype.  This internal version keeps track of the name of the
        /// field that is currently being processed so that any exceptions thrown can indicate which field caused
        /// the problem.
        /// </summary>
        /// <param name="source">The C# object to convert</param>
        /// <param name="destinationType">A string indicating the Cadence datatype to conver to.</param>
        /// <param name="fieldName">If a class/struct is being processed, keeps track of the name of the field that is currently being processed.</param>
        /// <returns>A CadenceBase object</returns>
        /// <exception cref="Exception">Throws an exception if the source can not be converted.</exception>
        private static CadenceBase ToCadence(object source, string destinationType, string fieldName)
        {
            destinationType = destinationType.Trim();
            
            //See if this is a primitive, if so return the cadence version
            if (CadenceToCSharpTypeConversions.ContainsKey(destinationType))
            {
                return ToCadencePrimitive(source, destinationType);
            }

            //See if this is an array via regex
            if (Regex.IsMatch(destinationType, @"^\[(.+?)\]$"))
            {
                if (source == null)
                {
                    throw new System.Exception($"{fieldName} is null, Cadence type is not an Optional.  Make the CadenceType an optional if null is a valid options.");
                }

                //If it is an array, ensure the source type is a list
                if (!source.GetType().IsGenericType || source.GetType().GetGenericTypeDefinition() != typeof(List<>))
                {
                    throw new System.Exception($"Cadence type is Array, passed type is {source.GetType()}.  Source type must be List<>.");
                }

                //Create array to hold cadence values
                CadenceArray cadenceArray = new CadenceArray
                {
                    Value = new CadenceBase[] { }
                };

                List<CadenceBase> tempList = new List<CadenceBase>();

                string extractedType = Regex.Match(destinationType, @"\[(.*?)\]").Groups[1].Value;
                foreach (object element in source as IList)
                {
                    tempList.Add(ToCadence(element, extractedType));
                }

                cadenceArray.Value = tempList.ToArray();

                return cadenceArray;
            }

            //Use regex to determine if the cadence type is a dictionary
            if (Regex.IsMatch(destinationType, @"^{(.+?):(.+?)}$"))
            {
                if (source == null)
                {
                    throw new System.Exception($"{fieldName} is null, Cadence type is not an Optional.  Make the CadenceType an optional if null is a valid options.");
                }

                if (!source.GetType().IsGenericType || source.GetType().GetGenericTypeDefinition() != typeof(Dictionary<,>))
                {
                    throw new System.Exception($"Cadence type is Dictionary, passed type is {source.GetType()}.  Source type must be Dictionary<,>.");
                }

                string keyType, valueType;
                (keyType, valueType) = FindCadenceDictionaryKeyAndValue(destinationType);

                List<CadenceDictionaryItem> tempValueList = new List<CadenceDictionaryItem>();

                CadenceDictionary cadenceDictionary = new CadenceDictionary
                {
                    Value = new CadenceDictionaryItem[] { }
                };

                foreach (object key in (source as IDictionary).Keys)
                {
                    tempValueList.Add(new CadenceDictionaryItem
                    {
                        Key = ToCadence(key, keyType),
                        Value = ToCadence((source as IDictionary)[key], valueType)
                    });
                }

                cadenceDictionary.Value = tempValueList.ToArray();

                return cadenceDictionary;
            }

            //Check if this is an optional (ends in ?)
            if (destinationType.EndsWith("?"))
            {
                //See if the optional is null.  If so, just return a new CadenceOptional which has its value set to null
                if (source == null)
                {
                    return new CadenceOptional();
                }

                //Create and return a CadenceOptional by populating it with the base type
                return new CadenceOptional(ToCadence(source, destinationType.Substring(0, destinationType.Length - 1)));
            }

            //Handle "Struct" types
            if (destinationType == "Struct" || destinationType == "Enum" ||  destinationType == "Resource" || destinationType== "Event" || destinationType== "Contract")
            {
                if (source == null)
                {
                    throw new System.Exception($"{fieldName} is null, Cadence type is not an Optional.  Make the CadenceType an optional if null is a valid option.");
                }
                
                CadenceComposite cadenceComposite = new CadenceComposite(destinationType);
                CadenceAttribute structAttribute = (CadenceAttribute)Attribute.GetCustomAttribute(source.GetType(), typeof(CadenceAttribute));

                cadenceComposite.Value = new CadenceCompositeValue
                {
                    Type = structAttribute.CadenceType
                };

                List<CadenceCompositeField> cadenceFields = new List<CadenceCompositeField>();

                //Iterate through all the fields in the source type
                FieldInfo[] targetFields = source.GetType().GetFields();
                foreach (FieldInfo field in targetFields)
                {
                    //Use the field's name unless a CadenceAttribute indicates a different name
                    string name = field.Name;
                    CadenceAttribute f = field.GetCustomAttribute<CadenceAttribute>();

                    if (f == null || f.CadenceType == null)
                    {
                        continue;
                    }

                    if (f.Name != null)
                    {
                        name = f.Name;
                    }

                    cadenceFields.Add(new CadenceCompositeField
                    {
                        Name = name,
                        Value = ToCadence(field.GetValue(source), f.CadenceType, $"{fieldName}.{name}")
                    });
                }

                cadenceComposite.Value.Fields = cadenceFields;

                return cadenceComposite;
            }

            throw new System.Exception($"Unknown cadence destination type {destinationType}");
        }

        private static (string, string) FindCadenceDictionaryKeyAndValue(string input)
        {
            Dictionary<char, int> nestingLevels = new Dictionary<char, int>();
            Dictionary<char, char> inverseChars = new Dictionary<char, char>
            {
                ['}'] = '{',
                [']'] = '['
            };

            //If the string doesn't start with '{', it's not a dictionary
            if (input[0] != '{' || input[input.Length-1] != '}')
            {
                throw new System.Exception($"Input string {input} does not appear to represent a Cadence dictionary type");
            }

            //Remove the outer curley brackets
            input = input.Substring(1, input.Length - 2);

            //Go all the characters in the inner string
            for (int i = 0; i < input.Length; i++)
            {
                //Look for ':'.  If a ':' is found and there are nesting levels, our key is everything before the ':'
                //our value is everything after the ':'
                if (input[i] == ':')
                {
                    if (nestingLevels.Count == 0)
                    {
                        return (input.Substring(0, i), input.Substring(i + 1));
                    }
                }

                //See if we're starting a new nesting level and record it if so
                if (input[i] == '{' || input[i] == '[')
                {
                    if (nestingLevels.ContainsKey(input[i]))
                    {
                        nestingLevels[input[i]]++;
                    }
                    else
                    {
                        nestingLevels[input[i]] = 1;
                    }
                }

                //See if we're ending a nesting level and record it if so
                if (input[i] == '}' || input[i] == ']')
                {
                    if (nestingLevels.ContainsKey(inverseChars[input[i]]))
                    {
                        nestingLevels[inverseChars[input[i]]]--;
                        if (nestingLevels[inverseChars[input[i]]] == 0)
                        {
                            nestingLevels.Remove(inverseChars[input[i]]);
                        }
                    }
                    else
                    {
                        throw new System.Exception($"Invalid cadence dictionary type: {{{input}}}");
                    }
                }
            }

            throw new System.Exception($"Invalid cadence dictionary type: {{{input}}}");
        }

        /// <summary>
        /// Helper function to determine if a type is nullable
        /// </summary>
        /// <param name="type">The type to check for nullability</param>
        /// <returns>True if the passed type is nullable, false if not</returns>
        private static bool IsNullable(Type type)
        {
            if (!type.IsValueType) return true;
            return Nullable.GetUnderlyingType(type) != null;
        }

        /// <summary>
        /// Calculates the SHA3-256 hash of a script.  Useful for determining the types of cadence structs declared in scripts.
        /// </summary>
        /// <param name="script">The Cadence code of the script</param>
        /// <returns>The string representation of the hash of the script</returns>
        private static string CalculateScriptHash(string script)
        {
            Sha3Digest hashAlgorithm = new Sha3Digest(256);
            byte[] input = Encoding.UTF8.GetBytes(script);
            hashAlgorithm.BlockUpdate(input, 0, input.Length);
            byte[] result = new byte[32];
            hashAlgorithm.DoFinal(result, 0);
            return BitConverter.ToString(result).Replace("-", "").ToLowerInvariant();
        }
    }
}
