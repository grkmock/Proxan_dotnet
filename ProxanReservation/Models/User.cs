using System.ComponentModel.DataAnnotations.Schema;

namespace ProxanReservation.Models;

public class User {
    public int Id { get; set; } // [Column] yoksa varsayılan Id'dir
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
}