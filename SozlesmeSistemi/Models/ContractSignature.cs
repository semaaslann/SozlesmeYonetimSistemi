using SozlesmeSistemi.Models;
using System.ComponentModel.DataAnnotations.Schema;

public class ContractSignature
{
    public int Id { get; set; }
    public int ContractId { get; set; }
    public int UserId { get; set; }
    public string SignatureBase64 { get; set; }
    public DateTime SignedAt { get; set; }

    [ForeignKey("ContractId")]
    public virtual Contract Contract { get; set; }

    [ForeignKey("UserId")]
    public virtual User User { get; set; }
}