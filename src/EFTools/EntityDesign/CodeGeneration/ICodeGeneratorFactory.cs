
namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using Microsoft.Data.Entity.Design.Common;

    internal interface ICodeGeneratorFactory
    {
        IContextGenerator GetContextGenerator(LangEnum language, bool isEmptyModel);
        IEntityTypeGenerator GetEntityTypeGenerator(LangEnum language);
    }
}
