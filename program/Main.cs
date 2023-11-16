using System.Globalization;
using System.Text;
using ConsoleAppVisuals;
using CsvHelper;

using program.models;
using program.ressources;

namespace program
{
    class Program
    {
        readonly static string s_TITLE_PATH = "ressources/title.txt";
        readonly static string s_ACCOUNTS_PATH = "../data/accounts/";
        private static string s_selectedAccount = "";
        private static string s_selectedMonth = "";
        private static Jump s_Jump = Jump.Home;
        private static (Jump,Jump) s_JumpTuple = (Jump.Exit, Jump.Home);
        private static CsvSheet? sheet;
        private static CsvLine? line;

        static void Main()
        {
            Config();

            Home:
            Home();
            goto Selection;

            Account_Selection:
            Accounts();
            goto Selection;

            Months_Selection:
            Months();
            goto Selection;

            Display_Sheet:
            line = Sheet();
            goto Selection;

            Add:
            Add();
            goto Selection;

            Update:
            Update();
            goto Selection;

            Archive:
            Archive();
            goto Selection;
            
            Exit:
            Exit();

            Selection:
            switch(s_Jump)
            {
                case Jump.Home:
                    goto Home;
                case Jump.Accounts:
                    goto Account_Selection;
                case Jump.Months:
                    goto Months_Selection;
                case Jump.Sheet:
                    goto Display_Sheet;
                case Jump.Add:
                    goto Add;
                case Jump.Update:
                    goto Update;
                case Jump.Archive:
                    goto Archive;
                case Jump.Exit:
                    goto Exit;
            }
        }
        static void Config()
        {
            Core.LoadTitle(s_TITLE_PATH);
            Core.SetDefaultBanner(("","",""), ("[ESC] Retour" ,"[Z|↑] Monter   [S|↓] Descendre","[ENTRER] Sélectionner"));
        }
        static void Navigation(Jump next) => s_JumpTuple = (s_JumpTuple.Item2, next);
        static void Home(Jump first = Jump.Accounts, Jump second = Jump.Archive, Jump back = Jump.Exit)
        {
            Core.WriteFullScreen(false, default, default);
            var index = Core.ScrollingMenuSelector(
                "Bienvenue dans votre gestionnaire de comptes : ", 
                default, 
                "Choisir un compte", 
                "Générer une archive",
                "Actualiser fenêtre");
            switch (index)
            {
                case 0:
                    s_Jump = first;
                    break;
                case 1:
                    s_Jump = second;
                    break;
                case 2:
                    s_Jump = Jump.Home;
                    break;
                case -1:
                    s_Jump = back;
                    break;
            }
            
        }
        static void Accounts(Jump next = Jump.Months, Jump redo = Jump.Accounts , Jump back = Jump.Home)
        {
            string[] folders = Directory.GetDirectories(s_ACCOUNTS_PATH);
            string[] folders_renamed = new string[folders.Length];
            foreach (string folder in folders)
                folders_renamed[Array.IndexOf(folders, folder)] = folder.Replace(s_ACCOUNTS_PATH, "") + (Directory.GetFiles(folder).Length == 0 ? " (vide)" : "");   
            var index = Core.ScrollingMenuSelector("Veuillez sélectionner un compte : ", default, folders_renamed);
            switch (index) {
                case -1:
                    s_Jump = back;
                    break;
                default:
                        s_selectedAccount = folders[index] + "/";
                    if (Directory.GetFiles(folders[index]).Length == 0)
                    {
                        _ =  new CsvSheet(folders[index] + "/" + DateTime.Now.ToString("MM.yyyy", CultureInfo.InvariantCulture) + ".csv");
                        s_Jump = redo;
                    }
                    else 
                        s_Jump = next;
                    break;
            }
        }
        static void Months(Jump next = Jump.Sheet, Jump back = Jump.Accounts)
        {
            string[] files = Directory.GetFiles(s_selectedAccount);
            string[] files_renamed = new string[files.Length];
            foreach (string file in files)
            {
                files_renamed[Array.IndexOf(files, file)] = file.Replace(s_selectedAccount, "");
            }
            var index = Core.ScrollingMenuSelector("Veuillez sélectionner un mois : ", default, files_renamed);
            switch (index)
            {
                case -1:
                    s_Jump = back;
                    s_selectedMonth = "";
                    break;
                default:
                    s_selectedMonth = files[index];
                    s_Jump = next;
                    break;
            }
        }
        static CsvLine? Sheet(Jump add = Jump.Add, Jump update = Jump.Update, Jump back = Jump.Months, Jump again = Jump.Sheet)
        {
            sheet = new CsvSheet(s_selectedMonth);
            var index = Tools.ScrollingTableSelector(sheet.DataAsList[0], default, true, sheet.DataAsList.GetRange(1, sheet.DataAsList.Count - 1).ToArray());
            if (index.Item1 == -1)
            {
                s_Jump = back;
                return null;
            }
            else if (index.Item1 == -2)
            {
                if (index.Item2 == sheet.DataAsList.Count - 2){
                    s_Jump = again;
                    return null;
                }
                switch(Core.ScrollingMenuSelector("Êtes-vous sûr de vouloir supprimer cet élément ?", default, "Oui", "Non"))
                {
                    case 0:
                        sheet?.RemoveLine(index.Item2);
                        s_Jump = again;
                        break;
                    case 1: case -1:
                        s_Jump = again;
                        break;
                }
                return null;
            }
            else if (index.Item2 == sheet.DataAsList.Count - 2)
            {
                s_Jump = add;
                return null;
            }
            else
            {
                s_Jump = update;
                return sheet?.GetLine(index.Item2);
            }
        }
        static void Add(Jump back = Jump.Sheet)
        {
            var newLine = new CsvLine();

            New_Date:
            var date = Tools.WritePromptDefaultValue("Veuillez rentrer la date : ", newLine.date is null ? DateTime.Now.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture) : newLine.date);
            if (date.Item1 == -1)
            {
                Core.ClearMultipleLines(Core.ContentHeigth, 5);
                s_Jump = back;
                return;
            }
            try {
                newLine.date = DateTime.Parse(date.Item2).ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
            }catch {
                goto New_Date;
            }
            Core.ClearMultipleLines(Core.ContentHeigth, 5);

