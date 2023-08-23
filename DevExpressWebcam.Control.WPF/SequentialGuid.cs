using System.Linq;
using System;

namespace DevExpressWebcam.Control.WPF
{
    public static class SequentialGuid
    {
        private static Random rnd = new Random();
        private static int counter = 0;
        private static byte[] MacAddress = null;
        private static object syncOb = new object();

        static SequentialGuid()
        {
            lock (syncOb)
            {
                counter = rnd.Next();
            }
        }

        public static Guid NewGuid()
        {
            DateTime tm = DateTime.UtcNow;

            var t = new byte[16];
            rnd.NextBytes(t);

            // Group 5 & 4 (Most Significant).  Fill with DateTime
            var s = BitConverter.GetBytes(tm.Ticks);

            // Group 5
            t[10] = s[7];
            t[11] = s[6];
            t[12] = s[5];
            t[13] = s[4];
            t[14] = s[3];
            t[15] = s[2];

            // Group 4
            t[8] = s[1];
            t[9] = s[0];

            // Groups 3 & 2.  Fill with sequential counter;
            s = BitConverter.GetBytes(System.Threading.Interlocked.Increment(ref counter));

            // Group 3
            t[6] = s[3];
            t[7] = s[2];

            // Group 2
            t[4] = s[1];
            t[5] = s[0];

            // Group 1 (Least Significant).  Fill with last 4 bytes of MAC Address
            byte[] macAddress = GetMacAddress();
            t[0] = MacAddress[0];
            t[1] = MacAddress[1];
            t[2] = MacAddress[2];
            t[3] = MacAddress[3];

            return new Guid(t);
        }

        private static byte[] GetMacAddress()
        {
            if (MacAddress != null) return MacAddress;

            lock (syncOb)
            {
                var macAddr =
                (
                    from nic in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
                    where nic.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up
                    select nic.GetPhysicalAddress()?.GetAddressBytes() ?? new byte[] { }
                ).FirstOrDefault(n => n.Length > 0);

                if ((macAddr?.Length ?? 0) > 0)
                {
                    MacAddress = macAddr.Reverse().Take(4).ToArray();
                }
                else
                {
                    MacAddress = new byte[4];
                    rnd.NextBytes(MacAddress);
                }
            }

            return MacAddress;
        }
    }

}