using System.Xml;
using System.Xml.Schema;

class Program
{
    static void Main()
    {
        Console.WriteLine("START");
        if (!Directory.Exists(".\\config"))
        {
            Console.WriteLine("Brak scheme do walidacji.");
            Console.ReadKey(true);
            return;
        }
        string path = ".\\check";
        if (!File.Exists(".\\config\\path.txt"))
        {
            Console.WriteLine("Brak alternatywnej ścieżki. Walidacja w folderze '" + path + "'.");

        }
        else
        {
            string readPath = File.ReadAllText(".\\config\\path.txt").Trim();
            if (!string.IsNullOrEmpty(readPath))
            {
                path = readPath;
            }
            Console.WriteLine("Podano alternatywną ścieżkę. Walidacja w folderze '" + path + "'.");
        }

        string[] xsds = Directory.GetFiles("./config", "*.xsd");
        if (!Directory.Exists(path))
        {
            Console.WriteLine("Folder z fakturami do sprawdzenia nie istnieje.");
            Console.ReadKey(true);
            return;
        }
        string[] xmls = Directory.GetFiles(path, "*.xml");
        if (xmls.Length == 0)
        {
            Console.WriteLine("Folder z fakturami do sprawdzenia jest pusty.");
            Console.ReadKey(true);
            return;
        }
        XmlSchemaSet schemas = new XmlSchemaSet
        {
            XmlResolver = new XmlUrlResolver()
        };
        string mainSchemaPath = "./config/scheme.xsd";
        using (var reader = XmlReader.Create(mainSchemaPath))
        {
            schemas.Add(null, reader);
        }
        schemas.Compile();
        foreach (string xmlPath in xmls)
        {
            Console.WriteLine($"Sprawdzanie Faktury {xmlPath}:");
            try
            {
                int errorCount = 0;
                int warningCount = 0;
                XmlReaderSettings settings = new XmlReaderSettings
                {
                    ValidationType = ValidationType.Schema,
                    Schemas = schemas
                };
                settings.ValidationEventHandler += (_, e) =>
{
    Console.WriteLine($"{e.Severity}: {e.Message}");

    if (e.Severity == XmlSeverityType.Error)
        errorCount++;
    if (e.Severity == XmlSeverityType.Warning)
        warningCount++;
};
                using XmlReader reader = XmlReader.Create(xmlPath, settings);
                while (reader.Read()) { }
                if (errorCount > 0 || warningCount > 0)
                {
                    Console.WriteLine($"XML NIE jest poprawny względem XSD. Zawiera {errorCount} błędów i {warningCount} ostrzeżęń");
                }
                else
                {
                    Console.WriteLine($"XML jest poprawny względem XSD.");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Błąd walidacji:");
                Console.WriteLine(ex.Message);
            }
        }
        Console.ReadKey(true);
        return;
    }
}