using CsvHelper.Configuration;

namespace program.models
{
    public class CsvMap : ClassMap<CsvLine>
    {
        public CsvMap()
        {
            Map(m => m.id).Name("id");
            Map(m => m.date).Name("date");
            Map(m => m.amount).Name("amount");
            Map(m => m.tag).Name("tag");
            Map(m => m.note).Name("note");
        }
    }
}