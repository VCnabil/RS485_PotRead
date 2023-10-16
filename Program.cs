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

            //byte b0 = 0xE4;
            //byte b2 = 0xF9;
            //byte b1 = (byte)(b2 & 0x3F);
            //int rez14 = (b1 << 8) | b0;
            //Console.WriteLine("14bit");
            //Console.WriteLine(rez14);
            //Console.WriteLine("12bit");
            //int rez12 = rez14 >> 2;
            //Console.WriteLine(rez12);

            //return;

            mySerialPort = new SerialPort("COM3");

            mySerialPort.BaudRate = 115200; 
            mySerialPort.Parity = Parity.None;
            mySerialPort.StopBits = StopBits.One;
            mySerialPort.DataBits = 8;
            mySerialPort.Handshake = Handshake.None;
           // mySerialPort.ReceivedBytesThreshold = 1;

            mySerialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);

            mySerialPort.Open();

            // Set up a timer to send the read position command every 500 ms
            timer = new Timer(800);
            timer.Elapsed += new ElapsedEventHandler(SendCommand);
            timer.Start();

            Console.WriteLine("Reading RS485 data. Press 'q' to exit...");

            while (true)
            {
                if (Console.ReadKey().KeyChar == 'q')
                {
                    break;
                }
            }

            timer.Stop();
            mySerialPort.Close();
            Console.WriteLine("Serial port closed. Exiting...");
        }
        private static void SendCommand(object sender, ElapsedEventArgs e)
        {
            // Send the read position command (0x54)
            byte[] command = new byte[] { 0x54 };
            mySerialPort.Write(command, 0, command.Length);
        }
        private static int ReverseBits(byte b)
        {
            int rev = 0;
            for (int i = 0; i < 8; i++)
            {
                rev <<= 1;
                rev |= (b & 1);
                b >>= 1;
            }
            return rev;
        }
        private static void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            int bytesToRead = sp.BytesToRead;
            byte[] receivedData = new byte[bytesToRead];
            sp.Read(receivedData, 0, bytesToRead);

            // Debug: Show the number of bytes received
            Console.WriteLine($"Bytes received: {bytesToRead}");

            // Debug: Show the received bytes in hexadecimal
            Console.Write("Received bytes: ");
            foreach (byte b in receivedData)
            {
                Console.Write($"{b:X2} ");
            }
            Console.WriteLine();

            // Assuming the received data is at least 2 bytes
            if (receivedData.Length >= 2)
            {
                //byte reversedLowByte = (byte)ReverseBits(receivedData[0]);
                //byte reversedHighByte = (byte)ReverseBits(receivedData[1]);
                byte reversedLowByte = receivedData[0];
                byte reversedHighByte = receivedData[1];

                byte b0 = reversedLowByte;// 0xE4;
                byte b2 = reversedHighByte;// 0xF9;
                byte b1 = (byte)(b2 & 0x3F);
                int rez14 = (b1 << 8) | b0;
             //   Console.WriteLine("14bit");
          //      Console.WriteLine(rez14);
           //     Console.WriteLine("12bit");
                int rez12 = rez14 >> 2;
                Console.Clear();
                Console.WriteLine(rez12);
            }
        }

        private static void DataReceivedHandler3(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            int bytesToRead = sp.BytesToRead;
            byte[] receivedData = new byte[bytesToRead];
            sp.Read(receivedData, 0, bytesToRead);

            // Debug: Show the number of bytes received
            Console.WriteLine($"Bytes received: {bytesToRead}");

            // Debug: Show the received bytes in hexadecimal
            Console.Write("Received bytes: ");
            foreach (byte b in receivedData)
            {
                Console.Write($"{b:X2} ");
            }
            Console.WriteLine();

            // Assuming the received data is at least 2 bytes
            if (receivedData.Length >= 2)
            {
                byte reversedLowByte = (byte)ReverseBits(receivedData[0]);
                byte reversedHighByte = (byte)ReverseBits(receivedData[1]);
                // Little-endian value
                int positionLittleEndian = (receivedData[1] << 8) | receivedData[0];
                // Big-endian value
                int positionBigEndian = (receivedData[0] << 8) | receivedData[1];

                // Debug: Show the values
                Console.WriteLine($"Little-endian value: {positionLittleEndian}");
                Console.WriteLine($"Big-endian value: {positionBigEndian}");

                // Shift right by 2 bits for a 12-bit encoder
                int shiftedLittleEndian = positionLittleEndian >> 2;
                int shiftedBigEndian = positionBigEndian >> 2;

                // Debug: Show the shifted values
                Console.WriteLine($"Shifted little-endian value: {shiftedLittleEndian}");
                Console.WriteLine($"Shifted big-endian value: {shiftedBigEndian}");
            }
        }

        private static void DataReceivedHandler2(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            int bytesToRead = sp.BytesToRead;
            byte[] receivedData = new byte[bytesToRead];
            sp.Read(receivedData, 0, bytesToRead);

            // Assuming the received data is 2 bytes for a 12-bit encoder
            if (receivedData.Length >= 2)
            {
                int position = (receivedData[0] << 8) | receivedData[1];
                // Shift right by 2 bits for a 12-bit encoder
                position >>= 2;

                Console.WriteLine($"Position: {position}");
            }
        }
        private static void DataReceivedHandler1(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            int bytesToRead = sp.BytesToRead;
            byte[] receivedData = new byte[bytesToRead];
            sp.Read(receivedData, 0, bytesToRead);

            // Process receivedData as needed
            Console.WriteLine("Data Received:");
            foreach (byte b in receivedData)
            {
                Console.Write(b.ToString("X2") + " ");
            }
            Console.WriteLine();
        }
        /* {
             SerialPort mySerialPort = new SerialPort("COM3");

             mySerialPort.BaudRate = 9600;
             mySerialPort.Parity = Parity.None;
             mySerialPort.StopBits = StopBits.One;
             mySerialPort.DataBits = 8;
             mySerialPort.Handshake = Handshake.None;
             mySerialPort.ReceivedBytesThreshold = 1; // Trigger the event for any received byte

             mySerialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);

             mySerialPort.Open();

             // Send the read position command (0x54)
             byte[] command = new byte[] { 0x00 };
             mySerialPort.Write(command, 0, command.Length);

             Console.WriteLine("Reading RS485 data. Press 'q' to exit...");

             while (true)
             {
                 if (Console.ReadKey().KeyChar == 'q')
                 {
                     break;
                 }
             }

             mySerialPort.Close();
             Console.WriteLine("Serial port closed. Exiting...");
         }*/

        /*
                private static void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
                {
                    SerialPort sp = (SerialPort)sender;
                    int bytesToRead = sp.BytesToRead;
                    byte[] receivedData = new byte[bytesToRead];
                    sp.Read(receivedData, 0, bytesToRead);

                    // Process receivedData as needed
                    Console.WriteLine("Data Received:");
                    foreach (byte b in receivedData)
                    {
                        Console.Write(b.ToString("X2") + " ");
                    }
                    Console.WriteLine();
                }
                */
    }
}
