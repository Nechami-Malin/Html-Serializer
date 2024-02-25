using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;
using Practicode2;
using System.Xml.Linq;

public class Program
{
    static async Task Main(string[] args)
    {
        HtmlHelper _htmlHelper = HtmlHelper.Instance;
        var html = await Load("https://learn.malkabruk.co.il/practicode/projects/pract-2/#_4");

        var cleanHtml = new Regex("([\\r\\n\\t\\v\\f]+)").Replace(html, "");

        var htmlLines = new Regex("<(.*?)>").Split(cleanHtml).Where(s => s.Length > 0);

        // סינון נוסף: בדיקה האם השורה היא רצף של רווחים בלבד והחלפתה במחרוזת ריקה
        var finalLines = htmlLines.Select(line =>
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return string.Empty;
            }
            else
            {
                return line;
            }
        }).ToList();
     
        // Build HTML tree

        HtmlElement rootElement = BuildTree(finalLines, _htmlHelper);
        var lst = rootElement.Descendants();
        var lst2 = rootElement.Children[0].Ancestors();

        string str = "h3#_5";
        var se = rootElement.FindElementsBySelector(str);
        foreach (var element in se)
        {
            Console.WriteLine($"Selected Element: <{element.Name}>");
        }
        Console.ReadKey();
    }

    static async Task<string> Load(string url)
    {
        HttpClient client = new HttpClient();
        var response = await client.GetAsync(url);
        var html = await response.Content.ReadAsStringAsync();
        return html;
    }

    static HtmlElement BuildTree(IEnumerable<string> htmlLines, HtmlHelper htmlHelper)
    {
        var rootElement = new HtmlElement();
        var currentElement = rootElement;

        foreach (var line in htmlLines)
        {
            if (line == "")
                continue;
            string tagName = line.Split(' ')[0];
            var attributes = new Regex("([^\\s]*?)=\"(.*?)\"").Matches(line);

            if (tagName == "html/")
            {
                // Reached end of HTML
                break;
            }
            else if (tagName.StartsWith("/"))
            {
                // Closing tag
                currentElement = currentElement?.Parent;
            }
            else if(htmlHelper.WithoutClosingTags.Contains(tagName)|| htmlHelper.Tags.Contains(tagName))
            {
                // New tag
                var newElement = new HtmlElement
                {
                    Name = tagName,
                    Parent = currentElement
                };

                // Parse attributes

                if (attributes != null)
                {
                    var attributesLst = attributes.ToList();
                    for (int i = 0; i < attributesLst.Count; i++)
                    {

                        if (attributesLst[i].ToString().Contains('='))
                        {
                            var attributeParts = attributesLst[i].ToString().Split('=');
                            string attributeName = attributeParts[0];
                            string value = attributeParts[1];

                            if (attributeName.ToLower() == "class")
                            {
                                // Parse and add classes
                                string[] classes = value.Split(' ');
                                newElement.Classes.AddRange(classes);
                            }
                            else if (attributeName.ToLower() == "id")
                            {
                                // Set Id
                                newElement.Id = value;
                            }
                            else
                            {
                                // Add other attributes
                                if (!newElement.Attributes.ContainsKey(attributeName))
                                {
                                    newElement.Attributes.Add(attributeName, value);
                                }

                            }
                        }
                    }
                }

                currentElement?.Children.Add(newElement);
                currentElement = newElement;

                // Check if self-closing tag
                if (line.EndsWith("/") || htmlHelper.WithoutClosingTags.Contains(tagName))
                {
                    currentElement = currentElement?.Parent;
                }
            }
            else
            {
                if(currentElement != null)
                    currentElement.InnerHtml += line;
            }

        }
        return rootElement;
    }
    static void PrintTree(HtmlElement rootElement, HtmlHelper htmlHelper, string indent = "")
    {
        // Print current element
        Console.WriteLine($"{indent}<{rootElement.Name}>");

        // Print attributes if any
        if (rootElement.Attributes.Count > 0)
        {
            foreach (var attribute in rootElement.Attributes)
            {
                Console.WriteLine($"{indent}  {attribute.Key}=\"{attribute.Value}\"");
            }
        }

        // Print classes if any
        if (rootElement.Classes.Count > 0)
        {
            Console.WriteLine($"{indent}  Class: {string.Join(", ", rootElement.Classes)}");
        }

        // Print inner HTML if any
        if (!string.IsNullOrWhiteSpace(rootElement.InnerHtml))
        {
            Console.WriteLine($"{indent}  Inner HTML: {rootElement.InnerHtml}");
        }

        // Recursively print children
        foreach (var child in rootElement.Children)
        {
            PrintTree(child, htmlHelper, indent + "  ");
        }

        // Print closing tag if not a self-closing tag
        if (!htmlHelper.WithoutClosingTags.Contains(rootElement.Name))
        {
            Console.WriteLine($"{indent}</{rootElement.Name}>");
        }
    }



}
