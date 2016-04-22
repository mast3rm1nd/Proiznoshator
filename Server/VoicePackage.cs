using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server
{
    class VoicePackage
    {
        public string VoiceName { get; set; }
        public ushort Rate { get; set; }
        public ushort Volume { get; set; }
        public string Text { get; set; }

        public VoicePackage(string voiceName, ushort rate, ushort volume, string text)
        {
            VoiceName = voiceName;
            Rate = rate;
            Volume = volume;
            Text = text;
        }

        /// <summary>
        /// Deserialize data received
        /// </summary>
        public VoicePackage(byte[] data)
        {
            //IsMale = BitConverter.ToBoolean(data, 0);
            //Age = BitConverter.ToUInt16(data, 1);
            //int nameLength = BitConverter.ToInt32(data, 3);
            //Name = Encoding.ASCII.GetString(data, 7, nameLength);
            Rate = BitConverter.ToUInt16(data, 0);
            Volume = BitConverter.ToUInt16(data, 2);//2 - сдвиг от ushort

            int voiceNameLength = BitConverter.ToInt32(data, 4);//4 == 2 * ushort

            VoiceName = Encoding.UTF8.GetString(data, 8, voiceNameLength);//8 == 2 * ushort + unt

            //int voiceTextLength = BitConverter.ToInt32(data, 8 + voiceNameLength);//x == 2 * ushort + int + voice_name_length
            int voiceTextLength = data.Count() - (12 + voiceNameLength);

            Text = Encoding.UTF8.GetString(data, 12 + voiceNameLength, voiceTextLength);//x == 2 * ushort + 2 * unt + voice_name_length
        }

        /// <summary>
        ///  Serialize object to send over a network
        /// </summary>
        public byte[] ToByteArray()
        {
            List<byte> byteList = new List<byte>();
            byteList.AddRange(BitConverter.GetBytes(Rate));
            byteList.AddRange(BitConverter.GetBytes(Volume));

            byteList.AddRange(BitConverter.GetBytes(VoiceName.Length));
            byteList.AddRange(Encoding.UTF8.GetBytes(VoiceName));

            byteList.AddRange(BitConverter.GetBytes(Text.Length));
            byteList.AddRange(Encoding.UTF8.GetBytes(Text));

            return byteList.ToArray();
        }
    }
}
