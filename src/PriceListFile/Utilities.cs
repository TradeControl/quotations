using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeControl.PriceList
{
    public class Utilities
    {
        public static List<string> Lines(string data)
        {
            List<string> extras = new List<string>();
            string s = string.Empty;

            ASCIIEncoding ascii = new ASCIIEncoding();
            Byte[] bytes = ascii.GetBytes(data);

            for (int i = 0; i < bytes.Length; i++)
            {
                switch (bytes[i])
                {
                    case 10:
                        break;
                    case 13:
                        extras.Add(s);
                        s = string.Empty;
                        break;
                    default:
                        s += data[i];
                        break;
                }
            }

            if (s.Trim() != string.Empty)
                extras.Add(s);

            return extras;
        }

    }
}
