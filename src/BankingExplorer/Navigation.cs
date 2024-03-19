namespace BankingExplorer;

class Navigation
{
    #region Fields
    private static string s_selectedAccount = "";
    private static string s_selectedMonth = "";
    public static (Jump, Jump) s_Jump = (Jump.Exit, Jump.Home);
    public static CsvSheet? s_Sheet;
    public static CsvLine? s_Line;
    #endregion

    #region Constants
    const string ACCOUNTS_PATH = "../../data/accounts/";
    const string ARCHIVE_PATH = "../../data/archive/";
    #endregion

    static void Main()
    {
        SetupWindow();
        Window.Render();

        while (true)
        {
            switch (s_Jump.Item2)
            {
                case Jump.Home:
                    Home();
                    break;
                case Jump.Accounts:
                    Accounts();
                    break;
                case Jump.Months:
                    Months();
                    break;
                case Jump.Sheet:
                    Display_Sheet();
                    break;
                case Jump.Update:
                    Update();
                    break;
                case Jump.Archive:
                    Archive();
                    break;
                case Jump.Exit:
                    Window.Close();
                    break;
            }
        }
    }

    #region Methods
    static void Next(Jump next) => s_Jump = (s_Jump.Item2, next);

    static void SetupWindow()
    {
        var title = new Title("Banking Explorer");
        var header = new Header("", "Welcome to the Banking Explorer", "", 2);
        var footer = new Footer("[ESC] Back", "[Z|↑] Up   [S|↓] Down", "[ENTER] Select");

        Window.AddElement(title, header, footer);
        Window.Open();
    }
    #endregion

    static void Home()
    {
        ScrollingMenu mainMenu = new ScrollingMenu(
            "Welcome to the Banking Explorer !",
            0,
            Placement.TopCenter,
            "Choose an account",
            "Archive data",
            "Quit the app"
        );
        Window.AddElement(mainMenu);
        Window.ActivateElement(mainMenu);

        var response = mainMenu.GetResponse();
        switch (response?.Status)
        {
            case Output.Selected:
                switch (response?.Value)
                {
                    case 0:
                        Next(Jump.Accounts);
                        break;
                    case 1:
                        Next(Jump.Archive);
                        break;
                    case 2:
                        Next(Jump.Exit);
                        break;
                }
                break;
            case Output.Deleted:
            case Output.Escaped:
                Next(Jump.Exit);
                break;
        }

        Window.RemoveElement(mainMenu);
    }

    static void Accounts()
    {
        string[] folders = Directory.GetDirectories(ACCOUNTS_PATH);
        string[] folders_renamed = new string[folders.Length];
        foreach (string folder in folders)
            folders_renamed[Array.IndexOf(folders, folder)] =
                folder.Replace(ACCOUNTS_PATH, "")
                + (Directory.GetFiles(folder).Length == 0 ? " (empty)" : "");

        ScrollingMenu accountsMenu = new ScrollingMenu(
            "Please select an account :",
            0,
            Placement.TopCenter,
            folders_renamed
        );
        Window.AddElement(accountsMenu);
        Window.ActivateElement(accountsMenu);

        var response = accountsMenu.GetResponse();
        switch (response?.Status)
        {
            case Output.Deleted:
            case Output.Escaped:
                Next(Jump.Home);
                break;
            case Output.Selected:
                s_selectedAccount = folders[response.Value] + "/";
                if (Directory.GetFiles(folders[response.Value]).Length == 0)
                {
                    _ = new CsvSheet(
                        folders[response.Value]
                            + "/"
                            + DateTime.Now.ToString("MM.yyyy", CultureInfo.InvariantCulture)
                            + ".csv"
                    );
                    Next(Jump.Accounts);
                }
                else
                    Next(Jump.Months);
                break;
        }

        Window.RemoveElement(accountsMenu);
    }

