 -
# Entity Framework 6 Power Tools Community Edition

This is a fork of the "official" [EF 6 repository](https://github.com/aspnet/entityFramework6/), which hosts the Visual Studio 2015 and 2017 version of EF Power Tools. 

# What are the Power Tools?

Useful design-time utilities for EF 6, accessible through the Visual Studio Solution Explorer context menu. 

When right-clicking on a file containing a derived DbContext class, the following context menu functions are available: 
1. View Entity Data Model (Read-only)
2. View Entity Data Model XML 
3. View Entity Data Model DDL SQL 
4. Generate Views 

When right-clicking on an Entity Data Model .edmx  file, the following context menu function is available: 
1. Generate Views.

If you are looking for Reverse Engineeering tools, I recommend using the [EF Reverse POCO Generator Template](https://marketplace.visualstudio.com/items?itemName=SimonHughes.EntityFrameworkReversePOCOGenerator). You can also use the less advanced ["Code First from Database" feature](http://www.entityframeworktutorial.net/code-first/code-first-from-existing-database.aspx) that is included with the standard Visual Studio tooling for EF 6.

A MSDN article about the tool is [available here](https://msdn.microsoft.com/en-us/data/jj593170

# Downloads/builds

**Release**

The Power Tools will remain in "beta" status.

Download the latest released version from [Visual Studio MarketPlace](https://marketplace.visualstudio.com/items?itemName=ErikEJ.EntityFramework6PowerToolsCommunityEdition)

Or just install from Tools, Extensions and Updates in Visual Studio! ![](https://github.com/ErikEJ/SqlCeToolbox/blob/master/img/ext.png)

**Daily build**

You can download the daily build from [VSIX Gallery](http://vsixgallery.com/extensions/F0A7D01D-4834-44C3-99B2-5907A0701906/extension.vsix). 

Install the [VSIX Gallery Nightly builds extension](https://marketplace.visualstudio.com/items?itemName=MadsKristensen.VSIXGallery-nightlybuilds) to get the latest daily build automatically.

# How do I contribute

There are lots of ways to contribute, including testing out nighty builds, reporting bugs, and contributing code.

If you encounter a bug or have a feature request, please use the [Issue Tracker](https://github.com/ErikEJ/EntityFramework6PowerTools/issues/new).

# Release Notes

## Daily build

**Bug fixes**

## Release 0.9.35 (July 19, 2017)

* Fix error: "Unable to open configSource" with linked config files - thanks to [CZEMacLeod](https://github.com/CZEMacLeod) 
* Clean temp files

## Release 0.9.20 (July 11, 2017)

Initial release based on the current EF codebase
