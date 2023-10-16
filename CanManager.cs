using Kvaser.CanLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 

namespace ConsoleRS485
{
    internal class CanManager
    {
        private int handle;

        public CanManager()
        {
            Canlib.canInitializeLibrary();
        }

        public void ListChannels()
        {
            Canlib.canStatus stat = Canlib.canGetNumberOfChannels(out int numberOfChannels);
            CheckStatus(stat, "canGetNumberOfChannels");

            if (stat == Canlib.canStatus.canOK)
            {
                Console.WriteLine($"Found {numberOfChannels} channels");

                for (int i = 0; i < numberOfChannels; i++)
                {
                    stat = Canlib.canGetChannelData(i, Canlib.canCHANNELDATA_DEVDESCR_ASCII, out object deviceName);
                    CheckStatus(stat, "canGetChannelData (Device Name)");

                    stat = Canlib.canGetChannelData(i, Canlib.canCHANNELDATA_CHAN_NO_ON_CARD, out object deviceChannel);
                    CheckStatus(stat, "canGetChannelData (Channel Number)");

                    Console.WriteLine($"Found channel: {i} {deviceName} {((UInt32)deviceChannel + 1)}");
                }
            }
        }


        public void OpenChannel(int channelNumber)
        {
            handle = Canlib.canOpenChannel(channelNumber, Canlib.canOPEN_ACCEPT_VIRTUAL);
            CheckStatus((Canlib.canStatus)handle, "canOpenChannel");
        }

        public void SetBusParams()
        {
            var status = Canlib.canSetBusParams(handle, Canlib.canBITRATE_250K, 0, 0, 0, 0);
            CheckStatus(status, "canSetBusParams");
        }

        public void GoOnBus()
        {
            var status = Canlib.canBusOn(handle);
            CheckStatus(status, "canBusOn");
        }

        public void SendMessage(int id, byte[] message)
        {
            var status = Canlib.canWrite(handle, id, message, 8, 4);
            CheckStatus(status, "canWrite");
        }

        public void ReceiveMessage()
        {
            Canlib.canStatus status;
            byte[] data = new byte[8];
            int id;
            int dlc;
            int flags;
            long timestamp;
            bool finished = false;

            Console.WriteLine("Press the Spacebar to stop receiving messages.");

            while (!finished)
            {
                status = Canlib.canReadWait(handle, out id, data, out dlc, out flags, out timestamp, 100);

                if (status == Canlib.canStatus.canOK)
                {
                    Console.WriteLine($"Received Message: ID={id}, DLC={dlc}, Data={BitConverter.ToString(data, 0, dlc)}, Timestamp={timestamp}");
                }
                else if (status != Canlib.canStatus.canERR_NOMSG)
                {
                    CheckStatus(status, "canReadWait");
                    break;  // Exit the loop if an error occurs
                }

                if (Console.KeyAvailable)
                {
                    if (Console.ReadKey(true).Key == ConsoleKey.Spacebar)
                    {
                        finished = true;
                    }
                }
            }
        }


        public void GoOffBus()
        {
            var status = Canlib.canBusOff(handle);
            CheckStatus(status, "canBusOff");
        }

        public void CloseChannel()
        {
            var status = Canlib.canClose(handle);
            CheckStatus(status, "canClose");
        }

        private void CheckStatus(Canlib.canStatus status, string method)
        {
            if (status < 0)
            {
                string errorText;
                Canlib.canGetErrorText(status, out errorText);
                Console.WriteLine($"{method} failed: {errorText}");
            }
        }
    }
}