    static void Months()
    {
        string[] files = Directory.GetFiles(s_selectedAccount);
        string[] files_renamed = new string[files.Length];
        foreach (string file in files)
        {
            files_renamed[Array.IndexOf(files, file)] = file.Replace(s_selectedAccount, "");
        }
        ScrollingMenu monthsMenu = new ScrollingMenu(
            "Please select a month :",
            0,
            Placement.TopCenter,
            files_renamed
        );
        Window.AddElement(monthsMenu);
        Window.ActivateElement(monthsMenu);

        var response = monthsMenu.GetResponse();
        switch (response?.Status)
        {
            case Output.Deleted:
            case Output.Escaped:
                s_selectedMonth = "";
                Next(Jump.Accounts);
                break;
            case Output.Selected:
                s_selectedMonth = files[response.Value];
                Next(Jump.Sheet);
                break;
        }

        Window.RemoveElement(monthsMenu);
    }

    static void Display_Sheet()
    {
        s_Sheet = new CsvSheet(s_selectedMonth);
        TableSelector lineSelector = new TableSelector(
            "Cash flow",
            s_Sheet.HEADERS,
            s_Sheet.originalMatrix.GetRange(1, s_Sheet.originalMatrix.Count - 1),
            true,
            false,
            "Add an element",
            Placement.TopCenter
        );
        Window.AddElement(lineSelector);
        Window.ActivateElement(lineSelector);

        var response = lineSelector.GetResponse();
        switch (response?.Status)
        {
            case Output.Escaped:
                Next(Jump.Months);
                break;

            case Output.Selected:
                if (response.Value == s_Sheet.originalMatrix.Count - 1)
                {
                    Next(Jump.Update);
                    break;
                }
                s_Line = s_Sheet.GetLine(response.Value);
                Next(Jump.Update);
                break;

            case Output.Deleted:
                if (response.Value == s_Sheet.originalMatrix.Count - 1)
                {
                    Next(Jump.Update);
                    break;
                }
                ScrollingMenu deleteConfirmationMenu = new ScrollingMenu(
                    "Are you sure you want to delete this element ?",
                    0,
                    Placement.TopCenter,
                    "Yes",
                    "No"
                );
                Window.AddElement(deleteConfirmationMenu);
                Window.ActivateElement(deleteConfirmationMenu);

                var deleteResponse = deleteConfirmationMenu.GetResponse();
                switch (deleteResponse?.Status)
                {
                    case Output.Selected:
                        if (deleteResponse.Value == 0)
                        {
                            s_Sheet.RemoveLine(response.Value);
                            Next(Jump.Sheet);
                        }
                        else
                            Next(Jump.Sheet);
                        break;
                    case Output.Deleted:
                    case Output.Escaped:
                        Next(Jump.Sheet);
                        break;
                }
                break;
        }
        Window.RemoveElement(lineSelector);
    }

    static void Update()
    {
        CsvLine line = s_Line ?? new CsvLine();

        # region New_Date
        New_Date:

        Prompt new_date = new Prompt(
            "Please enter the date (dd.MM.yyyy):",
            line.date is null
                ? DateTime.Now.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture)
                : line.date,
            Placement.TopCenter,
            20
        );
        Window.AddElement(new_date);
        Window.ActivateElement(new_date);

        var date = new_date.GetResponse();
        if (date?.Status == Output.Escaped)
        {
            Next(Jump.Sheet);
            Window.RemoveElement(new_date);
            s_Line = null;
            return;
        }

        if (
            DateTime.TryParseExact(
                date?.Value,
                "dd.MM.yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime date_input
            )
        )
        {
            line.date = date_input.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
        }
        else
        {
            Window.RemoveElement(new_date);
            goto New_Date;
        }

        Window.RemoveElement(new_date);
        #endregion

        #region New_Amount
        New_Amount:

        Prompt new_amount = new Prompt(
            "Please enter the amount :",
            line.amount is null ? "-" : line.amount.ToString(),
            Placement.TopCenter,
            10
        );
        Window.AddElement(new_amount);
        Window.ActivateElement(new_amount);

