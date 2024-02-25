using System;
using System.IO;
using System.Text.Json;

namespace Practicode2
{
    public class HtmlHelper
    {

        private readonly static HtmlHelper _instance = new HtmlHelper();
        public static HtmlHelper Instance => _instance;
        public string[] Tags { get; set; }
        public string[] WithoutClosingTags { get; set; }

        private HtmlHelper()
        {
            LoadTags();
        }

        private void LoadTags()
        {
            try
            {
                // קריאת קובץ JSON שמכיל את רשימת תגיות HTML
                string tagsJson = File.ReadAllText("HtmlTags.json");
                // קריאת קובץ JSON שמכיל את רשימת תגיות HTML שאינן מצריכות סגירה
                string withoutClosingTagsJson = File.ReadAllText("HtmlVoidTags.json");

                // המרת הנתונים מ JSON למערך של string באמצעות JsonSerializer
                Tags = JsonSerializer.Deserialize<string[]>(tagsJson);
                WithoutClosingTags = JsonSerializer.Deserialize<string[]>(withoutClosingTagsJson);
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error deserializing JSON: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}
