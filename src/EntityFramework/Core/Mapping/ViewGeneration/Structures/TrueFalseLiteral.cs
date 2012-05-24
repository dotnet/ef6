namespace System.Data.Entity.Core.Mapping.ViewGeneration.Structures
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Common.Utils.Boolean;
    using System.Diagnostics;
    using System.Linq;

    internal abstract class TrueFalseLiteral : BoolLiteral
    {
        internal override BoolExpr<DomainConstraint<BoolLiteral, Constant>> GetDomainBoolExpression(MemberDomainMap domainMap)
        {
            // Essentially say that the variable can take values true or false and here its value is only true
            IEnumerable<Constant> actualValues = new Constant[] { new ScalarConstant(true) };
            IEnumerable<Constant> possibleValues = new Constant[] { new ScalarConstant(true), new ScalarConstant(false) };
            var variableDomain = new Set<Constant>(possibleValues, Constant.EqualityComparer).MakeReadOnly();
            var thisDomain = new Set<Constant>(actualValues, Constant.EqualityComparer).MakeReadOnly();

            var result = MakeTermExpression(this, variableDomain, thisDomain);
            return result;
        }

        internal override BoolExpr<DomainConstraint<BoolLiteral, Constant>> FixRange(Set<Constant> range, MemberDomainMap memberDomainMap)
        {
            Debug.Assert(range.Count == 1, "For BoolLiterals, there should be precisely one value - true or false");
            var scalar = (ScalarConstant)range.First();
            var expr = GetDomainBoolExpression(memberDomainMap);

            if ((bool)scalar.Value == false)
            {
                // The range of the variable was "inverted". Return a NOT of
                // the expression
                expr = new NotExpr<DomainConstraint<BoolLiteral, Constant>>(expr);
            }
            return expr;
        }
    }
}
