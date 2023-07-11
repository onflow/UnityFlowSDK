// Testsuite
access(all) contract TestSuite
{
    init(){}

    access(all) event AddressResult(result: Address);
    access(all) fun TestAddress(value: Address)
    {
        emit AddressResult(result: value);
    }

    access(all) event CharacterResult(result: Character);
    access(all) fun TestCharacter(value: Character)
    {
        emit CharacterResult(result: value);
    }

    access(all) event StringResult(result: String);
    access(all) fun TestString(value: String)
    {
        emit StringResult(result: value);
    }

    access(all) event BoolResult(result: Bool);
    access(all) fun TestBool(value: Bool)
    {
        emit BoolResult(result: value);
    }

    access(all) event OptionalResult(result: String?);
    access(all) fun TestOptional(value: String?)
    {
        emit OptionalResult(result: value);
    }
    
    access(all) event IntResult(result: Int);
    access(all) fun TestInt(value: Int)
    {
        emit IntResult(result: value);
    }
    
    access(all) event UIntResult(result: UInt);
    access(all) fun TestUInt(value: UInt)
    {
        emit UIntResult(result: value);
    }

	access(all) event Int8Result(result: Int8);
    access(all) fun TestInt8(value: Int8)
    {
        emit Int8Result(result: value);
    }
        
	access(all) event UInt8Result(result: UInt8);
    access(all) fun TestUInt8(value: UInt8)
    {
        emit UInt8Result(result: value);
    }
        
	access(all) event Int16Result(result: Int16);
    access(all) fun TestInt16(value: Int16)
    {
        emit Int16Result(result: value);
    }
        
	access(all) event UInt16Result(result: UInt16);
    access(all) fun TestUInt16(value: UInt16)
    {
        emit UInt16Result(result: value);
    }
        
	access(all) event Int32Result(result: Int32);
    access(all) fun TestInt32(value: Int32)
    {
        emit Int32Result(result: value);
    }
        
	access(all) event UInt32Result(result: UInt32);
    access(all) fun TestUInt32(value: UInt32)
    {
        emit UInt32Result(result: value);
    }
    
	access(all) event Int64Result(result: Int64);
    access(all) fun TestInt64(value: Int64)
    {
        emit Int64Result(result: value);
    }
        
	access(all) event UInt64Result(result: UInt64);
    access(all) fun TestUInt64(value: UInt64)
    {
        emit UInt64Result(result: value);
    }
        
	access(all) event Int128Result(result: Int128);
    access(all) fun TestInt128(value: Int128)
    {
        emit Int128Result(result: value);
    }
        
	access(all) event UInt128Result(result: UInt128);
    access(all) fun TestUInt128(value: UInt128)
    {
        emit UInt128Result(result: value);
    }
        
	access(all) event Int256Result(result: Int256);
    access(all) fun TestInt256(value: Int256)
    {
        emit Int256Result(result: value);
    }
        
	access(all) event UInt256Result(result: UInt256);
    access(all) fun TestUInt256(value: UInt256)
    {
        emit UInt256Result(result: value);
    }
        
	access(all) event Word8Result(result: Word8);
    access(all) fun TestWord8(value: Word8)
    {
        emit Word8Result(result: value);
    }
        
	access(all) event Word16Result(result: Word16);
    access(all) fun TestWord16(value: Word16)
    {
        emit Word16Result(result: value);
    }
            
	access(all) event Word32Result(result: Word32);
    access(all) fun TestWord32(value: Word32)
    {
        emit Word32Result(result: value);
    }
            
	access(all) event Word64Result(result: Word64);
    access(all) fun TestWord64(value: Word64)
    {
        emit Word64Result(result: value);
    }
            
	access(all) event Fix64Result(result: Fix64);
    access(all) fun TestFix64(value: Fix64)
    {
        emit Fix64Result(result: value);
    }
            
	access(all) event UFix64Result(result: UFix64);
    access(all) fun TestUFix64(value: UFix64)
    {
        emit UFix64Result(result: value);
    }
      
	access(all) event ArrayResult(result: [String]);
    access(all) fun TestArray(value: [String])
    {
        emit ArrayResult(result: value);
    }
     
	access(all) event FixedArrayResult(result: [String; 4]);
    access(all) fun TestFixedArray(value: [String; 4])
    {
        emit FixedArrayResult(result: value);
    }

	access(all) event DictionaryResult(result: {String:String});
    access(all) fun TestDictionary(value: {String:String})
    {
        emit DictionaryResult(result: value);
    }

    access(all) struct ExampleStruct
    {
        access(all) var string : String
        access(all) var int : Int

        init(string: String, int: Int)
        {
            self.string = string
            self.int = int
        }
    }
    access(all) event StructResult(result: ExampleStruct);
    access(all) fun TestStruct(value: ExampleStruct)
    {
        emit StructResult(result: value);
    }

    access(all) enum ExampleEnum : UInt8
    {
        access(all) case one
        access(all) case two
        access(all) case three
    }
    access(all) fun TestEnum(value: ExampleEnum)
    {
        let int:UInt8  = value.rawValue
        emit UInt8Result(result: int);  // cannot emit an enum
    }

    access(all) event PathResult(result: Path);
    access(all) fun TestPath(value: Path)
    {
        emit PathResult(result: value);
    }

    access(all) event TypeResult(result: Type);
    access(all) fun TestType(value: Type)
    {
        emit TypeResult(result: value);
    }

    access(all) resource TestResource
    {
        access(all) let data:String

        init(data:String)
        {
            self.data = data
        }
    }
    access(all) fun TestCapability(value: Capability)
    {
        emit AddressResult(result: value.address);
    }
}