using System;

public class pubclass
{
    public static Byte[] ToBytes(Decimal value)

    {

        Int32[] bits = Decimal.GetBits(value);

        Byte[] bytes = new Byte[bits.Length * 4];

        for (Int32 i = 0; i < bits.Length; i++)

        {

            for (Int32 j = 0; j < 4; j++)

            {

                bytes[i * 4 + j] = (Byte)(bits[i] >> (j * 8));

            }

        }

        return bytes;

    }

    /// <summary> 

    /// 将字节数组转换为<c>Decimal</c>对象。 

    /// </summary> 

    /// <param name="array">要转换的字节数组。</param> 

    /// <returns>所转换的<c>Decimal</c>对象。</returns> 

    public static Decimal FromBytes(Byte[] array)

    {

        Int32[] bits = new Int32[array.Length / 4];

        for (Int32 i = 0; i < bits.Length; i++)

        {

            for (Int32 j = 0; j < 4; j++)

            {

                bits[i] |= array[i * 4 + j] << j * 8;

            }

        }

        return new Decimal(bits);

    }
}