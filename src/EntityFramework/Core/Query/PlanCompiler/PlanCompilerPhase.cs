// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.PlanCompiler
{
    // <summary>
    // Enum describing which phase of plan compilation we're currently in
    // </summary>
    internal enum PlanCompilerPhase
    {
        // <summary>
        // Just entering the PreProcessor phase
        // </summary>
        PreProcessor = 0,

        // <summary>
        // Entering the AggregatePushdown phase
        // </summary>
        AggregatePushdown = 1,

        // <summary>
        // Entering the Normalization phase
        // </summary>
        Normalization = 2,

        // <summary>
        // Entering the NTE (Nominal Type Eliminator) phase
        // </summary>
        NTE = 3,

        // <summary>
        // Entering the Projection pruning phase
        // </summary>
        ProjectionPruning = 4,

        // <summary>
        // Entering the Nest Pullup phase
        // </summary>
        NestPullup = 5,

        // <summary>
        // Entering the Transformations phase
        // </summary>
        Transformations = 6,

        // <summary>
        // Entering the JoinElimination phase
        // </summary>
        JoinElimination = 7,

        // <summary>
        // Entering the codegen phase
        // </summary>
        CodeGen = 8,

        // <summary>
        // We're almost done
        // </summary>
        PostCodeGen = 9,

        // <summary>
        // Marker
        // </summary>
        MaxMarker = 10
    }
}
