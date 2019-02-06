// Original code author: Tim Haynes, May 2006.  
// Use freely as you see fit.
// Copied from codeproject.com many moons ago and modified slightly.
namespace MyBudgetExplorer.Models.BinarySerialization
{
    internal enum ObjectType : byte
    {
        nullType,
        boolType,
        byteType,
        uint16Type,
        uint32Type,
        uint64Type,
        sbyteType,
        int16Type,
        int32Type,
        int64Type,
        charType,
        stringType,
        singleType,
        doubleType,
        decimalType,
        dateTimeType,
        byteArrayType,
        charArrayType,
        otherType
    }
}
