using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebScraper.Database.Entities;

[Table("Notices")]
public class Notice
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    /// <summary>
    /// Identifier from ted.europa.eu
    /// </summary>
    public string PublicationNumber { get; set; }
    
    /// <summary>
    /// Title of the notice
    /// </summary>
    public string Title { get; set; }
    
    /// <summary>
    /// Description of the notice
    /// </summary>
    public string Description { get; set; }
    
    /// <summary>
    /// Deadline for receipt of tenders 
    /// </summary>
    public DateTime Deadline { get; set; }
    
    /// <summary>
    /// Date of when the notice has been published
    /// </summary>
    public DateTime PublicationDate { get; set; }
    
    /// <summary>
    /// Official name of the buyer
    /// </summary>
    public string BuyerName { get; set; }
    
    /// <summary>
    /// Country the buyer is located in
    /// </summary>
    public string BuyerCountry { get; set; }
    
    /// <summary>
    /// Main nature of the contract
    /// </summary>
    public string ContractNature { get; set; }
    
    /// <summary>
    /// The country where the notice is taking place
    /// </summary>
    public string Country { get; set; }
    
    /// <summary>
    /// The type of the notice
    /// </summary>
    public string Type { get; set; }
}