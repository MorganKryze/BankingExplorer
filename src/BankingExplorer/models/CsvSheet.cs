namespace BankingExplorer;

public class CsvSheet
{
    public List<string> DataAsList = new();
    public readonly List<List<string>> originalMatrix = new();
    readonly List<List<string>> updatedMatrix = new();
    readonly List<int> maxLengths = new();
    readonly string month;

    #region Constants

    public readonly List<string> HEADERS = new() { "Id", "Date", "Amount", "Tag", "Notes" };

    #endregion

    public CsvSheet(string pathCsv)
    {
        if (!File.Exists(pathCsv))
        {
            var file = File.Create(pathCsv);
            file.Write(Encoding.UTF8.GetBytes("id,date,amount,tag,note\n"));
            file.Close();
        }
        month = pathCsv;
        List<CsvLine> values = new();
        var headers = CsvLine.HeaderToStringList();

        using (var reader = new StreamReader(pathCsv))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            csv.Context.RegisterClassMap<CsvMap>();
            var records = csv.GetRecords<CsvLine>().ToList();
            foreach (var record in records)
                values.Add(record);
        }
        for (int i = 0; i < headers.Count; i++)
        {
            var localMax = headers[i].Length;
            foreach (var value in values)
                if (value.ValuesToStringList()[i].Length > localMax)
                    localMax = value.ValuesToStringList()[i].Length;
            maxLengths.Add(localMax);
        }

        originalMatrix.Add(headers);
        foreach (var value in values)
            originalMatrix.Add(value.ValuesToStringList());
        Config();
    }

    public void Config()
    {
        for (int i = 0; i < originalMatrix.Count; i++)
        {
            updatedMatrix.Add(new List<string>());
            for (int j = 0; j < originalMatrix[i].Count; j++)
                updatedMatrix[i].Add(originalMatrix[i][j]);
        }

        var margin = 2;
        for (int i = 0; i < originalMatrix.Count; i++)
        for (int j = 0; j < originalMatrix[i].Count; j++)
            updatedMatrix[i][j] = originalMatrix[i][j].PadRight(maxLengths[j] + margin);

        for (int i = 0; i < updatedMatrix.Count; i++)
        {
            var localString = "";
            for (int j = 0; j < updatedMatrix[i].Count; j++)
            {
                localString += $"│ {updatedMatrix[i][j]} ";
            }
            localString += "│";
            DataAsList.Add(localString);
        }
        DataAsList.Add("└".PadRight(DataAsList[0].Length - 1, '─') + "┘");
    }

    public void AddLine(CsvLine line)
    {
        var lastIndex = originalMatrix[^1][0];
        if (lastIndex == "id")
            line.id = 0;
        else
            line.id = int.Parse(lastIndex) + 1;
        originalMatrix.Add(line.ValuesToStringList());
        SaveAndWrite();
    }

    public void RemoveLine(int index)
    {
        originalMatrix.RemoveAt(index + 1);
        for (int i = 1; i < originalMatrix.Count; i++)
            originalMatrix[i][0] = (i - 1).ToString();
        SaveAndWrite();
    }

    public CsvLine GetLine(int index)
    {
        return new CsvLine
        {
            id = int.Parse(originalMatrix[index + 1][0]),
            date = originalMatrix[index + 1][1],
            amount = float.Parse(originalMatrix[index + 1][2]),
            tag = originalMatrix[index + 1][3],
            note = originalMatrix[index + 1][4] == "\n" ? null : originalMatrix[index + 1][4]
        };
    }

    public void SaveAndWrite()
    {
        var dataToWrite = new List<CsvLine>();
        for (int i = 1; i < originalMatrix.Count; i++)
        {
            var line = new CsvLine
            {
                id = int.Parse(originalMatrix[i][0]),
                date = originalMatrix[i][1],
                amount = float.Parse(originalMatrix[i][2]),
                tag = originalMatrix[i][3],
                note = originalMatrix[i][4] == "\n" ? null : originalMatrix[i][4]
            };
            dataToWrite.Add(line);
        }
        dataToWrite = dataToWrite
            .OrderBy(x =>
            {
                DateTime.TryParseExact(
                    x.date,
                    "dd.MM.yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime temp
                );
                return temp;
            })
            .ToList();
        for (int i = 0; i < dataToWrite.Count; i++)
            dataToWrite[i].id = i;
        using (var writer = new StreamWriter(month))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.Context.RegisterClassMap<CsvMap>();
            csv.WriteRecords(dataToWrite);
        }
    }

    public static void CsvSheetExistsCheck(string path)
    {
        if (!File.Exists(path))
        {
            var file = File.Create(path);
            file.Write(Encoding.UTF8.GetBytes("id,date,amount,tag,note\n"));
            file.Close();
        }
    }
}
