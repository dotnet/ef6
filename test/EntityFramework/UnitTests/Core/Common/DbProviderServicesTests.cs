namespace System.Data.Entity.Core.Common
{
    using System.Data.Entity.Resources;
    using Xunit;

    public class DbProviderServicesTests
    {
        public class ExpandDataDirectory
        {
            [Fact]
            public void ExpandDataDirectory_returns_the_given_string_if_it_does_not_start_with_DataDirectory_ident()
            {
                Assert.Null(DbProviderServices.ExpandDataDirectory(null));
                Assert.Equal("", DbProviderServices.ExpandDataDirectory(""));
                Assert.Equal("It's Late", DbProviderServices.ExpandDataDirectory("It's Late"));
            }

            [Fact]
            public void ExpandDataDirectory_throws_if_expansion_is_needed_but_DataDirectory_is_not_a_string()
            {
                TestWithDataDirectory(
                    new Random(),
                    () => Assert.Equal(
                        Strings.ADP_InvalidDataDirectory,
                        Assert.Throws<InvalidOperationException>(() => DbProviderServices.ExpandDataDirectory("|DataDirectory|")).Message));
            }

            [Fact]
            public void ExpandDataDirectory_uses_app_domain_base_directory_if_data_directory_is_set_but_empty()
            {
                TestWithDataDirectory(
                    "",
                    () => Assert.Equal(
                        AppDomain.CurrentDomain.BaseDirectory + @"\",
                        DbProviderServices.ExpandDataDirectory("|DataDirectory|")));
            }

            [Fact]
            public void ExpandDataDirectory_uses_empty_string_if_DataDirectory_is_not_set()
            {
                TestWithDataDirectory(
                    null,
                    () => Assert.Equal(@"\", DbProviderServices.ExpandDataDirectory("|DataDirectory|")));
            }

            [Fact]
            public void ExpandDataDirectory_correctly_concatenates_paths_with_correct_number_of_slashes()
            {
                TestWithDataDirectory(
                    @"C:\MelancholyBlues",
                    () => Assert.Equal(@"C:\MelancholyBlues\", DbProviderServices.ExpandDataDirectory(@"|DataDirectory|")));

                TestWithDataDirectory(
                    @"C:\MelancholyBlues\",
                    () => Assert.Equal(@"C:\MelancholyBlues\", DbProviderServices.ExpandDataDirectory(@"|DataDirectory|")));

                TestWithDataDirectory(
                    null,
                    () => Assert.Equal(@"\SheerHeartAttack", DbProviderServices.ExpandDataDirectory(@"|DataDirectory|SheerHeartAttack")));

                TestWithDataDirectory(
                    null,
                    () => Assert.Equal(@"\SheerHeartAttack", DbProviderServices.ExpandDataDirectory(@"|DataDirectory|\SheerHeartAttack")));

                TestWithDataDirectory(
                    @"C:\MelancholyBlues",
                    () => Assert.Equal(@"C:\MelancholyBlues\SheerHeartAttack", DbProviderServices.ExpandDataDirectory(@"|DataDirectory|SheerHeartAttack")));

                TestWithDataDirectory(
                    @"C:\MelancholyBlues",
                    () => Assert.Equal(@"C:\MelancholyBlues\SheerHeartAttack", DbProviderServices.ExpandDataDirectory(@"|DataDirectory|\SheerHeartAttack")));

                TestWithDataDirectory(
                    @"C:\MelancholyBlues\",
                    () => Assert.Equal(@"C:\MelancholyBlues\SheerHeartAttack", DbProviderServices.ExpandDataDirectory(@"|DataDirectory|SheerHeartAttack")));

                TestWithDataDirectory(
                    @"C:\MelancholyBlues\",
                    () => Assert.Equal(@"C:\MelancholyBlues\SheerHeartAttack", DbProviderServices.ExpandDataDirectory(@"|DataDirectory|\SheerHeartAttack")));
            }

            [Fact]
            public void ExpandDataDirectory_throws_if_the_result_is_not_a_fully_expanded_path()
            {
                TestWithDataDirectory(
                    @"C:\MelancholyBlues\..\",
                    () => Assert.Equal(
                        Strings.ExpandingDataDirectoryFailed,
                        Assert.Throws<ArgumentException>(() => DbProviderServices.ExpandDataDirectory("|DataDirectory|")).Message));
            }

            private static void TestWithDataDirectory(object dataDirectory, Action test)
            {
                var previousDataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory");
                try
                {
                    AppDomain.CurrentDomain.SetData("DataDirectory", dataDirectory);
                    test();
                }
                finally
                {
                    AppDomain.CurrentDomain.SetData("DataDirectory", previousDataDirectory);
                }
            }
        }
    }
}
