using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MGNBIO.Common
{
    /// <summary>
    /// C# 实现byte 与float 之间的转换
    /// </summary>
    public static class FloatToByte
    {
        //float 数据变为 byte 数组
        private static byte[] ToByte(float data)
        {
            unsafe
            {
                byte* pdata = (byte*)&data;
                byte[] byteArray = new byte[sizeof(float)];
                for (int i = 0; i < sizeof(float); ++i)
                    byteArray[i] = *pdata++;
                return byteArray;
            }
        }
        //byte数组变为 float数值
        private static float ToFloat(byte[] data)
        {
            unsafe
            {
                float a = 0.0F;
                byte i;
                byte[] x = data;
                void* pf;
                fixed (byte* px = x)
                {
                    pf = &a;
                    for (i = 0; i < data.Length; i++)
                    {
                        *((byte*)pf + i) = *(px + i);
                    }
                }
                return a;
            }
        }
        //float数组变为byte 数组
        public static byte[] ConvertToByteArray(float[] srcFloat)
        {
            unsafe
            {
                int srcLen = srcFloat.Length;
                int FLOATLEN = sizeof(float);
                byte[] dstByte = new byte[FLOATLEN * srcLen];
                int index = 0;
                for (int i = 0; i < srcLen; i++)
                {
                    float temp = srcFloat[i];
                    byte* pdata = (byte*)&temp;

                    //byte[] byteArray = new byte[FLOATLEN];
                    index = i * FLOATLEN;
                    for (int j = 0; j < FLOATLEN; j++)
                    {
                        dstByte[index] = *pdata++;
                        ++index;
                    }
                }
                return dstByte;
            }
        }
        //byte 数组变为float 数组
        public static float[] ConvertToFloatArray(byte[] srcByte)
        {
            unsafe
            {
                int FLOATLEN = sizeof(float);
                int srcLen = srcByte.Length;
                int dstLen = srcLen / FLOATLEN;
                float[] dstFloat = new float[dstLen];
                for (int i = 0; i < dstLen; i++)
                {
                    float temp = 0.0F;
                    void* pf = &temp;
                    fixed (byte* pxb = srcByte)
                    {
                        byte* px = pxb;
                        px += i * FLOATLEN;

                        for (int j = 0; j < FLOATLEN; j++)
                        {
                            *((byte*)pf + j) = *(px + j);
                        }
                        dstFloat[i] = temp;
                    }
                }
                return dstFloat;
            }
        }
        //byte 数组变为float 数组
        public static float?[] ConvertToNullAbleFloatArray(byte[] srcByte)
        {
            unsafe
            {
                int FLOATLEN = sizeof(float);
                int srcLen = srcByte.Length;
                int dstLen = srcLen / FLOATLEN;
                float?[] dstFloat = new float?[dstLen];
                for (int i = 0; i < dstLen; i++)
                {
                    float temp = 0.0F;
                    void* pf = &temp;
                    fixed (byte* pxb = srcByte)
                    {
                        byte* px = pxb;
                        px += i * FLOATLEN;

                        for (int j = 0; j < FLOATLEN; j++)
                        {
                            *((byte*)pf + j) = *(px + j);
                        }
                        dstFloat[i] = temp;
                    }
                }
                return dstFloat;
            }
        }

    }
}
