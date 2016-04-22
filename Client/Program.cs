using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Diagnostics;
using System.Threading;

namespace Client
{
    class Program
    {
        static Client client;

        static string[] installedVoices = { "test voice", "supervoice", "ololo voice" };

        static string serverIP = "127.0.0.1";

        static string name;

        static Thread playMessagesFromQueThread;

        static void Main(string[] args)
        {
            var serverExecutable = @"E:\Dropbox\!Coding\All_in_one\Proiznoshator\Server\bin\Debug\Server.exe";

            //Process.Start(serverExecutable);

            WaitForConnection();

            Console.Write("Введите имя: ");
            name = Console.ReadLine();
            Console.Title = name;

            playMessagesFromQueThread = new Thread(new ThreadStart(PlayMessagesFromQue));
            playMessagesFromQueThread.IsBackground = true;

            playMessagesFromQueThread.Start();

            while (true)
            {
                Console.Write("Введите сообщение: ");
                var text = Console.ReadLine();

                var message = String.Format("{0} говорит: {1}", name, text);

                var testVoice = new VoicePackage("test voice", 0, 100, message).ToByteArray();
                client.Send(testVoice);
                Console.WriteLine();
            }
        }


        static void WaitForConnection()
        {
            client = new Client();

            var tries = 0;
            while(!client.IsConnected)
            {
                tries++;
                Console.Clear();
                Console.WriteLine(String.Format("Попытка коннекта: {0}", tries));
                client.Connect(serverIP);
                Thread.Sleep(1000);
            }

            Console.Clear();
            Console.WriteLine("Соединение с {0} установлено.", serverIP);
        }



        static void PlayMessagesFromQue()
        {
            while (true)
            {
                Thread.Sleep(200);

                if (Globals.messagesQue.Count != 0) // если есть сообщения
                    if (Globals.IsReceiveMessages || true)
                    {
                        var voice = Globals.messagesQue[0].VoiceName;

                        //var test = installedVoices.First(x => x.VoiceInfo.Name == voice);

                        //if (test == null)
                        //{
                        //    Globals.messagesQue.RemoveAt(0);
                        //    continue;
                        //}
                        //try
                        //{
                        //    installedVoices.First(x => x.VoiceInfo.Name == voice);
                        //}
                        //catch
                        //{
                        //    Globals.messagesQue.RemoveAt(0);
                        //    continue;
                        //}
                        if(!installedVoices.Contains(voice))
                        {
                            Console.WriteLine(
                            String.Format("--------------------------------") + Environment.NewLine +
                            String.Format("Принят голосовой пакет с отсутствующим голосом: ") + Environment.NewLine +
                            String.Format("Текст:     {0}", Globals.messagesQue[0].Text) + Environment.NewLine +
                            String.Format("Голос:     {0}", Globals.messagesQue[0].VoiceName) + Environment.NewLine +
                            String.Format("Громкость: {0}", Globals.messagesQue[0].Volume) + Environment.NewLine +
                            String.Format("Скорость:  {0}", Globals.messagesQue[0].Rate) + Environment.NewLine +
                            String.Format("--------------------------------") + Environment.NewLine
                            );

                            Globals.messagesQue.RemoveAt(0);
                            continue;
                        }



                        var text = Globals.messagesQue[0].Text;

                        //MessageBox.Show(text);

                        if (text.StartsWith(string.Format("{0} говорит: ", name)))
                        {
                            Console.WriteLine(
                            String.Format("--------------------------------") + Environment.NewLine +
                            String.Format("Принят голосовой пакет с таким же именем: ") + Environment.NewLine +
                            String.Format("Текст:     {0}", Globals.messagesQue[0].Text) + Environment.NewLine +
                            String.Format("Голос:     {0}", Globals.messagesQue[0].VoiceName) + Environment.NewLine +
                            String.Format("Громкость: {0}", Globals.messagesQue[0].Volume) + Environment.NewLine +
                            String.Format("Скорость:  {0}", Globals.messagesQue[0].Rate) + Environment.NewLine +
                            String.Format("--------------------------------") + Environment.NewLine
                            );


                            Globals.messagesQue.RemoveAt(0);
                            continue;
                        }

                        var rate = Globals.messagesQue[0].Rate;
                        var volume = Globals.messagesQue[0].Volume;

                        //synth.SelectVoice(voice);
                        //synth.Rate = (int)rate;
                        //synth.Volume = (int)volume;

                        var delay = Globals.messagesQue[0].Text.Length * 300;

                        Console.WriteLine(
                            String.Format("--------------------------------") + Environment.NewLine +
                            String.Format("Принят голосовой пакет, симулируем произнесение задержкой {0} мс: ", delay) + Environment.NewLine +
                            String.Format("Текст:     {0}", Globals.messagesQue[0].Text) + Environment.NewLine +
                            String.Format("Голос:     {0}", Globals.messagesQue[0].VoiceName) + Environment.NewLine +
                            String.Format("Громкость: {0}", Globals.messagesQue[0].Volume) + Environment.NewLine +
                            String.Format("Скорость:  {0}", Globals.messagesQue[0].Rate) + Environment.NewLine +
                            String.Format("--------------------------------") + Environment.NewLine
                            );

                        Thread.Sleep(delay);

                        Globals.messagesQue.RemoveAt(0);
                    }
                    else
                    {
                        Globals.messagesQue.Clear();
                    }
            }
        }
    }
}
