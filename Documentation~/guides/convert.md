# Flow SDK Convert

The FlowSDK provides a Convert class to convert between Cadence and C# datatypes.  It provides two functions, ToCadence and FromCadence.

The conversion is strict in regards to the types of the data.  C# types *must* match the Cadence type being converted from/to or an exception will be thrown.  The following are the required types:

Cadence|C#
---|---
Int|BigInteger
UInt|BigInteger
Int8|SByte
Int16|Int16
Int32|Int32
Int64|Int64
Int128|BigInteger
Int256|BigInteger
UInt8|Byte
UInt16|UInt16
UInt32|UInt32
UInt64|UInt64
UInt128|BigInteger
UInt256|BigInteger
Word8|Byte
Word16|UInt16
Word32|UInt32
Word64|UInt64
Fix64|Decimal
UFix64|Decimal
Address|String
String|String
Bool|Boolean
Path|CadencePathValue
Capability|CadenceCapabilityValue
Array ([T])|List&lt;T&gt;
Dictionary ({T:R})|Dictionary&lt;T,R&gt;
Struct|Class or Struct


## Usage

### ToCadence

The Convert.ToCadence function has the following signature:
```csharp
public static CadenceBase ToCadence(object source, string destinationType)
```

ToCadence will return a CadenceBase that can be passed into Script and Transaction functions.  Example:

```csharp
CadenceBase cb = Convert.ToCadence((Int64)44, "Int64");
```

This will result in a CadenceNumber with a value of "44" and a type of "Int64".  However:

```csharp
CadenceBase cb = Convert.ToCadence(44, "Int64");
```

will result in an exception "Exception: Can not convert System.Int32 into Int64.  Requires source to be System.Int64."

Similarly, the Cadence Int type is an arbitrary precision integer type and thus must be created using a System.BigInteger.

```csharp
CadenceBase cb = Convert.ToCadence(new BigInteger(44), "Int");
```

### FromCadence

The Convert.FromCadence has the following signature:

```csharp
public static T FromCadence<T>(CadenceBase cadence)
```
where T is the C# you would like the Cadence converted into.

FromCadence will return a value of the requested type given a CadenceBase value.  The Cadence and C# types must match.

```csharp
CadenceBase cb = new CadenceNumber(CadenceNumberType.Int16, "44");
Int16 i = Convert.FromCadence<Int16>(cb);
```

If the requested and source types do not match, an exception will be thrown.

```csharp
CadenceBase cb = new CadenceNumber(CadenceNumberType.Int16, "44");
Int64 i = Convert.FromCadence<Int64>(cb);
```

The above results in an exception:  "Exception: Attempt to convert cadence to invalid type.  Cadence type Int16 expects System.Int16, got System.Int64".

### Composite Types

Using the Convert class on primitive types isn't much easier than constructing the Cadence types yourself.  Using it on Composite types is much more useful.

Convert can convert between Cadence composite types (Struct, Enum, Resource, Event, Contract) and C# structs/classes.  In order to annotate your C# classes/structs a CadenceAttribute is provided.
The CadenceAttribute has two properties.  The Name and CadenceType properties allow you to provide hints to the converter as to what each C# types should be when converted to Cadence.

Given the following C# class:

```csharp
public class TestStruct
{
    public Int32 i32;
    public String s;
}
```

and the following Cadence struct:

```cadence
pub struct TestStruct {
    pub let i32: Int32
    pub let s: String
}
```

you can convert from Cadence to C# using:

```csharp
TestStruct ts = Convert.FromCadence<TestStruct>(cadence);
```

It will match based on the field names, and the types are compatible.  Converting from C# to Cadence, on the other hand, requires annotations.

```csharp
[Cadence(CadenceType="A.XXX.CCC.TestStruct")]
public class TestStruct
{
    [Cadence(CadenceType="Int32", Name="i32")]
    public Int32 i;
    [Cadence(CadenceType="String")]
    public String s;
}
```

This is because a C# string could be a Cadence Address or Cadence String.  For consistency, Convert requires all C#->Cadence conversions to be annotated with a CadenceType.  If a field is not 
annotated, it will be skipped when converting from C# to Cadence.

You can also use the Name parameter to account for differences in field naming.  In the above example we mapped the Cadence "i32" field to the C# "i" field.  The Name property is optional and
it will use the field name if no Name property is given.

***Note:  The CadenceType annotation on C# classes/structs ([Cadence(CadenceType="A.XXX.CCC.TestStruct")] in the above example) is ignored when converting from Cadence to C#.
Convert.FromCadence will populate all fields that have matching names/types regardless of the type of Cadence struct that is being converted.  This allows you to convert a Cadence struct defined
int a Cadence script into C# even if you do not know what the Cadence type is.***

The class annotation (A.XXX.CCC.TestStruct) is required when converting from C# to Cadence. XXX should be the address of the account that contains the contract where the struct is 
defined, without a leading "0x".  The CCC is the name of the contract.

Here's an example using the NBA TopShot contract (https://github.com/dapperlabs/nba-smart-contracts/blob/master/contracts/TopShot.cdc) on TestNet, defining a class to hold the Play struct:

```csharp
[Cadence(CadenceType="A.877931736ee77cff.TopShot.Play")]
public class Play
{
    [Cadence(CadenceType="UInt32")]
    public UInt32 playID; 
    [Cadence(CadenceType="{String:String}")]
    public Dictionary<String, String> metadata;
}
```

## Structs inside Structs

If a Cadence struct contains another struct, the field should be annotated as a "Struct".  Given the following Cadence:

```cadence
pub struct Other {
    pub let i: Int16

    pub init(i:Int16) {
        self.i=i
    }
}

pub struct Test {
    pub let o : Other
    
    pub init(i: Int16) {
        self.o = Other(i:i)
    }
}
```

you could use this C#:

```csharp
[Cadence(CadenceType="A.xxx.ccc.Nested")]
public class Other
{
    [Cadence(CadenceType="Int16")]
    public Int16 i;
}

[Cadence(CadenceType="A.xxx.ccc.Test")]
public class Test
{
    [Cadence(CadenceType="Struct")]
    public Other o;
} 
```

If you have a Cadence Test struct you can convert into C# using:
```csharp
TestStruct ts = Convert.FromCadence<TestStruct>(cadence);
```

If you have a C# Test object, you can convert to Cadence using:
```csharp
CadenceBase cb = Convert.ToCadence(ts, "Struct");
```

## Optionals

Cadence optionals are indicated by appending a ?.  For instance the Cadence type Int16? can contain either an Int16 value or nil.  If the C# type is a reference type, no additional work is required.
For instance a Cadence String? will have the C# equivalent type of String.  This is because the C# String is a reference type, which can natively be set to null.  On the other hand, the Cadence Int16? requires the C# type Int16? which wraps type the value type Int16 in a Nullable<>.

```csharp
//c is a Cadence Int16? type
Int16? i = Convert.FromCadence<Int16?>(c);
```

Trying to convert a Cadence optional into a non-nullable type results in an exception:

```csharp
//c is a Cadence Int16? type
Int16 i = Convert.FromCadence<Int16>(c);
```

"Exception: CadenceOptional requires a nullable type"