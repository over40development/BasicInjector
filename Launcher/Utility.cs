// Utility.cs


#region Using Directives
using Launcher.Properties;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
#endregion

namespace Launcher
{
    /// <summary>
    /// Utility Class
    /// </summary>
    public static class Utility
    {
        #region Locals
        private static Settings Settings = Properties.Settings.Default;
        private static WebClient Client = null;
        #endregion

        #region Constructor
        static Utility()
        {
            Client = new WebClient();
            Client.Proxy = null;
        }
        #endregion
        
        /// <summary>
        /// Generic Web Request
        /// </summary>
        /// <param name="url">site</param>
        /// <returns></returns>
        public static string Request(string url)
        {
            try
            {
                Client.Headers.Add("User-Agent", Settings.UserAgent);

                return Client.DownloadString(url);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Request Error");

                return null;
            }
        }

        /// <summary>
        /// ROT13 Encryption
        /// </summary>
        /// <param name="input">encrypted string</param>
        /// <returns></returns>
        public static string ROT13(string input)
        {
            return !string.IsNullOrEmpty(input)
                ? new string(input.ToCharArray().Select(s => {
                    return (char)((s >= 97 && s <= 122) ? ((s + 13 > 122) ? s - 13 : s + 13) : (s >= 65 && s <= 90 ? (s + 13 > 90 ? s - 13 : s + 13) : s));
                }).ToArray())
                : input;
        }

        /// <summary>
        /// Decrypt Authentication Response
        /// </summary>
        /// <param name="response">http response</param>
        /// <returns></returns>
        public static string DecryptAuthResponse(string response)
        {
            byte[] data = FromHex(response);
            string hexd = Encoding.ASCII.GetString(data);
            string hexd_rev_rot = IntRot13Reversal(hexd);
            string hexd_rev_str_rot = ROT13(hexd_rev_rot);
            byte[] bases = Convert.FromBase64String(hexd_rev_str_rot);
            string decoded = Encoding.UTF8.GetString(bases);

            return decoded;
        }

        /// <summary>
        /// Convert string to byte array
        /// </summary>
        /// <param name="hex">hex as string</param>
        /// <returns></returns>
        public static byte[] FromHex(string hex)
        {
            hex = hex.Replace("-", "");
            byte[] raw = new byte[hex.Length / 2];
            for (int i = 0; i < raw.Length; i++)
                raw[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);

            return raw;
        }

        /// <summary>
        /// ROT13 decryption
        /// </summary>
        /// <param name="source">encrypted response</param>
        /// <returns></returns>
        public static string IntRot13Reversal(string source)
        {
            StringBuilder someString = new StringBuilder(source);
            for (int i = 0; i < someString.Length; i++)
            {
                switch (source[i])
                {
                    case '3':
                        someString[i] = '0';
                        break;
                    case '4':
                        someString[i] = '1';
                        break;
                    case '5':
                        someString[i] = '2';
                        break;
                    case '6':
                        someString[i] = '3';
                        break;
                    case '7':
                        someString[i] = '4';
                        break;
                    case '8':
                        someString[i] = '5';
                        break;
                    case '9':
                        someString[i] = '6';
                        break;
                    case '0':
                        someString[i] = '7';
                        break;
                    case '1':
                        someString[i] = '8';
                        break;
                    case '2':
                        someString[i] = '9';
                        break;
                    default:
                        someString[i] = source[i];
                        break;
                }
            }

            return someString.ToString();
        }
    }
}
