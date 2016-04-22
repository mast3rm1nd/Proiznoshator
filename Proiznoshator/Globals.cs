using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proiznoshator
{
    public class Globals
    {
        internal static List<VoicePackage> messagesQue = new List<VoicePackage>();
        public static bool IsServer { get; set; } = false;
        public static bool IsReceiveMessages { get; set; } = false;
        public static bool IsSendMessages { get; set; } = false;
        public static string Username { get; set; }
    }
}
