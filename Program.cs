using Mono.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace XmlValidator
{
    class Program
    {
        static void Main(string[] args)
        {
            var showHelp = false;
            var schemaFiles = new List<string>();

            var options = new OptionSet {
                { "h|help", "show this message and exit", v => showHelp = v != null },
                { "s|schema=", @"XML Schema to validate against (may contain globs).", v => schemaFiles.Add(v) },
            };

            var xmlFiles = options.Parse(args);

            if (showHelp)
            {
                ShowHelp(options);
                return;
            }

            schemaFiles = schemaFiles.SelectMany(f => Glob.Glob.ExpandNames(f)).ToList();

            var set = new XmlSchemaSet();

            var schemas = schemaFiles.Select(f => XmlSchema.Read(XmlReader.Create(f, new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore }), 
                ValidationEventHandler));

            foreach (var s in schemas)
            {
                set.Add(s);
            }

            set.Compile();

            var settings = new XmlReaderSettings();
            settings.Schemas = set;
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationEventHandler += ValidationEventHandler;

            foreach (var xmlFile in xmlFiles.SelectMany(f => Glob.Glob.ExpandNames(f)))
            {
                var reader = XmlReader.Create(xmlFile, settings);
                while (reader.Read()) ;
            }
        }

        static void ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            var color = Console.ForegroundColor;

            try
            {
                var ex = e.Exception;
                var file = new Uri(ex.SourceUri).LocalPath;
                var msg = string.Format("{3}: Line {0}, Column {1}: {2}", ex.LineNumber, ex.LinePosition, e.Message, file);
                if (e.Severity == XmlSeverityType.Warning)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Error.WriteLine("Warning: " + msg);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Error.WriteLine("Error: " + msg);
                }
            }
            finally
            {
                Console.ForegroundColor = color;
            }
        }

        static void ShowHelp(OptionSet p)
        {
            System.Console.WriteLine("Usage: XmlValidator [OPTIONS]+ xmlFile...");
            System.Console.WriteLine("Validate XML files against XML schemas.");
            System.Console.WriteLine(@"xmlFiles may contain globs, e.g. ""content\{xml,files}\**\*.xml"".");
            System.Console.WriteLine();
            System.Console.WriteLine("Options:");
            p.WriteOptionDescriptions(System.Console.Out);
        }
    }
}
