using System;
using LogManager;


namespace GetLogs
{
    class Program
    {
        static void Main()
        {
            try
            {
                var dateTime = ChooseDate();
                var downloader = new Downloader();

                var runEnvironment = ChooseRunEnvironment();
                Console.WriteLine("Inserire la stringa da cercare");
                var text = Console.ReadLine();
                downloader.SaveLogs(dateTime, runEnvironment, text, timeSpecified);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadKey();
            }
        }

        private static WebScratcher.RunEnvironment ChooseRunEnvironment()
        {
            Console.Write("Selezionare un Ambiente [T(1),PP(2),PR(3)] ");
            var runEnvironment = WebScratcher.RunEnvironment.Test;
            switch (Console.ReadLine().ToUpper())
            {
                case "1":
                    runEnvironment = WebScratcher.RunEnvironment.Test;
                    break;
                case "2":
                    runEnvironment = WebScratcher.RunEnvironment.Preprod;
                    break;
                case "3":
                    runEnvironment = WebScratcher.RunEnvironment.Prod;
                    break;
            }

            return runEnvironment;
        }
        static bool timeSpecified;
        private static DateTime ChooseDate()
        {
            Console.WriteLine("Scaricare log [C]orrente o in una specifica [D]ata? ");

            DateTime dateTime;
            var readLine = Console.ReadLine();
            if (readLine == null)
            {
                throw new ArgumentException("Input errato: bye");
            }

            switch (readLine.ToUpper())
            {
                case "C":
                    dateTime = DateTime.Now;
                    break;
                case "D":
                    Console.Write("Selezionare Anno ");
                    var anno = Int32.Parse(Console.ReadLine());
                    Console.Write("Selezionare Mese ");
                    var mese = Int32.Parse(Console.ReadLine());
                    Console.Write("Selezionare Giorno ");
                    var giorno = Int32.Parse(Console.ReadLine());
                    Console.Write("Selezionare Ora ");
                    var oras = Console.ReadLine();
                    int ora;
                    timeSpecified = Int32.TryParse(oras,out ora);

                    Console.Write("Selezionare Minuto ");
                    var mins = Console.ReadLine();
                    int minuto;

                    timeSpecified &= Int32.TryParse(mins, out minuto);
                    dateTime = new DateTime(anno, mese, giorno, ora, minuto, 0);
                    break;
                default:
                    throw new ArgumentException("Input errato: bye");
            }
            //dateTime = new DateTime(2016, 5, 16, 1, 1, 0);

            return dateTime;
        }
    }
}
