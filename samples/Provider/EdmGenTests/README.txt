This test uses SampleEntityFrameworkProvider to reverse-engineer a NorthwindEF5 database. 
We make a copy of EdmGen.exe to be able to register new provider and run EdmGen.exe /mode:fullgeneration.

NOTES:

1. You must build the provider before running the sample.
2. Note that generated SSDL has a reference to the provider invariant name used as an argument to EdmGen.exe:

<Schema ... Provider="SampleEntityFrameworkProvider" ProviderManifestToken="2005" ...>


