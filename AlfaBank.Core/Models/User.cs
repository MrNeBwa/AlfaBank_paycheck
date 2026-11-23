using AlfaBank.Core.Models.Enums;

namespace AlfaBank.Core.Models;

public class User
{
    public int Id { get; set; }

    public string Login { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public ClientProfile? ClientProfile { get; set; }
}
