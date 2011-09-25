using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;
using System.IO;

namespace DeploymentHelper
{
    class Program
    {
        static void Main(string[] args)
        {

            var items = ConfigurationLoader.ScanAndGetConfig();

            if (args.Length != 2)
            {
                WriteInfo("Arguments not specified. Usage DeploymentHelper.exe applicationName environmentName");
                WriteInfo("e.g DeploymentHelper.exe deploymenttest QA");
                return;
            }

            string appName = args[0];
            string environmentName  = args[1];

            var item = items.Get(appName, environmentName);

            if (item == null)
            {
                WriteError(string.Format("Unable to find configuration file for {0} application's  {1} environment", appName, environmentName));
                return;
            }
            else
            {
                WriteInfo(string.Format("Found configuration file for {0} application's  {1} environment", appName, environmentName));
            }

            if (item.Settings.Count() <= 0)
            {
                WriteError("No settings found in file.");
                return;
            }

            XmlDocument doc = new XmlDocument();
            doc.Load(item.ConfigLocation);
            var navigator = doc.CreateNavigator();
            foreach(var setting in item.Settings)
            {
                navigator.MoveToRoot();
                XPathNodeIterator iterator = navigator.Select(setting.Key);

                if (iterator.Count == 0)
                    WriteWarn(string.Format("No matches found for key {0} when processing setting Id {1}", setting.Key, setting.Id));

                while (iterator.MoveNext())
                {
                    if (iterator.Current.NodeType != XPathNodeType.Element)
                    {
                        throw new Exception(string.Format("XPath key for id {0}, is not pointing to an element", setting.Id));
                    }
                    XmlElement element = iterator.Current.UnderlyingObject as XmlElement;

                    string j = element.GetAttribute(setting.AttributeName);

                    string result = setting.ReplaceValue;
                    if (!string.IsNullOrEmpty(setting.MatchExpression))
                    {
                        bool isMatching = Regex.Match(j, setting.MatchExpression,RegexOptions.IgnoreCase).Success;

                        if (isMatching)
                            result = Regex.Replace(j, setting.MatchExpression, setting.ReplaceValue, RegexOptions.IgnoreCase);
                        else
                            result = null;
                    }
                    else 
                    {
                        WriteWarn(string.Format("Match expression missing in id {0}", setting.Id));
                    }

                    if (result != null)
                    {
                        WriteReplacedInfo(j, result, element.OuterXml, setting.Id);
                        element.RemoveAttribute(setting.AttributeName);
                        element.SetAttribute(setting.AttributeName, result);
                    }
                }
            }

            string backupFileName=  item.ConfigLocation + "." + DateTime.Now.ToString("yyyyMMdd_HHmmss");
            
            doc.Save(backupFileName );
            WriteInfo(string.Format("Saving file {0} with updated configuration", backupFileName));

            WriteWarn("Complete", false);
        }

        private static void WriteReplacedInfo(string contentToBeReplaced, string replaceWith, string actualXmlContent, string id)
        {
            WriteInfo("INFO >> Replacing ",false);
            WriteWarn(contentToBeReplaced,false);
            WriteInfo(" with ", false);
            WriteWarn(replaceWith, false);
            WriteInfo(string.Format(" in {0} for id {1}{2}",actualXmlContent, id,System.Environment.NewLine),false);

        }

        private static void WriteWarn(string p)
        {
            WriteWarn(p, true);
        }


        private static void WriteWarn(string p, bool displayWarn)
        {
            ConsoleColor original = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            if(displayWarn)
                Console.WriteLine("WARN >> " + p);
            else
                Console.Write(p);
            Console.ForegroundColor = original;
        }

        private static void WriteInfo(string p, bool displayInfo)
        {
             if(displayInfo)
                Console.WriteLine("INFO >> " + p);
            else
                Console.Write(p);
        }

        private static void WriteInfo(string p)
        {
            WriteInfo(p, true);
        }

        private static void WriteError(string content)
        {
            ConsoleColor original = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("ERROR >> " + content);
            Console.ForegroundColor = original;
        }
    }
}
