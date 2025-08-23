using System.Xml.Serialization;

namespace Application.Models;

[XmlRoot("Container")]
public class ContainerXml
{
    [XmlElement("Id")] public string Id { get; set; } = string.Empty;

    [XmlElement("ShippingDate")] public DateTime ShippingDate { get; set; }

    [XmlArray("parcels")]
    [XmlArrayItem("Parcel")]
    public List<ParcelXml> Parcels { get; set; } = [];
}

public class ParcelXml
{
    [XmlElement("Receipient")] public RecipientXml Recipient { get; set; } = new();

    [XmlElement("Weight")] public decimal Weight { get; set; }

    [XmlElement("Value")] public decimal Value { get; set; }
}

public class RecipientXml
{
    [XmlElement("Name")] public string Name { get; set; } = string.Empty;

    [XmlElement("Address")] public AddressXml Address { get; set; } = new();
}

public class AddressXml
{
    [XmlElement("Street")] public string Street { get; set; } = string.Empty;

    [XmlElement("HouseNumber")] public string HouseNumber { get; set; } = string.Empty;

    [XmlElement("PostalCode")] public string PostalCode { get; set; } = string.Empty;

    [XmlElement("City")] public string City { get; set; } = string.Empty;
}