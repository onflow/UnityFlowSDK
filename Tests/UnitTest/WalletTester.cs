using DapperLabs.Flow.Sdk;
using DapperLabs.Flow.Sdk.Cadence;
using DapperLabs.Flow.Sdk.Cadence.Types;
using DapperLabs.Flow.Sdk.Crypto;
using DapperLabs.Flow.Sdk.DataObjects;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Unit Tests for IWallet interfaces.
/// 
/// Usage: 
/// 1) Comment/Uncomment correct lines on LN27-28 to match your target network of choice (emulator / testnet)
///   a.) If using Emulator, deploy contract from folder containing these unit tests to service account.
/// 2) In the Start function, under INITIALISE SDK, comment / uncomment lines as appropriate to init the SDK for EMULATOR or TESTNET 
/// 3) In the Start function, under CREATE WALLET PROVIDER, comment / uncomment or add lines to configure the IWallet interface you wish to test.
/// 4) Open scene 'WalletTester.unity' from the folder containing these tests, and run.
/// </summary>
namespace DapperLabs.Flow.Sdk.Tests
{
    public class WalletTester : MonoBehaviour
    {
        public TMPro.TextMeshProUGUI m_outputPanel;

        public const string CONTRACT_ADDRESS = "06db93b2ca619c6d";    // TESTNET
        //public const string CONTRACT_ADDRESS = "f8d6e0586b0a20c7";    // EMULATOR

        private struct TestData
        {
            public TestData(string name, string functionName, string type, CadenceBase argument, string? fieldToCompare = null)
            {
                Name = name;
                FunctionName = functionName;
                Type = type;
                Argument = argument;
                FieldToCompare = fieldToCompare;
            }
            public string Name;
            public string FunctionName;
            public string Type;
            public CadenceBase Argument;
            public string? FieldToCompare;
        }
        private List<TestData> m_testSuite = new List<TestData>
    {
        new TestData("Address" , "TestAddress" ,"Address", new CadenceAddress("0xde982e6b445b576a")),
        new TestData("Character"  , "TestCharacter"  ,"Character" , new CadenceCharacter('a')),
        new TestData("String"  , "TestString"  ,"String" , new CadenceString("test")),
        new TestData("Bool"    , "TestBool"    ,"Bool"   , new CadenceBool(true)),
        new TestData("Optional(null)", "TestOptional","String?", new CadenceOptional(null)),
        new TestData("Optional(string)", "TestOptional","String?", new CadenceOptional(new CadenceString("optionalTest"))),
        new TestData("Int"     , "TestInt"     ,"Int"    , new CadenceNumber(CadenceNumberType.Int, "-1")),
        new TestData("Int8"    , "TestInt8"    ,"Int8"   , new CadenceNumber(CadenceNumberType.Int8, "-8")),
        new TestData("Int16"   , "TestInt16"   ,"Int16"  , new CadenceNumber(CadenceNumberType.Int16, "-16")),
        new TestData("Int32"   , "TestInt32"   ,"Int32"  , new CadenceNumber(CadenceNumberType.Int32, "-32")),
        new TestData("Int64"   , "TestInt64"   ,"Int64"  , new CadenceNumber(CadenceNumberType.Int64, "-64")),
        new TestData("Int128"  , "TestInt128"  ,"Int128" , new CadenceNumber(CadenceNumberType.Int128, "-128")),
        new TestData("Int256"  , "TestInt256"  ,"Int256" , new CadenceNumber(CadenceNumberType.Int256, "-256")),
        new TestData("UInt"    , "TestUInt"    ,"UInt"   , new CadenceNumber(CadenceNumberType.UInt, "1")),
        new TestData("UInt8"   , "TestUInt8"   ,"UInt8"  , new CadenceNumber(CadenceNumberType.UInt8, "8")),
        new TestData("UInt16"  , "TestUInt16"  ,"UInt16" , new CadenceNumber(CadenceNumberType.UInt16, "16")),
        new TestData("UInt32"  , "TestUInt32"  ,"UInt32" , new CadenceNumber(CadenceNumberType.UInt32, "32")),
        new TestData("UInt64"  , "TestUInt64"  ,"UInt64" , new CadenceNumber(CadenceNumberType.UInt64, "64")),
        new TestData("UInt128" , "TestUInt128" ,"UInt128", new CadenceNumber(CadenceNumberType.UInt128, "128")),
        new TestData("UInt256" , "TestUInt256" ,"UInt256", new CadenceNumber(CadenceNumberType.UInt256, "256")),
        new TestData("Word8"   , "TestWord8"   ,"Word8"  , new CadenceNumber(CadenceNumberType.Word8, "8")),
        new TestData("Word16"  , "TestWord16"  ,"Word16" , new CadenceNumber(CadenceNumberType.Word16, "16")),
        new TestData("Word32"  , "TestWord32"  ,"Word32" , new CadenceNumber(CadenceNumberType.Word32, "32")),
        new TestData("Word64"  , "TestWord64"  ,"Word64" , new CadenceNumber(CadenceNumberType.Word64, "64")),
        new TestData("Fix64"   , "TestFix64"   ,"Fix64"  , new CadenceNumber(CadenceNumberType.Fix64, "-64.00000000")),
        new TestData("UFix64"  , "TestUFix64"  ,"UFix64" , new CadenceNumber(CadenceNumberType.UFix64, "64.00000000")),
        new TestData("Array"   , "TestArray"   ,"[String]"  , Convert.ToCadence(new List<string>{ "a","b","c","d" }, "[String]")),
        new TestData("Path"  , "TestPath"  ,"Path" , new CadencePath(new CadencePathValue("private", "TestPath"))),

        new TestData("Struct"  , "TestStruct"  ,"TestSuite.ExampleStruct" , Convert.ToCadence(new ExampleStruct{ String = "test", Int = -12 }, "Struct")),
        new TestData("Fixed Array"   , "TestFixedArray"   ,"[String; 4]"  , Convert.ToCadence(new List<string>{ "a","b","c","d" }, "[String; 4]")),
        new TestData("Dictionary"   , "TestDictionary"   ,"{String:String}" , Convert.ToCadence(new Dictionary<string, string>{{"keyA", "valueA"},{"keyB", "valueB"},{"keyC", "valueC"},{"keyD", "valueD"},}, "{String:String}")),
        new TestData("Enum"  , "TestEnum"  ,"TestSuite.ExampleEnum" , Convert.ToCadence(ExampleEnum.one, "Enum"), "rawValue"),
        new TestData("Type"  , "TestType"  ,"Type" , new CadenceType(new CadenceTypeValue { StaticType = new CadenceTypeBase { Kind = "Int" } })),
        new TestData("Capability"  , "TestCapability"  ,"Capability" , new CadenceCapability(new CadenceCapabilityValue {
            Address = "0x" + CONTRACT_ADDRESS,
            Path = new CadencePath(new CadencePathValue("public", "TestPath")),
            BorrowType = new CadenceReferenceType(false, new CadenceTypeBase { Kind = "Int" } )
        }), "address"),

        // UNSUPPORTED - new TestData("Event"  , ""  ,"Event" , new CadenceBase()),
        // UNSUPPORTED - new TestData("Resource"  , ""  ,"Resource" , new CadenceBase()),
        // UNSUPPORTED - new TestData("Link"  , ""  ,"Link" , new CadenceBase()),
        // UNSUPPORTED - new TestData("Contract"  , ""  ,"Contract" , new CadenceBase()),
    };

