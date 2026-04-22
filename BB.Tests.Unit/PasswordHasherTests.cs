using BB.Infrastructure.Security;

namespace BB.Tests.Unit
{
    public class PasswordHasherTests
    {
        private readonly PasswordHasher _passwordHasher = new();

        [Fact]
        public void Hash_ReturnsValidBCryptString() { 
            // Arrange
            var password = "TestPassword123!";
            
            // Act
            var hash = _passwordHasher.Hash(password);
            
            // Assert
            Assert.NotNull(hash);
            Assert.StartsWith("$2", hash); // BCrypt hashes start with $2a$, $2b$, or $2y$
            Assert.NotEqual("MyPassword123", hash);
        }

        [Fact]
        public void Hash_SamePassword_ProducesDifferentHashes()
        {
            var hash1 = _passwordHasher.Hash("SamePassword123");
            var hash2 = _passwordHasher.Hash("SamePassword123");
            Assert.NotEqual(hash1, hash2);
        }
        [Fact]
        public void Verify_CorrectPassword_ReturnsTrue()
        {
            var password = "MyPassword123";
            var hash = _passwordHasher.Hash(password);
            Assert.True(_passwordHasher.Verify(password, hash));
        }

        [Fact]
        public void Verify_WrongPassword_ReturnsFalse()
        {
            var hash = _passwordHasher.Hash("CorrectPassword");
            Assert.False(_passwordHasher.Verify("WrongPassword", hash));
        }
    }
}
