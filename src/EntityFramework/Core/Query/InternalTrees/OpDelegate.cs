// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    // <summary>
    // Delegate that describes the processing
    // </summary>
    // <param name="context"> RuleProcessing context </param>
    // <param name="node"> Node to process </param>
    internal delegate void OpDelegate(RuleProcessingContext context, Node node);
}