        private const string m_transactionTemplate = @"
import TestSuite from CONTRACT_ADDRESS
transaction (parameter: PARAMTYPE)
{
    prepare(acct: AuthAccount) {}
    execute 
    {
    TestSuite.FUNCTION(value: parameter)
    }
}";

        public static bool _running = true;

        // testing structs
        private const string struct_identifier = "A." + CONTRACT_ADDRESS + ".TestSuite.ExampleStruct";
        [Cadence(CadenceType = struct_identifier)]
        private struct ExampleStruct
        {
            [Cadence(CadenceType = "String", Name = "string")]
            public string String;
            [Cadence(CadenceType = "Int", Name = "int")]
            public BigInteger Int;
        }

        private const string enum_identifier = "A." + CONTRACT_ADDRESS + ".TestSuite.ExampleStruct";
        [Cadence(CadenceType = enum_identifier)]
        public enum ExampleEnum : byte
        {
            one,
            two,
            three
        }

        // Start is called before the first frame update
        async void Start()
        {
            m_outputPanel.text = "Initializing...\n";

            // INITIALISE SDK
            // -- EMULATOR
            // FlowConfig flowConfig = new FlowConfig();

            // -- TESTNET
            FlowConfig flowConfig = new FlowConfig() // - TESTNET
            {
                NetworkUrl = "https://rest-testnet.onflow.org/v1",  // testnet
                Protocol = FlowConfig.NetworkProtocol.HTTP
            };

            // INITIALISE SDK
            FlowSDK.Init(flowConfig);


            // CREATE WALLET PROVIDER

            // -- EMULATOR
            // IWallet walletProvider = new DapperLabs.Flow.Sdk.DevWallet.DevWalletProvider();
            // walletProvider.Init(null);

            // -- WALLET CONNECT
            //IWallet walletProvider = new DapperLabs.Flow.Sdk.WalletConnect.WalletConnectProvider();
            //walletProvider.Init(new DapperLabs.Flow.Sdk.WalletConnect.WalletConnectConfig
            //{
            //    ProjectId = "fb087df84af28bc20669151a5efb3ff7", // insert Project ID from Wallet Connect dashboard
            //    ProjectDescription = "Flow SDK Wallet Unit Tests",
            //    ProjectIconUrl = "https://walletconnect.com/meta/favicon.ico",
            //    ProjectName = "Wallet Tester",
            //    ProjectUrl = "https://dapperlabs.com"
            //});

            // -- NIFTORY
            IWallet walletProvider = new DapperLabs.Flow.Sdk.Niftory.NiftoryProvider(); // - TESTNET
            walletProvider.Init(new DapperLabs.Flow.Sdk.Niftory.NiftoryConfig
            {
                ClientId = "clhgw0ggz0000lg0v4q6xdd62",
                AuthUrl = "https://auth.staging.niftory.com",
                GraphQLUrl = "https://graphql.api.staging.niftory.com"
            });

            // REGISTER wallet provider with SDK
            FlowSDK.RegisterWalletProvider(walletProvider);

            // run suite
            m_outputPanel.text += "Running Test Suite...\n";
            await RunTestSuite();
        }

