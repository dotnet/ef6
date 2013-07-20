// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Resources;
    using System.Linq;
    using Xunit;

    public class DataModelValidatorTests
    {
        [Fact]
        public void When_store_function_name_has_a_dot()
        {
            var errors = new List<DataModelErrorEventArgs>();
            var validator = new DataModelValidator();

            validator.OnError += (s, a) => errors.Add(a);

            var model = new EdmModel(DataSpace.SSpace);

            model.AddItem(new EdmFunction("Has.Dots", "N", DataSpace.SSpace));

            validator.Validate(model, validateSyntax: true);

            var error = errors.Single();

            Assert.Equal("Name", error.PropertyName);
            Assert.Equal(Strings.EdmModel_Validator_Syntactic_EdmModel_NameIsNotAllowed("Has.Dots"), error.ErrorMessage);
        }
    }
}
