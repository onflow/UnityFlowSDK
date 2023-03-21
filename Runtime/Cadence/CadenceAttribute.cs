using System;

namespace DapperLabs.Flow.Sdk.Cadence
{
    /// <summary>
    /// Annotating your classes/structs with CadenceAttributes allows CadenceConvert to convert your C# datatypes to
    /// the correct CadenceBase types.
    /// By default, CadenceConvert will use the field names of your class.  If the cadence field name is different, you
    /// can use the name property of the Cadence attribute to change it.
    /// The CadenceType property is mandatory when converting from C# classes/structs to Cadence.
    /// </summary>
    /// </example>
    /// <example>
    /// <code>
    /// [Cadence(CadenceType = "A.XXX.ContractName.StructName")]
    /// public class TestClass
    /// {
    ///    [Cadence(CadenceType = "String", Name = "theString")]
    ///    public String s;
    ///    [Cadence(CadenceType = "[Int16]?")]
    ///    public List&lt;Int16&gt; li;
    ///}
    /// </code>
    /// Where XXX is the address (without a leading 0x) of the account that contains the contract.
    /// The String s in this struct will become a cadence string with the name "theString" when converted from C# to cadence
    /// and the property theString of passed cadence will be placed in the s variable when converting from cadence to C#
    /// </example>
    [AttributeUsage(AttributeTargets.All)]
    public sealed class CadenceAttribute : Attribute
    {
        private string name = null;

        private string cadenceType = null;

        /// <summary>
        /// The name of the field in the cadence struct that this field will convert from/to.
        /// </summary>
        public string Name
        {
            get => name;
            set => name = value;
        }

        /// <summary>
        /// The cadence type that this field will be converted into.  This property is mandatory.
        /// </summary>
        public string CadenceType
        {
            get => cadenceType;
            set => cadenceType = value;
        }
    }
}