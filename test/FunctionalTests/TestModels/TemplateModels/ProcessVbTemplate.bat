copy /Y %SDXROOT%\ndp\fx\src\DataEntityDesign\Design\T4Templates\VBDbContext.ContextV4.5.tt %SDXROOT%\qa\Devdiv\dptest\DataEntity\CheckinTests\CodeFirst\ProductivityVBTests\%1\%1.Context.tt
copy /Y %SDXROOT%\ndp\fx\src\DataEntityDesign\Design\T4Templates\VBDbContext.TypesV4.5.tt %SDXROOT%\qa\Devdiv\dptest\DataEntity\CheckinTests\CodeFirst\ProductivityVBTests\%1\%1.tt

TemplateReplace %SDXROOT%\qa\Devdiv\dptest\DataEntity\CheckinTests\CodeFirst\ProductivityVBTests\%1\%1.Context.tt %2
TemplateReplace %SDXROOT%\qa\Devdiv\dptest\DataEntity\CheckinTests\CodeFirst\ProductivityVBTests\%1\%1.tt %2
