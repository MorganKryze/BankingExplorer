using CsvHelper.Configuration.Attributes;

namespace program.models
{
    public class CsvLine
    {
        [Name("id")]
        public int? id;
        [Name("date")]
        public string? date;
        [Name("amount")]
        public float? amount;
        [Name("tag")]
        public string? tag;
        [Name("note")]
        public string? note;

        public static List<string> HeaderToStringList() => new () {"id", "date", "amount", "tag", "note" };
        public List<string> ValuesToStringList() => new (){$"{id}", $"{date}", $"{amount}", $"{tag}", $"{note}" };
        override public string ToString() => $"{id} ; {date} ; {amount} ; {tag} ; {note}";
    }
}