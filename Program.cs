using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.ComponentModel.Design;
using System.Timers;

namespace ConsoleRS485
{
    internal class Program
    {
        static SerialPort mySerialPort;
        static Timer timer;
        static void Main(string[] args)
        {
            
            CanManager canManager = new CanManager();

            canManager.ListChannels();
            canManager.OpenChannel(0);
            canManager.SetBusParams();
            canManager.GoOnBus();
            byte[] message = { 0, 0, 0, 0, 0, 0, 0, 0 };
            int cnt = 0;

            RS485Manager manager = new RS485Manager("COM7", 115200);
            manager.Start();

            Console.WriteLine("Reading RS485 data. Press 'q' to exit...");

            Timer printTimer = new Timer(100); // 1 second
            printTimer.Elapsed += (sender, e) =>
            {
                cnt++;
                if (cnt > 254) cnt = 0;
                Console.Clear();

                Console.WriteLine($"raw_l: {manager.getRAW_low():X2}, raw_H: {manager.getRAW_HIGH():X2}");
                Console.WriteLine("");
                Console.WriteLine($"rez14: {manager.getREZ14()}");
                Console.WriteLine($"rez14_l: {manager.getREZ14_low():X2}, rez14_H: {manager.getREZ14_HIGH():X2}");

                Console.WriteLine($"rez12: {manager.getREZ12()} ");

                Console.WriteLine($"12l: {manager.getREZ12_low():X2}, rez12_H: {manager.getREZ12_HIGH():X2}");
                message[0] = manager.getREZ12_low();
                message[1] = manager.getREZ12_HIGH();
                message[2] = manager.getREZ14_low();
                message[3] = manager.getREZ14_HIGH();
                message[6] = (byte)cnt;
                message[7] = (byte) (cnt/2);

                canManager.SendMessage(0x18FFFA00, message);


                byte low = manager.getRAW_low();// 0xE4;
                byte hig = (byte)(manager.getRAW_HIGH() & 0xF3);// 0x39;

                int full = (int)(hig << 8) | low; ;// 0x39E4;
                int twelvbit = full >> 2;
                Console.WriteLine($"twelvbit: {twelvbit:X2}, dec {twelvbit}");

            };
            printTimer.Start();

            while (true)
            {
  

                if (Console.ReadKey().KeyChar == 'q')
                {
                    break;
                }
            }

            printTimer.Stop();
            manager.Stop();
            Console.WriteLine("Serial port closed. Exiting...");


            canManager.GoOffBus();
            canManager.CloseChannel();

            //RS485Manager manager = new RS485Manager("COM7", 2000000);
            //manager.Start();

            //Console.WriteLine("Reading RS485 data. Press 'q' to exit...");

            //while (true)
            //{
            //    Console.Clear();
            //    Console.WriteLine(manager.getREZ14());
            //    Console.WriteLine(manager.getREZ12());
            //    if (Console.ReadKey().KeyChar == 'q')
            //    {
            //        break;
            //    }
            //}

            //manager.Stop();
            //Console.WriteLine("Serial port closed. Exiting...");


        }
 
    }
}
