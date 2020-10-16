using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Lab2
{
    public class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("Hello World!");
            Encoding enc = Encoding.Default;
            string Test = "The quick brown fox jumps over the lazy dog";
            byte[] rest = enc.GetBytes(Test);
            byte[] testHash = SHA256.getHash(rest);
            byte[] testHash2 = SHA256.getHash(rest);
            string str = BitConverter.ToString(testHash);
            string str2 = BitConverter.ToString(testHash2);
            Console.WriteLine(str);
            Console.WriteLine(str2);
        }
    }

    public class SHA256
    {
        private static UInt32[] H = new UInt32[8]
        {//Инициализация переменных (первые 32 бита дробных частей квадратных корней первых восьми простых чисел[от 2 до 19])
            0x6A09E667, 0xBB67AE85, 0x3C6EF372, 0xA54FF53A, 0x510E527F, 0x9B05688C, 0x1F83D9AB, 0x5BE0CD19
        };

        private static UInt32[] HH = new UInt32[8];
        private static readonly UInt32[] K = new UInt32[64]
        { //Таблица констант (первые 32 бита дробных частей кубических корней первых 64 простых чисел[от 2 до 311])
            0x428A2F98, 0x71374491, 0xB5C0FBCF, 0xE9B5DBA5, 0x3956C25B, 0x59F111F1, 0x923F82A4, 0xAB1C5ED5,
            0xD807AA98, 0x12835B01, 0x243185BE, 0x550C7DC3, 0x72BE5D74, 0x80DEB1FE, 0x9BDC06A7, 0xC19BF174,
            0xE49B69C1, 0xEFBE4786, 0x0FC19DC6, 0x240CA1CC, 0x2DE92C6F, 0x4A7484AA, 0x5CB0A9DC, 0x76F988DA,
            0x983E5152, 0xA831C66D, 0xB00327C8, 0xBF597FC7, 0xC6E00BF3, 0xD5A79147, 0x06CA6351, 0x14292967,
            0x27B70A85, 0x2E1B2138, 0x4D2C6DFC, 0x53380D13, 0x650A7354, 0x766A0ABB, 0x81C2C92E, 0x92722C85,
            0xA2BFE8A1, 0xA81A664B, 0xC24B8B70, 0xC76C51A3, 0xD192E819, 0xD6990624, 0xF40E3585, 0x106AA070,
            0x19A4C116, 0x1E376C08, 0x2748774C, 0x34B0BCB5, 0x391C0CB3, 0x4ED8AA4A, 0x5B9CCA4F, 0x682E6FF3,
            0x748F82EE, 0x78A5636F, 0x84C87814, 0x8CC70208, 0x90BEFFFA, 0xA4506CEB, 0xBEF9A3F7, 0xC67178F2
        };
        private static UInt32[] W = new UInt32[64];
        private static UInt32[] TEMP = new UInt32[8];
        /*a := h0 
        b := h1
        c := h2
        d := h3
        e := h4
        f := h5
        g := h6
        h := h7*/

        private static UInt32 ROTR(UInt32 x, byte n)
        { //циклический сдвиг вправо
            return (x >> n) | (x << (32 - n));
        }

        private static UInt32 BIGSigma0(UInt32 x)
        {
            return ROTR(x, 2) ^ ROTR(x, 13) ^ ROTR(x, 22);
        }

        private static UInt32 BIGSigma1(UInt32 x)
        {
            return ROTR(x, 6) ^ ROTR(x, 11) ^ ROTR(x, 25);
        }

        private static UInt32 smallSigma0(UInt32 x)
        {
            return ROTR(x, 7) ^ ROTR(x, 18) ^ (x >> 3);
        }

        private static UInt32 smallSigma1(UInt32 x)
        {
            return ROTR(x, 17) ^ ROTR(x, 19) ^ (x >> 10);
        }

        private static UInt32 Ma(UInt32 x, UInt32 y, UInt32 z)
        {
            return (x & y) ^ (x & z) ^ (y & z);
        }

        private static UInt32 Ch(UInt32 x, UInt32 y, UInt32 z)
        {
            return (x & y) ^ ((~x) & z);
        }

        private static byte[] MessageExtension(byte[] OriginMessage)
        {//расширяем сообщение до необходимого размера
            int NewLength = OriginMessage.Length + 9;
            int ZeroBytes = 64 - (NewLength % 64);
            NewLength += ZeroBytes;
            byte[] NewMessage = new byte[NewLength];
            Array.Copy(OriginMessage, 0, NewMessage, 0, OriginMessage.Length);
            NewMessage[OriginMessage.Length] = (byte)0b10000000;//записываем 1 бит
            UInt64 OldLength = (ulong)(OriginMessage.Length);
            byte[] OldLengthInBytes = BitConverter.GetBytes(OldLength * 8);
            Array.Reverse(OldLengthInBytes);
            Array.Copy(OldLengthInBytes, 0, NewMessage, (NewMessage.Length - 8), OldLengthInBytes.Length);
            return NewMessage;
        }
        private static UInt32[] toUintArray(byte[] source)
        {
            UInt32[] destination = new UInt32[source.Length / 4];
            for (uint i = 0, j = 0; i < destination.Length; ++i, j += 4)
            {
                destination[i] = ((UInt32)source[j + 0] << 24) | ((UInt32)source[j + 1] << 16) | ((UInt32)source[j + 2] << 8) | ((UInt32)source[j + 3]);
            }
            return destination;
        }
        private static byte[] toByteArray(UInt32[] sourse)
        {
            byte[] destination = new byte[sourse.Length * 4];
            int pos = 0;

            for (int i = 0; i < sourse.Length; ++i)
            {
                destination[pos++] = (byte)(sourse[i] >> 24);
                destination[pos++] = (byte)(sourse[i] >> 16);
                destination[pos++] = (byte)(sourse[i] >> 8);
                destination[pos++] = (byte)(sourse[i]);
            }

            return destination;
        }
        public static byte[] getHash(byte[] message)
        {
            byte[] ExMessageByte = MessageExtension(message);
            UInt32[] ExMessageInt = toUintArray(ExMessageByte);
            Array.Copy(H, HH, 8);
            //Далее сообщение обрабатывается последовательными порциями по 512 бит: разбить сообщение на куски по 512 бит
            for (int i = 0; i < (ExMessageInt.Length / 16); i++) //для каждого куска: разбить кусок на 16 слов длиной 32 бита
            {
                Array.Copy(ExMessageInt, i * 16, W, 0, 16);
                for (int t = 16; t < 64; ++t) //Сгенерировать дополнительные 48 слов:
                {
                    W[t] = smallSigma1(W[t - 2]) + W[t - 7] + smallSigma0(W[t - 15]) + W[t - 16];
                }
                Array.Copy(HH, 0, TEMP, 0, HH.Length);
                /*a := h0 
                b := h1
                c := h2
                d := h3
                e := h4
                f := h5
                g := h6
                h := h7*/
                for (int t = 0; t < 64; ++t)
                {

                    //Σ0 := (a rotr 2) xor (a rotr 13) xor (a rotr 22)
                    //Ma := (a and b) xor (a and c) xor (b and c)
                    UInt32 t2 = BIGSigma0(TEMP[0]) + Ma(TEMP[0], TEMP[1], TEMP[2]);//t2 := Σ0 + Ma
                    //Σ1:= (e rotr 6) xor(e rotr 11) xor(e rotr 25)
                    //Ch:= (e and f) xor((not e) and g)
                    UInt32 t1 = TEMP[7] + BIGSigma1(TEMP[4]) + Ch(TEMP[4], TEMP[5], TEMP[6]) + K[t] + W[t];//t1 := h + Σ1 + Ch + k[i] + w[i]

                    Array.Copy(TEMP, 0, TEMP, 1, TEMP.Length - 1);//сдвигаем TEMP
                    TEMP[4] += t1; //e := d + t1
                    TEMP[0] = t1 + t2;// a := t1 + t2
                    /*h := g
                      g := f
                      f := e
                      e := d + t1
                      d := c
                      c := b
                      b := a
                      a := t1 + t2*/
                }

                // add values in TEMP to values in H
                for (int t = 0; t < HH.Length; ++t)
                {
                    HH[t] += TEMP[t];
                }
                //Добавить полученные значения к ранее вычисленному результату:
                /*h0:= h0 + a
                h1:= h1 + b
                h2:= h2 + c
                h3:= h3 + d
                h4:= h4 + e
                h5:= h5 + f
                h6:= h6 + g
                h7:= h7 + h*/
            }
            return toByteArray(HH);
        }

    }
}
