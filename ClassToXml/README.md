# ClassToXml

Serialize a C# class hierarchy to XML (and back) using `XmlSerializer`.

Highlights:
- `XmlRoot`, `XmlElement`, `XmlAttribute` attributes
- Serialize to string/file and deserialize from string
- Simple namespace demo via `XmlSerializerNamespaces`

Try it:
- Build and run the project. Inspect `invoice.xml` in the output folder.

Extend:
- Add lists (`List<T>`) with `[XmlArray]/[XmlArrayItem]`
- Use `[XmlIgnore]` for fields you don’t want in XML
- Customize element names and ordering
- Add XSD and validate serialized XML
