using Kvaser.CanLib;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Timers;


namespace ConsoleRS485
{
    public class RS1939Controller
    {
        private Timer timer;
        private SerialPort mySerialPort;
        byte[] command;
        private int rez14;
        private int rez12;
        private byte rez12_b0_LOW;
        private byte rez12_b1_HIGH;
        private byte rez14_b0_LOW;
        private byte rez14_b1_HIGH;
        byte[] message = { 0, 0, 0, 0, 0, 0, 0, 0 };


        private int handle;
        public RS1939Controller(string portName, int baudRate,byte CommandNum, int CanChannelNumber, int canBaudNeg3, int TimerSteps)
        {

            Init_RS485(portName, baudRate);
            Init_J1939(CanChannelNumber, canBaudNeg3);
            Init_Timer(TimerSteps);
            command = new byte[] { CommandNum };
        }
        ~RS1939Controller()
        {
            Cleanup1939();
            CleanupRS485();
            CleanupTimer();
        }

        private void Init_RS485(string portName, int baudRate) {
            mySerialPort = new SerialPort(portName)
            {
                BaudRate = baudRate,
                Parity = Parity.None,
                StopBits = StopBits.One,
                DataBits = 8,
                Handshake = Handshake.None
            };
            mySerialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceived_RS485_Handler);
            mySerialPort.Open();
        }

        private void Init_J1939(int channelNumber, int canBaudNeg3)
        {
            Canlib.canInitializeLibrary();
            handle = Canlib.canOpenChannel(channelNumber, Canlib.canOPEN_ACCEPT_VIRTUAL);
            //checkstatus
            if ((Canlib.canStatus)handle < 0)
            {
                string errorText;
                Canlib.canGetErrorText((Canlib.canStatus)handle, out errorText);
                Console.WriteLine($"{"canOpenChannel" + channelNumber} failed: {errorText}");
            }
            else {
                //set params
                //  Canlib.canSetBusParams(handle, Canlib.canBITRATE_250K, 0, 0, 0, 0);
                Canlib.canSetBusParams(handle, canBaudNeg3, 0, 0, 0, 0);
                //go on bus
                Canlib.canBusOn(handle);
            }
        }

        private void Init_Timer(int argTimerstep) {
            timer = new Timer(argTimerstep);
            timer.Elapsed += new ElapsedEventHandler(SendCommand);
            timer.Start();
        }
        private void SendCommand(object sender, ElapsedEventArgs e)
        {       
            mySerialPort.Write(command, 0, command.Length);
        }
        private void DataReceived_RS485_Handler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            int bytesToRead = sp.BytesToRead;
            byte[] receivedData = new byte[bytesToRead];
            sp.Read(receivedData, 0, bytesToRead);

            if (receivedData.Length >= 2)
            {
                byte reversedLowByte = receivedData[0];
                byte reversedHighByte = receivedData[1];

                byte b0 = reversedLowByte;// 0xE4;

                byte b1temp = reversedHighByte;// 0xF9;
                byte b1 = (byte)(b1temp & 0x3F);
                rez14 = (int)(b1 << 8) | b0;
                rez12 = (int)rez14 >> 2;

                int value1 = rez12;// 0xABCD; // 16-bit integer
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


                message[0] = rez12_b0_LOW;
                message[1] = rez12_b1_HIGH;
                message[2] = rez14_b0_LOW;
                message[3] = rez14_b1_HIGH;

                Canlib.canWrite(handle, 0x18FFFA00, message, 8, 4);
               

            }
        }

        private void Cleanup1939() {
            Canlib.canBusOff(handle);
            Canlib.canClose(handle);
        }
        private void CleanupRS485()
        {
            mySerialPort.Close();
        }
        private void CleanupTimer() {
            timer.Stop();
        }
    }
}