            New_Amount:
            var amount = Tools.WritePromptDefaultValue("Veuillez rentrer le montant : ", newLine.amount is null ? "-" : newLine.amount.ToString());
            if (amount.Item1 == -1)
            {
                Core.ClearMultipleLines(Core.ContentHeigth, 5);
                goto New_Date;
            }
            try {
                newLine.amount = float.Parse(amount.Item2);
            }catch {
                goto New_Amount;
            }
            Core.ClearMultipleLines(Core.ContentHeigth, 5);
            
            New_Tag:
            var tags = Enum.GetValues(typeof(Tag)).Cast<Tag>().Select(tag => tag.ToString()).ToArray();
            var index = Tools.ScrollingMenuSelectorDefaultvalue("Veuillez sélectionner une catégorie : ", default, newLine.tag is null ? null : Array.IndexOf(tags,newLine.tag), tags);
            if (index == -1)
            {
                Core.ClearMultipleLines(Core.ContentHeigth, 5);
                goto New_Amount;
            }
            newLine.tag = ((Tag)index).ToString();

            var note = Tools.WritePromptDefaultValue("Veuillez rentrer une description : ", newLine.note);
            if (note.Item1 == -1)
            {
                Core.ClearMultipleLines(Core.ContentHeigth, 5);
                goto New_Tag;
            }
            newLine.note = note.Item2;
            Core.ClearMultipleLines(Core.ContentHeigth, 5);

