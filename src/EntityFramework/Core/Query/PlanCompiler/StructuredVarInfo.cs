// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Query.InternalTrees;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// The StructuredVarInfo class contains information about a structured type Var
    /// and how it can be replaced. This is targeted towards Vars of complex/record/
    /// entity/ref types, and the goal is to replace all such Vars in this module.
    /// </summary>
    internal class StructuredVarInfo : VarInfo
    {
        private Dictionary<EdmProperty, Var> m_propertyToVarMap;
        private readonly List<Var> m_newVars;
        private readonly bool m_newVarsIncludeNullSentinelVar;
        private readonly List<EdmProperty> m_newProperties;
        private readonly RowType m_newType;
        private readonly TypeUsage m_newTypeUsage;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="newType"> new "flat" record type corresponding to the Var's datatype </param>
        /// <param name="newVars"> List of vars to replace current Var </param>
        /// <param name="newTypeProperties"> List of properties in the "flat" record type </param>
        /// <param name="newVarsIncludeNullSentinelVar"> Do the new vars include a var that represents a null sentinel either for this type or for any nested type </param>
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Query.PlanCompiler.PlanCompiler.Assert(System.Boolean,System.String)")]
        internal StructuredVarInfo(
            RowType newType, List<Var> newVars, List<EdmProperty> newTypeProperties, bool newVarsIncludeNullSentinelVar)
        {
            PlanCompiler.Assert(newVars.Count == newTypeProperties.Count, "count mismatch");
            // I see a few places where this is legal
            // PlanCompiler.Assert(newVars.Count > 0, "0 vars?");
            m_newVars = newVars;
            m_newProperties = newTypeProperties;
            m_newType = newType;
            m_newVarsIncludeNullSentinelVar = newVarsIncludeNullSentinelVar;
            m_newTypeUsage = TypeUsage.Create(newType);
        }

        /// <summary>
        /// Gets <see cref="VarInfoKind" /> for this <see cref="VarInfo" />. Always
        /// <see
        ///     cref="VarInfoKind.StructuredTypeVarInfo" />
        /// .
        /// </summary>
        internal override VarInfoKind Kind
        {
            get { return VarInfoKind.StructuredTypeVarInfo; }
        }

        /// <summary>
        /// The NewVars property of the VarInfo is a list of the corresponding
        /// "scalar" Vars that can be used to replace the current Var. This is
        /// mainly intended for use by other RelOps that maintain lists of Vars
        /// - for example, the "Vars" property of ProjectOp and other similar
        /// locations.
        /// </summary>
        internal override List<Var> NewVars
        {
            get { return m_newVars; }
        }

        /// <summary>
        /// The Fields property is matched 1-1 with the NewVars property, and
        /// specifies the properties of the record type corresponding to the
        /// original VarType
        /// </summary>
        internal List<EdmProperty> Fields
        {
            get { return m_newProperties; }
        }

        /// <summary>
        /// Indicates whether any of the vars in NewVars 'derives'
        /// from a null sentinel. For example, for a type that is a Record with two
        /// nested records, if any has a null sentinel, it would be set to true.
        /// It is used when expanding sort keys, to be able to indicate that there is a
        /// sorting operation that includes null sentinels. This indication is later
        /// used by transformation rules.
        /// </summary>
        internal bool NewVarsIncludeNullSentinelVar
        {
            get { return m_newVarsIncludeNullSentinelVar; }
        }

        /// <summary>
        /// Get the Var corresponding to a specific property
        /// </summary>
        /// <param name="p"> the requested property </param>
        /// <param name="v"> the corresponding Var </param>
        /// <returns> true, if the Var was found </returns>
        internal bool TryGetVar(EdmProperty p, out Var v)
        {
            if (m_propertyToVarMap == null)
            {
                InitPropertyToVarMap();
            }
            return m_propertyToVarMap.TryGetValue(p, out v);
        }

        /// <summary>
        /// The NewType property describes the new "flattened" record type
        /// that is a replacement for the original type of the Var
        /// </summary>
        internal RowType NewType
        {
            get { return m_newType; }
        }

        /// <summary>
        /// Returns the NewType wrapped in a TypeUsage
        /// </summary>
        internal TypeUsage NewTypeUsage
        {
            get { return m_newTypeUsage; }
        }

        /// <summary>
        /// Initialize mapping from properties to the corresponding Var
        /// </summary>
        private void InitPropertyToVarMap()
        {
            if (m_propertyToVarMap == null)
            {
                m_propertyToVarMap = new Dictionary<EdmProperty, Var>();
                IEnumerator<Var> newVarEnumerator = m_newVars.GetEnumerator();
                foreach (var prop in m_newProperties)
                {
                    newVarEnumerator.MoveNext();
                    m_propertyToVarMap.Add(prop, newVarEnumerator.Current);
                }
                newVarEnumerator.Dispose();
            }
        }
    }
}
