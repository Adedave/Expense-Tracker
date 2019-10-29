using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace ExpenseTracker.Common.Notification
{
    public static class NotificationTemplate
    {
        static object synclock = new object();
        /// <summary>
        /// Parses the input string template, replacing occurences of tokens and mask inputs.
        /// Tokens may be in format {Email} or {Amount:n} or {VCN:n|Mask:x,3,4}
        /// Where {Email} simply gets replaced by Email token.
        /// :n and Masking are not yet implemented.
        /// </summary>
        /// <param name="messageTemplate">The message template formatted with tokens</param>
        /// <param name="tokens">A collection of tokens to use for replacement.</param>
        /// <returns>A parsed string.</returns>
        public static string ParseTemplate(this string messageTemplate, Dictionary<string, string> tokens)
        {
            lock (synclock)
            {
                //tokens may be in format {Email} or {Amount:n} or {VCN:n|Mask:x,3,4}
                foreach (KeyValuePair<string, string> item in tokens)
                {
                    messageTemplate = messageTemplate.Replace(item.Key, item.Value);
                }

                return messageTemplate;
            }
        }

        public static string ProcessContent(this string template, Dictionary<string, string> data)
        {
            return Regex.Replace(template, "\\{\\{(.*?)\\}\\}", m =>
               m.Groups.Count > 1 && data.ContainsKey(m.Groups[1].Value) ?
               data[m.Groups[1].Value] : m.Value);
        }

        public static string Mask(string toMask, char maskXter, int startIndex, int maskLength)
        {
            return string.Format("{0}{1}{2}", toMask.Substring(0, startIndex), "".PadRight(maskLength, maskXter), toMask.Substring(startIndex + maskLength));
        }

        public static Dictionary<string, string> Tokenize(object toTokenize)
        {
            lock (synclock)
            {
                PropertyInfo[] pInfos = toTokenize.GetType().GetProperties();
                Dictionary<string, string> tokens = new Dictionary<string, string>();
                foreach (var item in pInfos)
                {
                    //i'll apply some formating here.
                    object vObj = item.GetValue(toTokenize, null);
                    string coarsed = string.Empty;
                    if (vObj != null)
                    {
                        if (item.PropertyType == typeof(DateTime))
                        {
                            coarsed = string.Format("{0:yyyy-MMM-dd H:mm:ss}", vObj);
                        }
                        if (item.PropertyType == typeof(decimal))
                        {
                            coarsed = string.Format("{0:n}", vObj);
                        }
                        if (item.PropertyType == typeof(string))
                        {
                            coarsed = vObj.ToString();
                        }
                    }
                    tokens.Add("{" + item.Name + "}", coarsed);
                }
                return tokens;
            }
        }
    }
}
