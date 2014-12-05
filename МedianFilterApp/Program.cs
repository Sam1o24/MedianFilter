using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;

namespace МedianFilterApp
{
    class Program
    {
        static void Main(string[] args)
        {
           
            //Bitmap bmp = new Bitmap("test.bmp"); //входной файл
            Bitmap bmp = new Bitmap(args[0]);

            int PCount = Environment.ProcessorCount; //количество "процессоров"

            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),ImageLockMode.ReadWrite,bmp.PixelFormat);

            int part = Convert.ToInt32(bd.Stride / PCount * bd.Height); //размер одной части

            int width = Convert.ToInt32(bd.Width / PCount);     //ширина одной части
            int height = bd.Height;                             //высота одной части

            Part[] m_part = new Part[PCount];

            //заполнение частей
            for (int i = 0; i < PCount; i++)
            {
                m_part[i] = new Part(i, width, height);
                m_part[i].Buff = new byte[part];
                Marshal.Copy(bd.Scan0 + i * m_part[i].Buff.Length, m_part[i].Buff, 0, m_part[i].Buff.Length);
            }

            //Parallel.ForEach<byte[]>(buffer, item => ProcessingPart(item, with, bd.Height));

            Thread[] m_th = new Thread[PCount];

            //создание потоков
            for (int i = 0; i < PCount; i++)
            {
                m_th[i] = new Thread(m_part[i].ProcessingPart);
            }

            //запуск потоков
            for (int i = 0; i < PCount; i++)
            {
                m_th[i].Start();
            }

            //ожидание результатов
            for (int i = 0; i < PCount; i++)
            {
                //m_th[i].Join();
            }

            for (int i = 0; i < PCount; i++)
            {
                Marshal.Copy(m_part[i].Buff, 0, bd.Scan0 + i * m_part[i].Buff.Length, m_part[i].Buff.Length);   //сбор частей в единую картинку
            }
                bmp.UnlockBits(bd);
                bmp.Save(args[1]);
                //bmp.Save("test1.bmp");
                bmp.Dispose();
        }




        
    }
}
