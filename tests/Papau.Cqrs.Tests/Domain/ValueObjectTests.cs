using FluentAssertions;
using Xunit;
using Papau.Cqrs.Domain;

namespace Papau.Cqrs.Tests.Unit.Domain
{
    public class ValueObjectTests
    {
        public class SomeValue : ValueObject<SomeValue>
        {
            private int _internalValue = 0;
            protected override int GetHashCodeMandatory()
            {
                return _internalValue.GetHashCode();
            }

            protected override bool IsEqual(SomeValue other)
            {
                return this._internalValue == other._internalValue;
            }
        }

        public class SomeOtherValue : SomeValue {}

        public class EqualsMethod : ValueObjectTests
        {
            [Fact]
            public void Returns_False_When_Compared_To_Null()
            {
                var valueObject = new SomeValue();
                
                var result = valueObject.Equals(null);

                result.Should().BeFalse();
            }

            [Fact]
            public void Returns_False_For_Derived_Types()
            {
                var valueObject = new SomeValue();
            }

            [Fact]
            public void Returns_False_For_Other_Types()
            {
                var valueObject = new SomeValue();

                var result = valueObject.Equals(new SomeOtherValue());

                result.Should().BeFalse();
            }
        }

        public class EqualsOperator
        {
            [Fact]
            public void Returns_True_When_Both_Sides_Are_Null()
            {
                SomeValue x1 = null;
                SomeValue x2 = null;

                (x1 == x2).Should().BeTrue();
            }
        }

        public class NotEqualsOperator
        {
            [Fact]
            public void Inverts_The_Equal_Operator()
            {
                var x1 = new SomeValue();
                var x2 = new SomeValue();
                
                (x1 != x2).Should().BeFalse();
            }
        }
    }
}