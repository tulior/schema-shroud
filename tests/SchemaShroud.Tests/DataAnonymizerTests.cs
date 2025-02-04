#nullable enable

using SchemaShroud.AnonymizationStrategies;

namespace SchemaShroud.Tests
{
    public class DataAnonymizerTests
    {
        private readonly DataAnonymizer _anonymizer = new();

        [Fact]
        public void Anonymize_HashEmail_ShouldHashEmailProperty()
        {
            var user = new UserDataForTesting
            {
                Email = "test@example.com"
            };
            var anonymizedUser = _anonymizer.Anonymize(user);

            Assert.NotEqual(user.Email, anonymizedUser.Email);
            Assert.Matches("^[a-f0-9]{64}$", anonymizedUser.Email);
        }

        [Fact]
        public void Anonymize_HashEmptyString_ShouldProduceKnownHash()
        {
            var user = new UserDataForTesting
            {
                Email = ""
            };
            var anonymizedUser = _anonymizer.Anonymize(user);

            Assert.Equal("e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
                anonymizedUser.Email);
        }

        [Fact]
        public void Anonymize_RedactName_ShouldRedactToThreeAsterisks()
        {
            var user = new UserDataForTesting
            {
                Name = "John Doe"
            };
            var anonymizedUser = _anonymizer.Anonymize(user);

            Assert.Equal("***", anonymizedUser.Name);
        }

        [Fact]
        public void Anonymize_MaskCreditCard_ShouldMaskAllButLastFour()
        {
            var user = new UserDataForTesting
            {
                CreditCard = "1234567890123456"
            };
            var anonymizedUser = _anonymizer.Anonymize(user);

            Assert.Equal("************3456", anonymizedUser.CreditCard);
        }

        [Fact]
        public void Anonymize_MaskShortCreditCard_ShouldRedact()
        {
            var user = new UserDataForTesting
            {
                CreditCard = "123"
            };
            var anonymizedUser = _anonymizer.Anonymize(user);

            Assert.Equal("***", anonymizedUser.CreditCard);
        }

        [Fact]
        public void Anonymize_MaskFourCharCreditCard_ShouldLeaveAsIs()
        {
            var user = new UserDataForTesting
            {
                CreditCard = "1234"
            };
            var anonymizedUser = _anonymizer.Anonymize(user);

            Assert.Equal("1234", anonymizedUser.CreditCard);
        }


        [Theory]
        [InlineData(25, 25, 25)]
        [InlineData(34, 25, 50)]
        [InlineData(35, 25, 50)]
        public void Anonymize_RangeWithCustomInterval(int input, int expectedFloor, int expectedCeiling)
        {
            var user = new UserDataForTesting {
                BonusFloor = input,
                BonusCeiling = input
            };

            var anonymizedUser = _anonymizer.Anonymize(user);

            Assert.Equal(expectedFloor, anonymizedUser.BonusFloor);
            Assert.Equal(expectedCeiling, anonymizedUser.BonusCeiling);
        }

        [Fact]
        public void Anonymize_NestedObject_ShouldAnonymizeRecursively()
        {
            var user = new UserDataForTesting {
                Profile = new Profile {
                    Address = "123 Main St",
                    Phone = "555-1234"
                }
            };

            var anonymizedUser = _anonymizer.Anonymize(user);

            Assert.Equal("***", anonymizedUser.Profile.Address);
            Assert.Equal("****1234", anonymizedUser.Profile.Phone);
        }

        [Fact]
        public void Anonymize_NullObject_ShouldReturnNull()
        {
            UserDataForTesting? user = null;
            var anonymizedUser = _anonymizer.Anonymize(user);
            Assert.Null(anonymizedUser);
        }

        [Fact]
        public void Anonymize_NullProperties_ShouldHandleGracefully()
        {
            var user = new UserDataForTesting
            {
                Email = null,
                Profile = null
            };

            var anonymizedUser = _anonymizer.Anonymize(user);

            Assert.Null(anonymizedUser.Email);
            Assert.Null(anonymizedUser.Profile);
        }

        [Fact]
        public void Anonymize_CustomStrategy_ShouldOverrideDefault()
        {
            var customStrategies = new Dictionary<AnonymizationMethod, IAnonymizationStrategy>
            {
                [AnonymizationMethod.Redact] = new CustomRedactStrategy("REDACTED")
            };

            var customAnonymizer = new DataAnonymizer(customStrategies);
            var user = new UserDataForTesting { Name = "Test User" };
            var anonymizedUser = customAnonymizer.Anonymize(user);

            Assert.Equal("REDACTED", anonymizedUser.Name);
    
            // Verify other strategies still work
            user.Email = "test@example.com";
            anonymizedUser = customAnonymizer.Anonymize(user);
            Assert.Matches("^[a-f0-9]{64}$", anonymizedUser.Email);
        }

        [Fact]
        public void Anonymize_NonStringProperties_ShouldHandleCorrectly()
        {
            var user = new UserDataForTesting {
                LoginCount = 5,
                Metadata = new Dictionary<string, object> {
                    ["LastLogin"] = "2023-01-01"
                }
            };

            var anonymizedUser = _anonymizer.Anonymize(user);
            
            Assert.Equal(5, anonymizedUser.LoginCount);
            Assert.NotNull(anonymizedUser.Metadata);
            Assert.Equal("2023-01-01", anonymizedUser.Metadata["LastLogin"]);
        }

        public class UserDataForTesting
        {
            [SensitiveData(AnonymizationMethod.Hash)]
            public string? Email { get; set; }

            [SensitiveData(AnonymizationMethod.Redact)]
            public string? Name { get; set; }

            [SensitiveData(AnonymizationMethod.Mask)]
            public string? CreditCard { get; set; }

            public Profile? Profile { get; set; }
            public int LoginCount { get; set; }
            public Dictionary<string, object>? Metadata { get; set; }
            
            [SensitiveData(AnonymizationMethod.Range, Interval = 25)]
            public int BonusFloor { get; set; }

            [SensitiveData(AnonymizationMethod.Range, Interval = 25, RoundingMode = RoundingMode.Ceiling)]
            public int BonusCeiling { get; set; }
            
        }

        public class Profile
        {
            [SensitiveData(AnonymizationMethod.Redact)]
            public string? Address { get; set; }

            [SensitiveData(AnonymizationMethod.Mask)]
            public string? Phone { get; set; }
        }

        public class CustomRedactStrategy : IAnonymizationStrategy
        {
            private readonly string _replacement;

            public CustomRedactStrategy(string replacement)
            {
                _replacement = replacement;
            }

            public object? Anonymize(object? value)
            {
                return _replacement;
            }
        }
    }
}