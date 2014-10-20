using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace МedianFilterApp
{
    class Program
    {
        static void Main(string[] args)
        {
           
            //Bitmap bmp = new Bitmap("test.bmp");
            Bitmap bmp = new Bitmap(args[0]);

            int PCount = Environment.ProcessorCount;

            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),ImageLockMode.ReadWrite,bmp.PixelFormat);

            int part = Convert.ToInt32(bd.Stride / PCount * bd.Height);

            byte[][] buffer = new byte[PCount][];
            for (int i = 0; i <  PCount; i++)
            {
                buffer[i] = new byte[part];
                Marshal.Copy(bd.Scan0 + i * buffer[i].Length, buffer[i], 0, buffer[i].Length);
            }

            int with = Convert.ToInt32(bd.Width / PCount);

            Parallel.ForEach<byte[]>(buffer, item => ProcessingPart(item, with, bd.Height));

            for (int i = 0; i < PCount; i++)
            {
                Marshal.Copy(buffer[i], 0, bd.Scan0 + i * buffer[i].Length, buffer[i].Length);
            }
                bmp.UnlockBits(bd);
                bmp.Save(args[1]);
                //bmp.Save("test1.bmp");
                bmp.Dispose();
        }

        static byte[] ProcessingPart(byte[] buffer, int width, int height)
        {
            for (int k = 0; k < 3; k++)
            {
                ChangePixColor(ref buffer, width, height,k);
            }

            return buffer;
        }

        static void ChangePixColor(ref byte[] buffer, int width, int height, int k)
        {
            int Stride = width * 3;

            byte[] temp = new byte[9];
            byte[] buffer_copy = new byte[buffer.Length];
            Array.Copy(buffer, buffer_copy, buffer.Length);

            for (int i = 1; i < width - 1; i++)
            {
                for (int j = 1; j < height - 1; j++)
                {
                    temp[0] = buffer[Stride * (j - 1) + 3 * (i - 1) + k];
                    temp[1] = buffer[Stride * (j - 1) + 3 * i + k];
                    temp[2] = buffer[Stride * (j - 1) + 3 * (i + 1) + k];

                    temp[3] = buffer[Stride * j + 3 * (i - 1) + k];
                    temp[4] = buffer[Stride * j + 3 * i + k];
                    temp[5] = buffer[Stride * j + 3 * (i + 1) + k];

                    temp[6] = buffer[Stride * (j + 1) + 3 * (i - 1) + k];
                    temp[7] = buffer[Stride * (j + 1) + 3 * i + k];
                    temp[8] = buffer[Stride * (j + 1) + 3 * (i + 1) + k];

                    Array.Sort(temp);

                    buffer_copy[Stride * j + 3 * i + k] = temp[4];
                }
            }
            Array.Copy(buffer_copy, buffer, buffer_copy.Length);
        }
        
    }
}
