// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Utilities
{
    internal static class BoolExtensions
    {
        internal static bool? Not(this bool? operand)
        {
            // three-valued logic 'not' (T = true, F = false, U = unknown)
            //      !T = F
            //      !F = T
            //      !U = U
            return operand.HasValue ? !operand.Value : (bool?)null;
        }

        internal static bool? And(this bool? left, bool? right)
        {
            // three-valued logic 'and' (T = true, F = false, U = unknown)
            //
            //      T & T = T
            //      T & F = F
            //      F & F = F
            //      F & T = F
            //      F & U = F
            //      U & F = F
            //      T & U = U
            //      U & T = U
            //      U & U = U
            bool? result;
            if (left.HasValue
                && right.HasValue)
            {
                result = left.Value && right.Value;
            }
            else if (!left.HasValue
                     && !right.HasValue)
            {
                result = null; // unknown
            }
            else if (left.HasValue)
            {
                result = left.Value
                             ? (bool?)null
                             : // unknown
                         false;
            }
            else
            {
                result = right.Value
                             ? (bool?)null
                             : false;
            }
            return result;
        }

        internal static bool? Or(this bool? left, bool? right)
        {
            // three-valued logic 'or' (T = true, F = false, U = unknown)
            //
            //      T | T = T
            //      T | F = T
            //      F | F = F
            //      F | T = T
            //      F | U = U
            //      U | F = U
            //      T | U = T
            //      U | T = T
            //      U | U = U
            bool? result;
            if (left.HasValue
                && right.HasValue)
            {
                result = left.Value || right.Value;
            }
            else if (!left.HasValue
                     && !right.HasValue)
            {
                result = null; // unknown
            }
            else if (left.HasValue)
            {
                result = left.Value
                             ? true
                             : (bool?)null; // unknown
            }
            else
            {
                result = right.Value
                             ? true
                             : (bool?)null; // unknown
            }
            return result;
        }
    }
}
