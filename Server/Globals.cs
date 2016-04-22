using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server
{
    public class Globals
    {
        internal static List<VoicePackage> messagesQue = new List<VoicePackage>();
        public static bool IsServer { get; set; } = true;
        public static bool IsReceiveMessages { get; set; } = true;
        public static bool IsSendMessages { get; set; } = true;
    }
}
