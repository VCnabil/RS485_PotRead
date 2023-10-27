using System;
using System.IO.Ports;
using System.Timers;

namespace ConsoleRS485
{
    public class RS485Manager
    {
        private SerialPort mySerialPort;
        private Timer timer;
        private int rez14;
        private int rez12;

        private byte rez12_b0_LOW;
        private byte rez12_b1_HIGH;

        private byte rez14_b0_LOW;
        private byte rez14_b1_HIGH;

        private byte raw_b0_LOW;
        private byte raw_b1_HIGH;

        public RS485Manager(string portName, int baudRate)
        {
            mySerialPort = new SerialPort(portName)
            {
                BaudRate = baudRate,
                Parity = Parity.None,
                StopBits = StopBits.One,
                DataBits = 8,
                Handshake = Handshake.None
            };

            mySerialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
        }

        public void Start()
        {
            mySerialPort.Open();
            timer = new Timer(50);
            timer.Elapsed += new ElapsedEventHandler(SendCommand);
            timer.Start();
        }

        public void Stop()
        {
            timer.Stop();
            mySerialPort.Close();
        }

        private void SendCommand(object sender, ElapsedEventArgs e)
        {
            byte[] command = new byte[] { 0x24 };
            mySerialPort.Write(command, 0, command.Length);
        }

    
        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            int bytesToRead = sp.BytesToRead;
            byte[] receivedData = new byte[bytesToRead];
            sp.Read(receivedData, 0, bytesToRead);


            if (receivedData.Length >= 2)
            {
                byte reversedLowByte = receivedData[0];
                byte reversedHighByte = receivedData[1];
                raw_b0_LOW = receivedData[0];
                raw_b1_HIGH = receivedData[1];

                byte b0 = reversedLowByte;// 0xE4;

                byte b1temp = reversedHighByte;// 0xF9;
                byte b1 = (byte)(b1temp & 0x3F);
                 rez14 = (int)(b1 << 8) | b0;
                 rez12 =(int) rez14 >> 2;

                int value1 = rez12;// 0xABCD; // 16-bit integer

                // Extract low byte (0xCD)
                byte lowByte1 = (byte)(value1 & 0xFF);

                // Extract high byte (0xAB)
                byte highByte1 = (byte)((value1 >> 8) & 0xFF);

                rez12_b0_LOW = lowByte1;
                rez12_b1_HIGH = highByte1;
                int value2 = rez14;// 0xABCD; // 16-bit integer

                // Extract low byte (0xCD)
                byte lowByte2 = (byte)(value2 & 0xFF);

                // Extract high byte (0xAB)
                byte highByte2 = (byte)((value2 >> 8) & 0xFF);
                rez14_b0_LOW = lowByte2;
                rez14_b1_HIGH = highByte2;


            }
        }
        public int getREZ14() { return this.rez14; }
        public int getREZ12() { return this.rez12; }


        public byte getREZ14_low() { return this.rez14_b0_LOW; }
        public byte getREZ14_HIGH() { return this.rez14_b1_HIGH; }


        public byte getREZ12_low() { return this.rez12_b0_LOW; }
        public byte getREZ12_HIGH() { return this.rez12_b1_HIGH; }


        public byte getRAW_low() { return this.raw_b0_LOW; }
        public byte getRAW_HIGH() { return this.raw_b1_HIGH; }

        /*    private int ReverseBits(byte b)
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
*/
    }
}