            var month = DateTime.Parse(newLine.date).ToString("MM.yyyy", CultureInfo.InvariantCulture);
            var monthPath = s_selectedAccount + month + ".csv";
            sheet = new CsvSheet(monthPath);
            sheet?.AddLine(newLine);
            switch(Core.ScrollingMenuSelector("Voulez-vous ajouter un autre élément ?", default, "Oui", "Non"))
            {
                case 0:
                    s_Jump = Jump.Add;
                    break;
                case 1: case -1:
                    s_Jump = back;
                    break;
            }
        }
        static void Update(Jump back = Jump.Sheet)
        {
            if (line is null)
            {
                s_Jump = back;
                return;
            }

            var newLine = new CsvLine{
                id = line.id,
                date = line.date,
                amount = line.amount,
                tag = line.tag,
                note = line.note
            };
        
            New_Date:
            var date = Tools.WritePromptDefaultValue("Veuillez rentrer la date : ", newLine.date is null ? DateTime.Now.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture) : newLine.date);
            if (date.Item1 == -1)
            {
                Core.ClearMultipleLines(Core.ContentHeigth, 5);
                s_Jump = back;
                return;
            }
            try {
                newLine.date = DateTime.Parse(date.Item2).ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
            }catch {
                goto New_Date;
            }
            Core.ClearMultipleLines(Core.ContentHeigth, 5);

            New_Amount:
            var amount = Tools.WritePromptDefaultValue("Veuillez rentrer le montant : ", newLine.amount is null ? "-" : newLine.amount.ToString());
            if (amount.Item1 == -1)
            {
                Core.ClearMultipleLines(Core.ContentHeigth, 5);
                goto New_Date;
            }
            try {
                newLine.amount = float.Parse(amount.Item2);
            }catch {
                goto New_Amount;
            }
            Core.ClearMultipleLines(Core.ContentHeigth, 5);
            
            New_Tag:
            var tags = Enum.GetValues(typeof(Tag)).Cast<Tag>().Select(tag => tag.ToString()).ToArray();
            var index = Tools.ScrollingMenuSelectorDefaultvalue("Veuillez sélectionner une catégorie : ", newLine.tag is null ? null : Array.IndexOf(tags,newLine.tag), null, tags);
            if (index == -1)
            {
                Core.ClearMultipleLines(Core.ContentHeigth, 5);
                goto New_Amount;
            }
            newLine.tag = ((Tag)index).ToString();

            New_Note:
            var note = Tools.WritePromptDefaultValue("Veuillez rentrer une description : ", newLine.note);
            if (note.Item1 == -1)
            {
                Core.ClearMultipleLines(Core.ContentHeigth, 5);
                goto New_Tag;
            }
            newLine.note = note.Item2;
            Core.ClearMultipleLines(Core.ContentHeigth, 5);

            switch(Core.ScrollingMenuSelector("Confirmer ?", default, "Oui", "Non"))
            {
                case 0:
                    sheet?.RemoveLine(line.id ?? 0);
                    var month = DateTime.Parse(newLine.date).ToString("MM.yyyy", CultureInfo.InvariantCulture);
                    var monthPath = s_selectedAccount + month + ".csv";
                    sheet = new CsvSheet(monthPath);
                    sheet?.AddLine(newLine);
                    break;
                case 1: case -1:
                    goto New_Note;
            }
            s_Jump = Jump.Sheet;
        }
        static void Archive(Jump next = Jump.Home)
        {
            var archivePath = "../data/archive/";
            if (!Directory.Exists(archivePath))
                Directory.CreateDirectory(archivePath);
            else 
                Directory.Delete(archivePath, true);
            var archiveName = DateTime.Now.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
            Tools.CopyDirectory("../data/accounts/", archivePath + archiveName, true);
            Core.LoadingBar("[ Archivage en cours ... ]");
            s_Jump = next;
        }
        static void Exit() => Core.ExitProgram();
    }
    enum Jump
    {
        Home,
        Accounts,
        Months,
        Sheet,
        Add,
        Update,
        Archive,
        Exit
    }
    enum Tag
    {
        Repas,
        Loisirs,
        Restaurant,
        Voyage,
        Virement,
        Retrait,
        Transports,
        Logement,
        Autre
    }
}