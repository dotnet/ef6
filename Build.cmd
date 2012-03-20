@ECHO OFF

%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\msbuild EF.msbuild /v:minimal /maxcpucount /nodeReuse:false %*
