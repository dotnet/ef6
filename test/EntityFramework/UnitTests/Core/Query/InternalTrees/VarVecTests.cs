// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using Moq;
    using Xunit;
    using System.Data.Entity.Utilities;

    public class VarVecTests
    {
        [Fact]
        public void MoveNext_returns_true_for_true_bits_and_false_when_end_is_reached()
        {
            var enumerator = new VarVec.VarVecEnumerator(CreateVarVec(1));
            Assert.True(enumerator.MoveNext());
            Assert.False(enumerator.MoveNext());
        }

        [Fact]
        public void Current_returns_the_var_at_the_current_position_or_null_if_position_is_outside_array()
        {
            var enumerator = new VarVec.VarVecEnumerator(CreateVarVec(1));
            Assert.Null(enumerator.Current);
            enumerator.MoveNext();
            Assert.Equal(1, enumerator.Current.Id);
            enumerator.MoveNext();
            Assert.Null(enumerator.Current);
        }

        [Fact]
        public void Subsumes_returns_true_for_subsumed_vectors_and_false_otherwise()
        {
            Assert.True(CreateVarVec(2, 3).Subsumes(CreateVarVec(2)));
            Assert.False(CreateVarVec(2).Subsumes(CreateVarVec(2, 3)));

            var other = CreateVarVec(2, 3, 6);
            other.Clear(CreateVar(6));
            Assert.True(CreateVarVec(2, 3).Subsumes(other));
        }

        private static VarVec CreateVarVec(params int[] bits)
        {
            var command = new Mock<Command>();
            var vec = new VarVec(command.Object);
            bits.Each(b =>
                          {
                              var v = CreateVar(b);
                              vec.Set(v);
                              command.Setup(m => m.GetVar(b)).Returns(v);
                          });
            return vec;
        }

        private static Var CreateVar(int b)
        {
            return new Mock<Var>(b, VarType.Column, null).Object;
        }
    }
}