        private async Task RunTestSuite()
        {
            // sign in
            m_outputPanel.text += "Authenticating...\n";
            if (FlowSDK.GetWalletProvider().IsAuthenticated() == false)
            {
                await FlowSDK.GetWalletProvider().Authenticate("", // blank string will show list of accounts from Accounts tab of Flow Control Window
                                                        (string address) => { m_outputPanel.text += $"Authenticated. Wallet Address: {address}\n"; },
                                                        () => { m_outputPanel.text += "Authentication Failed.\n"; });
            }

            // start data tests
            foreach (var test in m_testSuite)
            {
                if (_running == false)
                {
                    break;
                }

                await SubmitTest(test);
            }

            // de-authenticate
            m_outputPanel.text += "UnAuthenticating...\n";
            FlowSDK.GetWalletProvider().Unauthenticate();

            m_outputPanel.text += "Done.\n";
        }

        private async Task SubmitTest(TestData test)
        {
            try
            {
                m_outputPanel.text += $"Testing: {test.Name}";

                string script = m_transactionTemplate.Replace("CONTRACT_ADDRESS", "0x" + CONTRACT_ADDRESS).Replace("PARAMTYPE", test.Type).Replace("FUNCTION", test.FunctionName);

                FlowTransactionResult result = await Transactions.SubmitAndWaitUntilExecuted((string id) => { m_outputPanel.text += $" - Transaction Id: {id}"; }, script, test.Argument);

                // check for error.
                if (result.Error != null || result.ErrorMessage != string.Empty || result.Status == FlowTransactionStatus.EXPIRED)
                {
                    m_outputPanel.text += " - <color=red>FAILED.</color>\n";
                    Debug.LogError(result.Error?.Message);
                    Debug.LogError(result.ErrorMessage);
                    return;
                }

                // get events
                FlowEvent testEvent = result.Events.Find(x => x.Type.EndsWith("Result"));
                CadenceComposite testComposite = (CadenceComposite)testEvent.Payload;
                CadenceBase testResult = testComposite.CompositeFieldAs<CadenceBase>("result");

                if (testResult != null)
                {
                    string resultValue = testResult.GetValue();
                    string argumentValue = test.Argument.GetValue();

                    if (test.FieldToCompare != null)
                    {
                        // this area is for unit tests where the argument passed in cannot be emitted as an event
                        switch (test.Type)
                        {
                            case "Capability":  // hardcode the address for capability one
                                argumentValue = ((CadenceCapability)test.Argument).Value.Address;
                                break;
                            default:
                                argumentValue = ((CadenceComposite)test.Argument).CompositeFieldAs<CadenceBase>(test.FieldToCompare).GetValue();
                                break;
                        }
                    }

                    if (resultValue == argumentValue)
                    {
                        m_outputPanel.text += " - <color=#005500>SUCCESS.</color>\n";
                    }
                    else
                    {
                        m_outputPanel.text += " - <color=red>FAILED.</color>\n";
                    }
                }
                else
                {
                    m_outputPanel.text += " - <color=red>FAILED.</color>\n";
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private void OnDestroy()
        {
            _running = false;
        }
    }
}