pushd EdmGenTests
del NorthwindEF.*
del SampleEntityFrameworkProvider.dll
del EdmGen.exe
popd
pushd SampleEntityFrameworkProvider
rd /q /s bin
rd /q /s obj
popd
pushd ConsoleTests
rd /q /s bin
rd /q /s obj
popd
pushd DdexProvider
rd /q /s bin
rd /q /s obj
popd
pushd NorthwindEFModel
rd /q /s bin
rd /q /s obj
popd
pushd FunctionStubGenerator
rd /q /s bin
rd /q /s obj
popd
del /A:H SampleEntityFrameworkProvider.suo
attrib /S -R *.*
pushd ..
del EFSampleProvider.zip
zip -9 -X -r EFSampleProvider.zip EFSampleProvider
popd
