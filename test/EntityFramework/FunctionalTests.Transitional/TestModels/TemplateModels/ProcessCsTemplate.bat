copy /Y %SDXROOT%\ndp\fx\src\DataEntityDesign\Design\T4Templates\CSharpDbContext.ContextV4.5.tt %SDXROOT%\qa\Devdiv\dptest\DataEntity\CheckinTests\CodeFirst\ProductivityFunctionalTests\ProductivityApi\TemplateModels\%1\%1.Context.tt
copy /Y %SDXROOT%\ndp\fx\src\DataEntityDesign\Design\T4Templates\CSharpDbContext.TypesV4.5.tt %SDXROOT%\qa\Devdiv\dptest\DataEntity\CheckinTests\CodeFirst\ProductivityFunctionalTests\ProductivityApi\TemplateModels\%1\%1.tt

TemplateReplace %SDXROOT%\qa\Devdiv\dptest\DataEntity\CheckinTests\CodeFirst\ProductivityFunctionalTests\ProductivityApi\TemplateModels\%1\%1.Context.tt %2
TemplateReplace %SDXROOT%\qa\Devdiv\dptest\DataEntity\CheckinTests\CodeFirst\ProductivityFunctionalTests\ProductivityApi\TemplateModels\%1\%1.tt %2
