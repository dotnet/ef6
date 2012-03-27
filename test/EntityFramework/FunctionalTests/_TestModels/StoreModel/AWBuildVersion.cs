namespace FunctionalTests.Model
{
    using System;

    public class AWBuildVersion
    {
        public virtual byte SystemInformationID { get; set; }

        public virtual string Database_Version { get; set; }

        public virtual DateTime VersionDate { get; set; }

        public virtual DateTime ModifiedDate { get; set; }
    }
}