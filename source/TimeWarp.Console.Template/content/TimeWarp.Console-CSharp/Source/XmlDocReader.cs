namespace Console_CSharp
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Reflection;
  using System.Xml.Linq;

  internal class XmlDocReader
  {
    private IEnumerable<XElement> MemberElements { get; }

    public XmlDocReader(string aPath)
    {
      XDocument xDocument;
      using (TextReader reader = File.OpenText(aPath))
      {
        xDocument = XDocument.Load(reader);
      }

      MemberElements = xDocument.Root.Descendants("member");
    }

    public string GetDescriptionForName(string aName)
    {
      XElement memberElement = MemberElements.First(aElement => aElement.Attribute("name").Value == aName);
      XElement summary = memberElement.Descendants("summary").FirstOrDefault();
      return summary.Value;
    }

    public string GetDescriptionForPropertyInfo(PropertyInfo aPropertyInfo) =>
          GetDescriptionForName($"P:{aPropertyInfo.DeclaringType.FullName}.{aPropertyInfo.Name}");

    public string GetDescriptionForType(Type aType) => GetDescriptionForName($"T:{aType.FullName}");
  }
}