        var amount = new_amount.GetResponse();
        if (amount?.Status == Output.Escaped)
        {
            Window.RemoveElement(new_amount);
            goto New_Date;
        }
        try
        {
            if (amount is null)
            {
                throw new NullReferenceException();
            }
            line.amount = float.Parse(amount.Value);
        }
        catch
        {
            Window.RemoveElement(new_amount);
            goto New_Amount;
        }
        Window.RemoveElement(new_amount);
        #endregion

        #region New_Tag
        New_Tag:

        var tags = Enum.GetValues(typeof(Tag)).Cast<Tag>().Select(tag => tag.ToString()).ToArray();
        ScrollingMenu new_tag = new ScrollingMenu(
            "Please select a tag :",
            line.tag is null ? 0 : (int)Enum.Parse(typeof(Tag), line.tag),
            Placement.TopCenter,
            tags
        );
        Window.AddElement(new_tag);
        Window.ActivateElement(new_tag);

        var tag = new_tag.GetResponse();
        if (tag?.Status == Output.Escaped)
        {
            Window.RemoveElement(new_tag);
            goto New_Amount;
        }

        // Tag will not be null, but need to avoid nullable warning
        if (tag is null)
        {
            Window.RemoveElement(new_tag);
            goto New_Tag;
        }
        line.tag = ((Tag)tag.Value).ToString();
        Window.RemoveElement(new_tag);
        #endregion

        #region New_Note
        Prompt newt_note = new Prompt(
            "Please enter a few notes:",
            line.note,
            Placement.TopCenter,
            25
        );
        Window.AddElement(newt_note);
        Window.ActivateElement(newt_note);

        var note = newt_note.GetResponse();
        if (note?.Status == Output.Escaped)
        {
            Window.RemoveElement(newt_note);
            goto New_Tag;
        }
        line.note = note?.Value;
        Window.RemoveElement(newt_note);
        #endregion

        #region Save the element
        string month;
        if (
            DateTime.TryParseExact(
                line.date,
                "dd.MM.yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime date_save
            )
        )
        {
            month = date_save.ToString("MM.yyyy", CultureInfo.InvariantCulture);
        }
        else
        {
            throw new FormatException("Invalid date format");
        }
        if (s_Line is not null)
        {
            s_Sheet?.RemoveLine(line.id ?? 0);
        }
        string monthPath = s_selectedAccount + month + ".csv";
        s_Sheet = new CsvSheet(monthPath);
        s_Sheet?.AddLine(line);
        #endregion

        #region Repeat the process?
        if (s_Line is null)
        {
            ScrollingMenu confirmation = new ScrollingMenu(
                "Element added !",
                0,
                Placement.TopCenter,
                "Add another element",
                "Back to the sheet"
            );
            Window.AddElement(confirmation);
            Window.ActivateElement(confirmation);

            var confirm = confirmation.GetResponse();
            switch (confirm?.Status)
            {
                case Output.Selected:
                    if (confirm.Value == 0)
                    {
                        Next(Jump.Update);
                        break;
                    }
                    Next(Jump.Sheet);
                    break;
                case Output.Deleted:
                case Output.Escaped:
                    Next(Jump.Sheet);
                    break;
            }
            Window.RemoveElement(confirmation);
        }
        Next(Jump.Sheet);

        s_Line = null;
        #endregion
    }

    static void Archive()
    {
        if (!Directory.Exists(ARCHIVE_PATH))
            Directory.CreateDirectory(ARCHIVE_PATH);
        else
            Directory.Delete(ARCHIVE_PATH, true);

        var archiveName = DateTime.Now.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
        CopyDirectory(ACCOUNTS_PATH, ARCHIVE_PATH + archiveName, true);

        FakeLoadingBar loading = new FakeLoadingBar("[ Archiving ... ]");
        Window.AddElement(loading);
        Window.ActivateElement(loading);

        Window.RemoveElement(loading);
        Next(Jump.Home);

        void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            var dir = new DirectoryInfo(sourceDir);
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");
            DirectoryInfo[] dirs = dir.GetDirectories();
            Directory.CreateDirectory(destinationDir);
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath);
            }
            if (recursive)
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
        }
    }
}
