namespace AlfaBank.Core.Security;

public interface IPasswordHasher
{
    string Hash(string password);

    string Hash(string password, string salt);

    bool Verify(string password, string passwordHash);
}
