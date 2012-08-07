// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.ViewGeneration.QueryRewriting
{
    using System.Collections.Generic;
    using System.Diagnostics;

    internal class RewritingProcessor<T_Tile> : TileProcessor<T_Tile>
        where T_Tile : class
    {
        public const double PermuteFraction = 0.0;
        public const int MinPermutations = 0;
        public const int MaxPermutations = 0;

        private int m_numSATChecks;
        private int m_numIntersection;
        private int m_numDifference;
        private int m_numUnion;

        private int m_numErrors;

        private readonly TileProcessor<T_Tile> m_tileProcessor;

        public RewritingProcessor(TileProcessor<T_Tile> tileProcessor)
        {
            m_tileProcessor = tileProcessor;
        }

        internal TileProcessor<T_Tile> TileProcessor
        {
            get { return m_tileProcessor; }
        }

        public void GetStatistics(out int numSATChecks, out int numIntersection, out int numUnion, out int numDifference, out int numErrors)
        {
            numSATChecks = m_numSATChecks;
            numIntersection = m_numIntersection;
            numUnion = m_numUnion;
            numDifference = m_numDifference;
            numErrors = m_numErrors;
        }

        internal override T_Tile GetArg1(T_Tile tile)
        {
            return m_tileProcessor.GetArg1(tile);
        }

        internal override T_Tile GetArg2(T_Tile tile)
        {
            return m_tileProcessor.GetArg2(tile);
        }

        internal override TileOpKind GetOpKind(T_Tile tile)
        {
            return m_tileProcessor.GetOpKind(tile);
        }

        internal override bool IsEmpty(T_Tile a)
        {
            m_numSATChecks++;
            return m_tileProcessor.IsEmpty(a);
        }

        public bool IsDisjointFrom(T_Tile a, T_Tile b)
        {
            return m_tileProcessor.IsEmpty(Join(a, b));
        }

        internal bool IsContainedIn(T_Tile a, T_Tile b)
        {
            var difference = AntiSemiJoin(a, b);
            return IsEmpty(difference);
        }

        internal bool IsEquivalentTo(T_Tile a, T_Tile b)
        {
            var aInB = IsContainedIn(a, b);
            var bInA = IsContainedIn(b, a);
            return aInB && bInA;
        }

        internal override T_Tile Union(T_Tile a, T_Tile b)
        {
            m_numUnion++;
            return m_tileProcessor.Union(a, b);
        }

        internal override T_Tile Join(T_Tile a, T_Tile b)
        {
            if (a == null)
            {
                return b;
            }
            m_numIntersection++;
            return m_tileProcessor.Join(a, b);
        }

        internal override T_Tile AntiSemiJoin(T_Tile a, T_Tile b)
        {
            m_numDifference++;
            return m_tileProcessor.AntiSemiJoin(a, b);
        }

        public void AddError()
        {
            m_numErrors++;
        }

        public int CountOperators(T_Tile query)
        {
            var count = 0;
            if (query != null)
            {
                if (GetOpKind(query)
                    != TileOpKind.Named)
                {
                    count++;
                    count += CountOperators(GetArg1(query));
                    count += CountOperators(GetArg2(query));
                }
            }
            return count;
        }

        public int CountViews(T_Tile query)
        {
            var views = new HashSet<T_Tile>();
            GatherViews(query, views);
            return views.Count;
        }

        public void GatherViews(T_Tile rewriting, HashSet<T_Tile> views)
        {
            if (rewriting != null)
            {
                if (GetOpKind(rewriting)
                    == TileOpKind.Named)
                {
                    views.Add(rewriting);
                }
                else
                {
                    GatherViews(GetArg1(rewriting), views);
                    GatherViews(GetArg2(rewriting), views);
                }
            }
        }

        public static IEnumerable<T> AllButOne<T>(IEnumerable<T> list, int toSkipPosition)
        {
            var valuePosition = 0;
            foreach (var value in list)
            {
                if (valuePosition++ != toSkipPosition)
                {
                    yield return value;
                }
            }
        }

        public static IEnumerable<T> Concat<T>(T value, IEnumerable<T> rest)
        {
            yield return value;
            foreach (var restValue in rest)
            {
                yield return restValue;
            }
        }

        public static IEnumerable<IEnumerable<T>> Permute<T>(IEnumerable<T> list)
        {
            IEnumerable<T> rest = null;
            var valuePosition = 0;
            foreach (var value in list)
            {
                rest = AllButOne(list, valuePosition++);
                foreach (var restPermutation in Permute(rest))
                {
                    yield return Concat(value, restPermutation);
                }
            }
            if (rest == null)
            {
                yield return list; // list is empty enumeration
            }
        }

        private static Random rnd = new Random(1507);

        public static List<T> RandomPermutation<T>(IEnumerable<T> input)
        {
            var output = new List<T>(input);
            for (var i = 0; i < output.Count; i++)
            {
                var j = rnd.Next(output.Count);
                var tmp = output[i];
                output[i] = output[j];
                output[j] = tmp;
            }
            return output;
        }

        public static IEnumerable<T> Reverse<T>(IEnumerable<T> input, HashSet<T> filter)
        {
            var output = new List<T>(input);
            output.Reverse();
            foreach (var t in output)
            {
                if (filter.Contains(t))
                {
                    yield return t;
                }
            }
        }

        public bool RewriteQuery(T_Tile toFill, T_Tile toAvoid, IEnumerable<T_Tile> views, out T_Tile rewriting)
        {
            if (RewriteQueryOnce(toFill, toAvoid, views, out rewriting))
            {
                var usedViews = new HashSet<T_Tile>();
                GatherViews(rewriting, usedViews);
                var opCount = CountOperators(rewriting);

                // try several permutations of views, pick one with fewer operators
                T_Tile newRewriting;
                var permuteTries = 0;
                var numPermutations = Math.Min(MaxPermutations, Math.Max(MinPermutations, (int)(usedViews.Count * PermuteFraction)));
                while (permuteTries++ < numPermutations)
                {
                    IEnumerable<T_Tile> permutedViews;
                    if (permuteTries == 1)
                    {
                        permutedViews = Reverse(views, usedViews);
                    }
                    else
                    {
                        permutedViews = RandomPermutation(usedViews); // Tradeoff: views vs. usedViews!
                    }
                    var succeeded = RewriteQueryOnce(toFill, toAvoid, permutedViews, out newRewriting);
                    Debug.Assert(succeeded);
                    var newOpCount = CountOperators(newRewriting);
                    if (newOpCount < opCount)
                    {
                        opCount = newOpCount;
                        rewriting = newRewriting;
                    }
                    var newUsedViews = new HashSet<T_Tile>();
                    GatherViews(newRewriting, newUsedViews);
                    usedViews = newUsedViews; // can only be fewer!
                }
                return true;
            }
            return false;
        }

        public bool RewriteQueryOnce(T_Tile toFill, T_Tile toAvoid, IEnumerable<T_Tile> views, out T_Tile rewriting)
        {
            var viewList = new List<T_Tile>(views);
            return RewritingPass<T_Tile>.RewriteQuery(toFill, toAvoid, out rewriting, viewList, this);
        }
    }
}
