using System.Text.Json;

namespace programs
{
    public class Question
    {
        public string? question { get; set; }
        public List<string>? options { get; set; }
        public string? answer { get; set; }
        public static List<Question> Start(string path) {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<Question>>(json)!;
        }
    }
}
