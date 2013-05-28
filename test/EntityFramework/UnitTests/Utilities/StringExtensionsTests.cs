// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Utilities
{
    using System.Data.Entity.Migrations;
    using Xunit;

    public class StringExtensionsTests
    {
        public class EqualsIgnoreCase
        {
            [Fact]
            public void EqualsIgnoreCase_should_ignore_case()
            {
                Assert.True("foo".EqualsIgnoreCase("Foo"));
                Assert.False("Bar".EqualsIgnoreCase("Foo"));
            }

        }

        public class EachLine
        {
            [Fact]
            public void EachLine_can_slit_lines()
            {
                var lines = 0;

                "Foo\nBar\r\nBaz\r\n".EachLine(_ => lines++);

                Assert.Equal(4, lines);
            }

        }

        public class IsValidMigrationId
        {
            [Fact]
            public void IsValid_should_correctly_validate_ids()
            {
                Assert.False("Foo".IsValidMigrationId());
                Assert.False("11111111111111_Foo".IsValidMigrationId());
                Assert.False("11111111111111Foo".IsValidMigrationId());
                Assert.True("111111111111111_Foo".IsValidMigrationId());
                Assert.True(DbMigrator.InitialDatabase.IsValidMigrationId());
            }
        }

        public class IsAutomaticMigration
        {
            [Fact]
            public void IsAutomatic_detects_automatic_migration_names()
            {
                Assert.True("111111111111111_AutomaticMigration".IsAutomaticMigration());
                Assert.True("111111111111111_Foo_AutomaticMigration".IsAutomaticMigration());
                Assert.False("111111111111111_Foo".IsAutomaticMigration());
            }
        }

        public class RestrictTo
        {
            [Fact]
            public void RestrictTo_should_limit_string_length()
            {
                Assert.Equal("123", "123".RestrictTo(5));
                Assert.Equal("123", "123".RestrictTo(3));
                Assert.Equal("123", "12345".RestrictTo(3));
                Assert.Equal("", "12345".RestrictTo(0));
                Assert.Equal("", "".RestrictTo(0));
                Assert.Equal(null, StringExtensions.RestrictTo(null, 0));
            }
        }

        public class ToAutomaticMigrationId
        {
            [Fact]
            public void ToAutomaticMigrationId_should_rewind_timestamp_and_append_auto_string()
            {
                Assert.Equal("201205054534555_Foo_AutomaticMigration", "201205054534556_Foo".ToAutomaticMigrationId());
                Assert.Equal("111111111111109_Foo_AutomaticMigration", "111111111111110_Foo".ToAutomaticMigrationId());
            }
        }

        public class IsValidUndottedName
        {
            [Fact]
            public void Null_or_empty_strings_are_not_valid()
            {
                Assert.False(((string)null).IsValidUndottedName());
                Assert.False("".IsValidUndottedName());
                Assert.False(" ".IsValidUndottedName());
            }

            [Fact]
            public void Strings_that_dont_start_with_a_valid_characeter_are_not_valid()
            {
                Assert.False("_PinkyPie".IsValidUndottedName());
                Assert.False(" RainbowDash".IsValidUndottedName());
                Assert.False("-AppleJack".IsValidUndottedName());
                Assert.False("\u033CFlutterShy".IsValidUndottedName()); // Non-spacing mark (Combining seagull below)
                Assert.False("\u0940TwighlightSparkle".IsValidUndottedName()); // Spacing-combining mark (Devanagari vowel sign II)
                Assert.False("\uFE4FRarity".IsValidUndottedName()); // Connector punctuation (Wavy low line)
                Assert.False("6PrincessCelestia".IsValidUndottedName()); // Decimal digit number
                Assert.False("\U0001F40ESpike".IsValidUndottedName()); // Other (Horse)
                Assert.False("\u2061NightmareMoon".IsValidUndottedName()); // Other, format (Function application)
            }

            [Fact]
            public void Strings_that_contain_internal_invalid_characeters_are_not_valid()
            {
                Assert.False("Rainbow Dash".IsValidUndottedName());
                Assert.False("Apple-Jack".IsValidUndottedName());
                Assert.False("Shining\U0001F40EArmor".IsValidUndottedName()); // Other (Horse)
            }

            [Fact]
            public void Strings_that_have_only_valid_characters_are_valid()
            {
                Assert.True("Pinky_Pie".IsValidUndottedName());
                Assert.True("Flutter\u033CShy".IsValidUndottedName()); // Non-spacing mark (Combining seagull below)
                Assert.True("Twighlight\u0940Sparkle".IsValidUndottedName()); // Spacing-combining mark (Devanagari vowel sign II)
                Assert.True("Rarity\uFE4F".IsValidUndottedName()); // Connector punctuation (Wavy low line)
                Assert.True("Princess6Celestia".IsValidUndottedName()); // Decimal digit number
                Assert.True("\u2165Zecora".IsValidUndottedName()); // Number, letter (Roman numeral 6)
                Assert.True("Princess\u2165Celestia".IsValidUndottedName()); // Number, letter (Roman numeral 6)
            }
        }
    }
}
