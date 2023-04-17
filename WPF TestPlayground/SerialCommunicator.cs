using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using WPF_TestPlayground;

namespace WPF___Online_Arduino_Music_Player
{
    public class SerialCommunicator
    {
        private static SerialCommunicator? instance;
        private static readonly object padlock = new object();

        private SerialPort serialPort;

        private string _firstLine = "Connected!";
        public string FirstLine
        {
            get => _firstLine;
            set
            {
                _firstLine = value;
            }
        }

        private string _secondLine = "Hi there :)";
        public string SecondLine
        {
            get => _secondLine;
            set
            {
                _secondLine = value;
            }
        }

        public void UpdateArduino(MediaSessionModel mediaSessionModel)
        {
            FirstLine = $"[{mediaSessionModel.SongName}] by [{mediaSessionModel.Artist}]";
            SecondLine = $"[{mediaSessionModel.PlaybackStatus}] on [{mediaSessionModel.MediaSessionName}] - [{mediaSessionModel.Id}]";
            SendData(this.MemberwiseClone());
        }

        SerialCommunicator()
        {
            serialPort = new SerialPort()
            {
                BaudRate = 9600,
                PortName = "COM3"
            };
            SendData(this.MemberwiseClone());
        }

        public static SerialCommunicator Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (padlock)
                    {
                        if (instance == null)
                            instance = new SerialCommunicator();
                    }
                }
                return instance;
            }
        }

        public void SendData(object data)
        {
            try
            {
                if (!serialPort.IsOpen)
                    serialPort.Open();

                string input = JsonConvert.SerializeObject(data) + "#";
                var inputLength = input.Length - 1;
                serialPort.Write(input);

                System.Threading.Thread.Sleep(1000);
                string response = serialPort.ReadExisting();

                int start = response.IndexOf("Length: {") + 9; // add 9 to skip "Length: {"
                int end = response.IndexOf("}@", start);
                int outputLength = int.Parse(response.Substring(start, end - start));

                var a = response;

                if (inputLength != outputLength)
                    throw new Exception($"Failed to send or receive a message! Data length sent from the back-end: {inputLength}; Data length received in arduino: {outputLength}");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                serialPort.Close();
            }
        }

        public T? ReceivedData<T>()
        {
            string jsonString = "";

            try
            {
                if (!serialPort.IsOpen)
                    serialPort.Open();

                jsonString = serialPort.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                serialPort.Close();
            }

            return JsonConvert.DeserializeObject<T>(jsonString);

        }

    }
}
