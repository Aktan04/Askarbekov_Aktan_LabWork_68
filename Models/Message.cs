using System.ComponentModel.DataAnnotations;

namespace Hh.Models;

public class Message
{
    public int Id { get; set; }
    [Required]
    public string Text { get; set; }
    public int ChatId { get; set; }
    public Chat? Chat { get; set; }
    public DateTime DateOfSend { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
}