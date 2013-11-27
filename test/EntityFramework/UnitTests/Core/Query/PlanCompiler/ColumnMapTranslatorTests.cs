// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Query.InternalTrees;
    using Xunit;

    public class ColumnMapTranslatorTests
    {
        [Fact]
        public void Translate_preserves_column_type()
        {
            var intTypeUsage = 
                TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32));
            var enumTypeUsage = 
                TypeUsage.CreateDefaultTypeUsage(
                    new EnumType("ns", "DayOfWeek", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32), false, DataSpace.CSpace));

            var originalVar = new ComputedVar(42, intTypeUsage);
            var originalColumnMap = new VarRefColumnMap(enumTypeUsage, "dayOfWeek", originalVar);

            var replacementVar = new ComputedVar(911, intTypeUsage);
            var replacementColumnMap = new VarRefColumnMap(intTypeUsage, null, replacementVar);

            var varToColumnMap = new Dictionary<Var, ColumnMap> { { originalVar, replacementColumnMap } };

            var resultColumnMap = ColumnMapTranslator.Translate(originalColumnMap, varToColumnMap);

            Assert.Same(replacementColumnMap, resultColumnMap);
            Assert.Equal(originalColumnMap.Name, resultColumnMap.Name);
            Assert.Equal(originalColumnMap.Type.EdmType, resultColumnMap.Type.EdmType);
        }
    }
}
