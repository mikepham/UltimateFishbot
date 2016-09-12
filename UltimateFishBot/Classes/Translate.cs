namespace UltimateFishBot.Classes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;

    using UltimateFishBot.Properties;

    internal class Translate
    {
        private static XmlElement m_elements;

        public static string GetTranslate(string formName, string nodeName, params object[] list)
        {
            ExtractElements();
            var returnText = "MISSING TRANSLATION";

            // If we can't open the Translation file, everything will appear as "MISSING TRANSLATION"
            if (m_elements == null) return returnText;

            try
            {
                var formList = m_elements.GetElementsByTagName(formName);

                // Try to find the correct translation for formName and nodeName
                foreach (XmlNode mainNode in formList)
                    foreach (XmlNode node in mainNode.ChildNodes)
                        if (node.Name == nodeName)
                        {
                            returnText = node.InnerText;
                            break;
                        }

                // Remove the extras spaces from each lines
                returnText = string.Join("\n", returnText.Split('\n').Select(s => s.Trim()));

                // Replace {int} in text by variables. Ex : "Waiting for Fish ({0}/{1}s) ..."
                returnText = string.Format(returnText, list);
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }

            return returnText;
        }

        public static List<string> GetTranslates(string formName, string nodeName, params object[] list)
        {
            ExtractElements();
            var returnList = new List<string>();

            // If we can't open the Translation file, everything will appear as "MISSING TRANSLATION"
            if (m_elements == null)
            {
                returnList.Add("MISSING_TRANSLATION");
                return returnList;
            }

            try
            {
                var formList = m_elements.GetElementsByTagName(formName);

                // Try to find the correct translation for formName and nodeName
                foreach (XmlNode mainNode in formList) foreach (XmlNode node in mainNode.ChildNodes) if (node.Name == nodeName) returnList.Add(node.InnerText);

                // Remove the extras spaces from each lines
                returnList.Select(text => string.Join("\n", text.Split('\n').Select(s => s.Trim())));

                // Replace {int} in text by variables. Ex : "Waiting for Fish ({0}/{1}s) ..."
                returnList.Select(text => string.Format(text, list));
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }

            return returnList;
        }

        private static void ExtractElements()
        {
            if (m_elements == null)
            {
                var doc = new XmlDocument();

                try
                {
                    // Example : ./Resources/English.xml
                    doc.Load("./Resources/" + Settings.Default.Language + ".xml");
                    m_elements = doc.DocumentElement;
                }
                catch (Exception ex)
                {
                    Console.Out.WriteLine(ex.Message);
                }
            }
        }
    }
}