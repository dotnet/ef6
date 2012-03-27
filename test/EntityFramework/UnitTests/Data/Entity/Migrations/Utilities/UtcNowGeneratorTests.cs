namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Migrations.Utilities;
    using Xunit;

    public class UtcNowGeneratorTests
    {
        [Fact]
        public void UtcNowGenerator_returns_approximately_UTC_now_time()
        {
            var now = DateTime.UtcNow;
            var again = UtcNowGenerator.UtcNow();

            // Not checking exactness here, just that it isn't local time or something way wrong like that.
            Assert.True((again - now).Duration() <= TimeSpan.FromSeconds(10));
        }

        [Fact]
        public void UtcNowGenerator_does_not_return_the_same_migration_id_timestamp_when_called_in_quick_succession()
        {
            var current = UtcNowGenerator.UtcNow().ToString(UtcNowGenerator.MigrationIdFormat);

            for (var i = 0; i < 4; i++)
            {
                var next = UtcNowGenerator.UtcNow().ToString(UtcNowGenerator.MigrationIdFormat);

                Assert.NotEqual(current, next);

                current = next;
            }
        }
    }
}