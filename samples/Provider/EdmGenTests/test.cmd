copy /b %WINDIR%\Microsoft.NET\Framework\v4.0.30319\EdmGen.exe .
copy /b ..\SampleEntityFrameworkProvider\bin\Debug\SampleEntityFrameworkProvider.dll .
.\edmgen.exe /provider:SampleEntityFrameworkProvider /mode:fullgeneration /connectionstring:"server=.\sqlexpress;database=NorthwindEF5;integrated security=sspi" /project:NorthwindEF /targetVersion:4.5
