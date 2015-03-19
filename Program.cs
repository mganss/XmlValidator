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
            var warn = false;

            var options = new OptionSet {
                { "h|help", "show this message and exit", v => showHelp = v != null },
                { "s|schema=", @"XML Schemas to validate against (may contain globs, may occur more than once for multiple schemas).", v => schemaFiles.Add(v) },
                { "w|warn", "also report warnings", v => warn = v != null }
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

            try
            {
                set.Compile();
            }
            catch (XmlSchemaException ex)
            {
                WriteError(ex);
            }

            var settings = new XmlReaderSettings();
            settings.Schemas = set;
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationEventHandler += ValidationEventHandler;
            if (warn)
                settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;

            foreach (var xmlFile in xmlFiles.SelectMany(f => Glob.Glob.ExpandNames(f)))
            {
                try
                {
                    var reader = XmlReader.Create(xmlFile, settings);
                    while (reader.Read()) ;
                }
                catch (XmlSchemaValidationException ex)
                {
                    WriteError(ex);
                }
                catch (XmlException ex)
                {
                    WriteError(ex.LineNumber, ex.LinePosition, ex.Message, ex.SourceUri, XmlSeverityType.Error);
                }
            }
        }

        static void ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            WriteError(e.Exception, e.Severity);
        }

        static void WriteError(XmlSchemaException ex, XmlSeverityType severity = XmlSeverityType.Error)
        {
            WriteError(ex.LineNumber, ex.LinePosition, ex.Message, ex.SourceUri, severity);
        }

        static void WriteError(int line, int col, string message, string uri, XmlSeverityType severity)
        {
            var color = Console.ForegroundColor;

            try
            {
                var file = new Uri(uri).LocalPath;
                var msg = string.Format("{3}: Line {0}, Column {1}: {2}", line, col, message, file);
                if (severity == XmlSeverityType.Warning)
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
