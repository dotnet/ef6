// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.DatabaseGeneration.OutputGenerators
{
    using System.Activities;
    using System.Collections.Generic;

    /// <summary>
    ///     Interface that is used by OutputGeneratorActivities which performs the bulk of the transformation in-code
    /// </summary>
    public interface IGenerateActivityOutput
    {
        /// <summary>
        ///     Generates output for input that is in the specified OutputGeneratorActivity.
        /// </summary>
        /// <typeparam name="T">The type of the activity output.</typeparam>
        /// <param name="owningActivity">The activity that is calling this method.</param>
        /// <param name="context">The activity context that contains the state of the workflow.</param>
        /// <param name="inputs">Input for the activity as key-value pairs.</param>
        /// <returns>Output of type T for input that is in the specified OutputGeneratorActivity.</returns>
        T GenerateActivityOutput<T>(
            OutputGeneratorActivity owningActivity, NativeActivityContext context, IDictionary<string, object> inputs) where T : class;
    }
}
