using System;
using System.Numerics;

namespace mud
{
    public static partial class ProtocolParser
    {
        public static T UnsupportedStaticField<T>(T fieldType) where T : Enum
        {
            string fieldName = Enum.GetName(typeof(T), fieldType) ?? fieldType.ToString();
            throw new InvalidOperationException($"Unsupported static field type: {fieldName}");
        }

        public static T UnsupportedDynamicField<T>(T fieldType) where T : Enum
        {
            string fieldName = Enum.GetName(typeof(T), fieldType) ?? fieldType.ToString();
            throw new InvalidOperationException($"Unsupported dynamic field type: {fieldName}");
        }

        public static Exception InvalidHexLengthError(string value)
        {
            throw new InvalidOperationException(
                $"Hex value {value} is an odd length ({value.Length - 2}). It must be an even length.");
        }

        public static Exception InvalidHexLengthForStaticFieldError(SchemaAbiTypes.SchemaType abiType, string value)
        {
            throw new InvalidOperationException(
                $"Hex value {value} has length of {value.Length - 2}, but expected length of {SchemaAbiTypes.GetStaticByteLength[abiType] * 2} for {abiType.ToString()} type."
            );
        }

        public static Exception InvalidHexLengthForPackedCounterError(string value)
        {
            throw new InvalidOperationException(
                $"Hex value {value} has length of {value.Length - 2}, but expected length of 64 for a packed counter.");
        }
        
        public static Exception InvalidHexLengthForSchemaError(string value)
        {
            throw new InvalidOperationException(
                $"Hex value {value} has length of {value.Length - 2}, but expected length of 64 for a schema.");
        }

        public static Exception InvalidHexLengthForArrayFieldError(SchemaAbiTypes.SchemaType abiType, string value)
        {
            throw new InvalidOperationException(
                $"Hex value {value} has length of {value.Length - 2}, but expected a multiple of ${SchemaAbiTypes.GetStaticByteLength[abiType] * 2} for ${abiType.ToString()}[] type.");
        }

        public static Exception PackedCounterLengthMismatchError(string packedCounterData, BigInteger definedLength,
            BigInteger summedLength)
        {
            throw new InvalidOperationException(
                $"PackedCounter {packedCounterData} total bytes length ({definedLength}) did not match the summed length of all field byte lengths ({summedLength}).");
        }
        
        public static Exception SchemaStaticLengthMismatchError(string schemaData, BigInteger definedLength,
            BigInteger summedLength)
        {
            throw new InvalidOperationException(
                $"Schema {schemaData} total bytes length ({definedLength}) did not match the summed length of all field byte lengths ({summedLength}).");
        }
    }
}
