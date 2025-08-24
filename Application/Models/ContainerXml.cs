using System.Xml.Serialization;

namespace Application.Models;

/// <summary>
/// XML model for deserializing container data from XML import files
/// </summary>
[XmlRoot("Container")]
public class ContainerXml
{
    /// <summary>
    /// Business identifier of the container from XML
    /// </summary>
    [XmlElement("Id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Shipping date of the container from XML
    /// </summary>
    [XmlElement("ShippingDate")]
    public DateTime ShippingDate { get; set; }

    /// <summary>
    /// Collection of parcels contained in this container
    /// </summary>
    [XmlArray("parcels")]
    [XmlArrayItem("Parcel")]
    public List<ParcelXml> Parcels { get; set; } = new();
}

/// <summary>
/// XML model for deserializing parcel data from XML import files
/// </summary>
public class ParcelXml
{
    /// <summary>
    /// Recipient information for the parcel
    /// </summary>
    [XmlElement("Receipient")] // Note: Dutch spelling used in XML
    public RecipientXml Recipient { get; set; } = new();

    /// <summary>
    /// Weight of the parcel in kilograms
    /// </summary>
    [XmlElement("Weight")]
    public decimal Weight { get; set; }

    /// <summary>
    /// Monetary value of the parcel contents
    /// </summary>
    [XmlElement("Value")]
    public decimal Value { get; set; }
}

/// <summary>
/// XML model for deserializing recipient data from XML import files
/// </summary>
public class RecipientXml
{
    /// <summary>
    /// Full name of the recipient
    /// </summary>
    [XmlElement("Name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Address information of the recipient
    /// </summary>
    [XmlElement("Address")]
    public AddressXml Address { get; set; } = new();
}

/// <summary>
/// XML model for deserializing address data from XML import files
/// </summary>
public class AddressXml
{
    /// <summary>
    /// Street name from XML
    /// </summary>
    [XmlElement("Street")]
    public string Street { get; set; } = string.Empty;

    /// <summary>
    /// House or building number from XML
    /// </summary>
    [XmlElement("HouseNumber")]
    public string HouseNumber { get; set; } = string.Empty;

    /// <summary>
    /// Postal code in Dutch format from XML
    /// </summary>
    [XmlElement("PostalCode")]
    public string PostalCode { get; set; } = string.Empty;

    /// <summary>
    /// City name from XML
    /// </summary>
    [XmlElement("City")]
    public string City { get; set; } = string.Empty;
}