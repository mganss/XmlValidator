# XmlValidator

XmlValidator is a commandline tool to validate XML files against one or more XML schema files (.xsd files). It exposes the validation capabilities built into the .NET Framework, [XmlSchemaSet](https://msdn.microsoft.com/en-us/library/system.xml.schema.xmlschemaset.aspx) and [XmlReader](https://msdn.microsoft.com/en-us/library/system.xml.xmlreader.aspx) in particular.

## Usage

```
Usage: XmlValidator [OPTIONS]+ xmlFile...
Validate XML files against XML schemas.
xmlFiles may contain globs, e.g. "content\{xml,files}\**\*.xml".

Options:
  -h, --help                 show this message and exit
  -s, --schema=VALUE         XML Schemas to validate against (may contain globs,
                                may occur more than once for multiple schemas).
  -w, --warn                 also report warnings
```
