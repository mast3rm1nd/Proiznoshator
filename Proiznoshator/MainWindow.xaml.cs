using System;
using System.Linq;
using System.Text;
using System.Windows;

using System.Speech.Synthesis;
//using Microsoft.Speech.Synthesis;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Diagnostics;
using System.Xml;
using System.Speech.AudioFormat;



//Languages fo Microsoft.Speech: http://go.microsoft.com/fwlink/?LinkID=223569&clcid=0x409 (
namespace Proiznoshator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static SpeechSynthesizer synth = new SpeechSynthesizer();
        static Client client = new Client();
        static Server server = new Server();

        
        Thread playMessagesFromQueThread;


        System.Collections.ObjectModel.ReadOnlyCollection<InstalledVoice> installedVoices;
        private XmlDocument settings_xml;

        //IEnumerable<InstalledVoice> installedVoices = synth.GetInstalledVoices();
        //readonly InstalledVoice[] installedVoices1 = synth.GetInstalledVoices();

        public MainWindow()
        {
            InitializeComponent();



            LoadSettings();

            try
            {
                installedVoices = synth.GetInstalledVoices();
            }
            catch { }

            FillComboBoxWithVoices();
            
            synth.SpeakCompleted += synth_SpeakCompleted;
            synth.SpeakStarted += synth_SpeakStarted;

            //synth.SetOutputToDefaultAudioDevice();


            Volume_Slider_ValueChanged(null, null);
            Rate_Slider_ValueChanged(null, null);

            playMessagesFromQueThread = new Thread(new ThreadStart(PlayMessagesFromQue));
            playMessagesFromQueThread.IsBackground = true;
        }


        void PlayMessagesFromQue()
        {
            while(true)
            {
                Thread.Sleep(200);

                if (Globals.messagesQue.Count != 0) // если есть сообщения
                    if (synth.State == SynthesizerState.Ready) // если ничего не произносится
                        if (Globals.IsReceiveMessages)
                        {
                            var voice = Globals.messagesQue[0].VoiceName;

                            try
                            {
                                installedVoices.First(x => x.VoiceInfo.Name == voice);
                            }
                            catch
                            {
                                Globals.messagesQue.RemoveAt(0);
                                continue;
                            }



                            var text = Globals.messagesQue[0].Text;

                            //MessageBox.Show(text);

                            if (text.StartsWith(string.Format("{0} говорит: ", Globals.Username)))
                            {
                                Debug.WriteLine("Main: удалено сообщение, ник был тот же");
                                Globals.messagesQue.RemoveAt(0);
                                continue;
                            }

                            var rate = Globals.messagesQue[0].Rate;
                            var volume = Globals.messagesQue[0].Volume;

                            synth.SelectVoice(voice);
                            synth.Rate = (int)rate;
                            synth.Volume = (int)volume;

                            Debug.WriteLine("Main: начато произнесение полученого текста");
                            synth.SpeakAsync(text);
                            //MessageBox.Show(String.Format("{0}:{1}:{2}:{3}", text, rate, volume, voice));

                            Globals.messagesQue.RemoveAt(0);
                        }
                        else
                        {
                            Globals.messagesQue.Clear();
                        }                                      
            }
        }


        void synth_SpeakStarted(object sender, SpeakStartedEventArgs e)
        {
            Say_Button.IsEnabled = false;
            Stop_Button.IsEnabled = true;
            SaveToFile_Button.IsEnabled = false;
        }

        private void synth_SpeakCompleted(object sender, SpeakCompletedEventArgs e)
        {
            Say_Button.IsEnabled = true;
            Stop_Button.IsEnabled = false;
            SaveToFile_Button.IsEnabled = true;
        }

        private void Say_Button_Click(object sender, RoutedEventArgs e)
        {
            TrySendVoice();

            var selectedText = TextToSay_TextBox.SelectedText;


            if (installedVoices.Count() == 0)
                return;

            var selectedVoice = installedVoices.First(x => x.VoiceInfo.Name.Replace("Microsoft Server Speech Text to Speech Voice ", "").Replace(")", "").Replace("(", "") == (string)Voices_ComboBox.SelectedValue).VoiceInfo.Name;//.Replace("Microsoft Server Speech Text to Speech Voice ", "")

            synth.SelectVoice(selectedVoice);
            synth.Rate = (int)Rate_Slider.Value;
            synth.Volume = (int)Volume_Slider.Value;

            synth.SetOutputToDefaultAudioDevice();

            if (selectedText == "")
                synth.SpeakAsync(TextToSay_TextBox.Text);
            else
                synth.SpeakAsync(selectedText);
        }


        void SaveInstalledVoices()
        {
            var voicesText = "";
            var fileToSave = "InstalledVoices.txt";

            foreach (var voice in installedVoices)
            {
                voicesText += String.Format("Язык:                      {0}", voice.VoiceInfo.Culture) + Environment.NewLine;
                voicesText += String.Format("Пол:                       {0}", voice.VoiceInfo.Gender) + Environment.NewLine;
                voicesText += String.Format("Имя:                       {0}", voice.VoiceInfo.Name) + Environment.NewLine;                
                voicesText += String.Format("Возраст:                   {0}", voice.VoiceInfo.Age) + Environment.NewLine;
                voicesText += String.Format("Описание:                  {0}", voice.VoiceInfo.Description) + Environment.NewLine;
                voicesText += String.Format("Дополнительная информация: {0}", voice.VoiceInfo.AdditionalInfo) + Environment.NewLine;
                voicesText += "--------------------------------------------" + Environment.NewLine;
            }

            File.WriteAllText(fileToSave, voicesText);
        }


        void FillComboBoxWithVoices()
        {
            Voices_ComboBox.Items.Clear();

            if (installedVoices == null)
                return;

            if (installedVoices.Count() == 0)
                return;

            foreach (var voice in installedVoices)
            {
                Voices_ComboBox.Items.Add(voice.VoiceInfo.Name.Replace("Microsoft Server Speech Text to Speech Voice ", "").Replace(")", "").Replace("(", ""));                
            }

            var preferableIndex = Voices_ComboBox.Items.IndexOf("ru-RU, Elena");

            if (preferableIndex == -1)
                Voices_ComboBox.SelectedIndex = 0;
            else
                Voices_ComboBox.SelectedIndex = preferableIndex;
        }

        private void Volume_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(Volume_Slider != null && Volume_Label != null)
                Volume_Label.Content = ((int)Volume_Slider.Value).ToString();
        }

        private void Rate_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Rate_Slider != null && Rate_Label != null)
            {
                Rate_Label.Content = ((int)Rate_Slider.Value).ToString();
            }                
        }

        private void Stop_Button_Click(object sender, RoutedEventArgs e)
        {
            Say_Button.IsEnabled = true;
            Stop_Button.IsEnabled = false;

            synth.SpeakAsyncCancelAll();
            //synth.Pause();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            synth.SpeakAsyncCancelAll();
            SaveSettings();
        }

        private void SaveToFile_Button_Click(object sender, RoutedEventArgs e)
        {
            var fileToSave = String.Format("{0}.wav", getMd5Hash(TextToSay_TextBox.Text));

            var selectedVoice = installedVoices.First(x => x.VoiceInfo.Name.Replace("Microsoft Server Speech Text to Speech Voice ", "").Replace(")", "").Replace("(", "") == (string)Voices_ComboBox.SelectedValue).VoiceInfo.Name;//.Replace("Microsoft Server Speech Text to Speech Voice ", "")

            synth.SelectVoice(selectedVoice);
            synth.Rate = (int)Rate_Slider.Value;
            synth.Volume = (int)Volume_Slider.Value;

            //SpeechAudioFormatInfo format = new SpeechAudioFormatInfo(440000, AudioBitsPerSample.Sixteen, AudioChannel.Stereo);

            //synth.SetOutputToWaveFile(fileToSave, format);
            var currFolder = AppDomain.CurrentDomain.BaseDirectory;

            synth.SetOutputToWaveFile(currFolder + "\\" + fileToSave);

            try
            {
                synth.Speak(TextToSay_TextBox.Text);
            }
            
            catch
            {
                MessageBox.Show("Не удалось сохранить в файл.");                
                return;                
            }
            finally
            {
                synth.SetOutputToNull();
                SaveToFile_Button.IsEnabled = true;
                Say_Button.IsEnabled = true;
            }           

            

            MessageBox.Show(String.Format("Сохранено в \"{0}\"", fileToSave));
        }


        static string getMd5Hash(string input)
        {
            // Create a new instance of the MD5CryptoServiceProvider object.
            MD5 md5Hasher = MD5.Create();

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }



        byte[] GetUsersVoicePackage()
        {
            var selectedVoice = installedVoices.First(x => x.VoiceInfo.Name.Replace("Microsoft Server Speech Text to Speech Voice ", "").Replace(")", "").Replace("(", "") == (string)Voices_ComboBox.SelectedValue).VoiceInfo.Name;//.Replace("Microsoft Server Speech Text to Speech Voice ", "")
            var rate = (short)Rate_Slider.Value;
            var volume = (ushort)Volume_Slider.Value;
            var text = String.Format("{0} говорит: {1}", NickName_TextBox.Text, TextToSay_TextBox.Text);

            return new VoicePackage(selectedVoice, rate, volume, text).ToByteArray();
        }



        VoicePackage GetUsersVoicePackageNotByte()
        {
            var selectedVoice = installedVoices.First(x => x.VoiceInfo.Name.Replace("Microsoft Server Speech Text to Speech Voice ", "").Replace(")", "").Replace("(", "") == (string)Voices_ComboBox.SelectedValue).VoiceInfo.Name;//.Replace("Microsoft Server Speech Text to Speech Voice ", "")
            var rate = (short)Rate_Slider.Value;
            var volume = (ushort)Volume_Slider.Value;
            var text = String.Format("{0} говорит: {1}", NickName_TextBox.Text, TextToSay_TextBox.Text);

            return new VoicePackage(selectedVoice, rate, volume, text);
        }




        private void IsServer_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if(NickName_TextBox.Text == "")
            {
                MessageBox.Show("Сначала введите своё имя\\ник.");
                IsServer_CheckBox.IsChecked = false;
                return;
            }

            UpdateServerUI(false);

            Globals.IsServer = true;

            server.SetupServer();
            //client.Connect("127.0.0.1");
            playMessagesFromQueThread = new Thread(new ThreadStart(PlayMessagesFromQue));
            playMessagesFromQueThread.IsBackground = true;

            playMessagesFromQueThread.Start();
        }

        private void IsServer_CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateServerUI(true);

            Globals.IsServer = false;

            server.CloseAllSockets();

            playMessagesFromQueThread.Abort();
        }


        void UpdateServerUI(bool isEnabled)
        {
            IsReceiveMessages_CheckBox.IsEnabled = !isEnabled;
            IsSendMessages_CheckBox.IsEnabled = !isEnabled;
            ServerIP_TextBox.IsEnabled = isEnabled;
            NickName_TextBox.IsEnabled = isEnabled;
            Connect_Button.IsEnabled = isEnabled;
        }


        private void Connect_Button_Click(object sender, RoutedEventArgs e)
        {
            if(client.IsConnected)
            {
                client.CloseConnection();

                Connect_Button.Content = "Соединиться";
                NickName_TextBox.IsEnabled = true;
                ServerIP_TextBox.IsEnabled = true;

                UpdateServerUI(true);

                IsServer_CheckBox.IsEnabled = true;

                playMessagesFromQueThread.Abort();
            }
            else
            {
                client = new Client();

                var serverIP = "";

                switch(ServerIP_TextBox.Text.ToLower())
                {
                    case "лёха": serverIP = "188.243.202.217"; break;
                    case "леха": serverIP = "188.243.202.217"; break;
                    default: serverIP = ServerIP_TextBox.Text; break;
                }

                client.Connect(serverIP);
                WaitForConnection();

                if (client.IsConnected)
                {
                    Connect_Button.Content = "Отсоединиться";
                    NickName_TextBox.IsEnabled = false;
                    ServerIP_TextBox.IsEnabled = false;

                    UpdateServerUI(false);

                    IsServer_CheckBox.IsEnabled = false;

                    Globals.Username = NickName_TextBox.Text;

                    playMessagesFromQueThread = new Thread(new ThreadStart(PlayMessagesFromQue));
                    playMessagesFromQueThread.IsBackground = true;

                    playMessagesFromQueThread.Start();
                }
                else
                {
                    MessageBox.Show("Не удалось соединиться с " + ServerIP_TextBox.Text);
                    //Connect_Button.Content = "Соединиться";
                    //NickName_TextBox.IsEnabled = true;
                }
            }

            Connect_Button.IsEnabled = true;
                       
        }



        void WaitForConnection()
        {
            var times = 0;
            while(!client.IsConnected && times < 100)
            {
                Thread.Sleep(15);
                times++;
            }    
        }        


        void TrySendVoice()
        {
            if (IsSendMessages_CheckBox.IsChecked != true)
                return;

            if (IsSendMessages_CheckBox.IsEnabled != true)
                return;

            var voice = GetUsersVoicePackage();

            if (Globals.IsServer)
                server.ResendToEveryClient(GetUsersVoicePackage());
            else
                client.Send(voice);

            Debug.WriteLine("Main: sending voice...");
        }

        private void IsSendMessages_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Globals.IsSendMessages = true;
        }

        private void IsSendMessages_CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Globals.IsSendMessages = false;
        }

        private void IsReceiveMessages_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Globals.IsReceiveMessages = true;
        }

        private void IsReceiveMessages_CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Globals.IsReceiveMessages = false;
        }



        private void LoadSettings()
        {
            if (File.Exists(".\\Settings.xml"))
            {
                try
                {
                    settings_xml = new XmlDocument();
                    settings_xml.Load(".\\Settings.xml");

                    ServerIP_TextBox.Text = settings_xml.SelectSingleNode("settings/serverip").InnerText;
                    NickName_TextBox.Text = settings_xml.SelectSingleNode("settings/nickname").InnerText;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }



        private void SaveSettings()
        {
            if (File.Exists(".\\Settings.xml"))
            {
                try
                {
                    settings_xml.SelectSingleNode("settings/serverip").InnerText = ServerIP_TextBox.Text;
                    settings_xml.SelectSingleNode("settings/nickname").InnerText = NickName_TextBox.Text;                    
                    settings_xml.Save(".\\Settings.xml");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }

            }
            else
            {
                using (XmlWriter writer = XmlWriter.Create(".\\Settings.xml"))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("settings");                    

                    writer.WriteElementString("nickname", NickName_TextBox.Text);
                    writer.WriteElementString("serverip", ServerIP_TextBox.Text);
                    

                    writer.WriteEndElement();                  
                    
                    writer.WriteEndDocument();
                }                
            }
        }
    }
}
