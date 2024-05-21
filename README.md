# Entity Framework 6

Entity Framework 6 (EF6) is a tried and tested object-relational mapper for .NET with many years of feature development and stabilization. It eliminates the need for most of the data-access code that developers usually need to write.

## Status and Support

The latest version of EF6 is still supported by Microsoft--see [Entity Framework Support Policies](https://docs.microsoft.com/ef/efcore-and-ef6/support) for details. However, EF6 is no longer being actively developed. This means that:

- Security issues _will_ be fixed, as for any supported product.
- High-impact bugs, typically those impacting a very large number of users, _may_ be fixed.
- Other bugs will _not_ be fixed.
- New features will _not_ be implemented

This plan focuses on stability of the codebase and compatibility of new versions above all else, excepting security. In general, the EF6 codebase is reliable and has been stable for several years with few significant bugs. However, due to the complexity and number of usage scenarios, and also the sheer number of applications that use EF6, any change has the potential to regress existing behaviors. This is why we will be making only security fixes. Also, we will not be accepting pull requests made by the community, again to ensure stability of the codebase.

## Entity Framework Core

Entity Framework Core (EF Core) is a lightweight and extensible version of Entity Framework and continues to be actively developed on [the EFCore GitHub repo](https://github.com/dotnet/efcore). EF Core is developed exclusively for modern .NET and does not run on .NET Framework. EF Core includes [many improvements and new features over EF6](https://docs.microsoft.com/ef/efcore-and-ef6/). EF Core has a different architecture to EF6 and takes a very different approach to its internals--for example, EF Core does not support a visual designer or EDMX files. However, most EF6 projects can be ported to EF Core with some amount of work--see [Port from EF6 to EF Core](https://docs.microsoft.com/ef/efcore-and-ef6/porting/) for a guide.

## Getting help

See [the EF6 docs](https://docs.microsoft.com/ef/ef6/) for installation, documentation, tutorials, samples, etc. This documentation is no longer being updated, but still contains useful and usable content.

The EF team is focusing efforts on EF Core, and hence team members are unlikely to respond to issues filed on this repo. We recommend asking questions on [Stack Overflow](https://stackoverflow.com/questions/tagged/entity-framework*).

You may instead choose to [contact a Microsoft Support professional](http://support.microsoft.com/supportforbusiness/productselection?sapId=bec2bc54-b200-6962-301f-f098532f27b2) for support. Please note that this may incur a fee.

## EF6 Tools for Visual Studio

The code for the EF6 Tools for VS (including the visual designer) can be found in the [EF6Tools](https://github.com/dotnet/ef6tools) repo.

## EF6 PowerTools

The EF6 PowerTools is a community-driven project with its own [GitHub repo](https://github.com/ErikEJ/EntityFramework6PowerTools).
