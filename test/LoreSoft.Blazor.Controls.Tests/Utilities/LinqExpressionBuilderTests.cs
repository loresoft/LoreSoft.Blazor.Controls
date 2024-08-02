using System.Linq.Dynamic.Core;

using LoreSoft.Blazor.Controls.Tests.Data;
using LoreSoft.Blazor.Controls.Utilities;

namespace LoreSoft.Blazor.Controls.Tests.Utilities;

public class LinqExpressionBuilderTests
{
    [Fact]
    public void FilterNormal()
    {
        var queryFilter = new QueryFilter { Field = "Rank", Value = 7 };

        var builder = new LinqExpressionBuilder();
        builder.Build(queryFilter);

        builder.Expression.Should().NotBeEmpty();
        builder.Expression.Should().Be("Rank == @0");

        builder.Parameters.Count.Should().Be(1);
        builder.Parameters[0].Should().Be(7);

        var predicate = builder.Expression;
        var parameters = builder.Parameters.ToArray();

        var query = Fruit.Data().AsQueryable();

        var results = query.Where(predicate, parameters).ToList();
        results.Should().NotBeEmpty();
        results.Count.Should().Be(3);
    }

    [Fact]
    public void FilterGuid()
    {
        var pearId = new Guid("3a1ec4ee-239c-41e5-b934-fbe4ce8113df");
        var queryFilter = new QueryFilter { Field = "Id", Value = pearId };

        var builder = new LinqExpressionBuilder();
        builder.Build(queryFilter);

        builder.Expression.Should().NotBeEmpty();
        builder.Expression.Should().Be("Id == @0");

        builder.Parameters.Count.Should().Be(1);
        builder.Parameters[0].Should().Be(pearId);

        var predicate = builder.Expression;
        var parameters = builder.Parameters.ToArray();

        var query = Fruit.Data().AsQueryable();

        var results = query.Where(predicate, parameters).ToList();
        results.Should().NotBeEmpty();
        results.Count.Should().Be(1);
    }

    [Fact]
    public void FilterLogicalOr()
    {
        var queryFilter = new QueryGroup
        {
            Logic = QueryLogic.Or,
            Filters =
            [
                new QueryFilter{ Field = "Rank", Value = 7 },
                new QueryFilter{ Field = "Name", Value = "Apple" }
            ]
        };

        var builder = new LinqExpressionBuilder();
        builder.Build(queryFilter);

        builder.Expression.Should().NotBeEmpty();
        builder.Expression.Should().Be("(Rank == @0 or Name == @1)");

        builder.Parameters.Count.Should().Be(2);
        builder.Parameters[0].Should().Be(7);
        builder.Parameters[1].Should().Be("Apple");

        var predicate = builder.Expression;
        var parameters = builder.Parameters.ToArray();

        var query = Fruit.Data().AsQueryable();

        var results = query.Where(predicate, parameters).ToList();
        results.Should().NotBeEmpty();
        results.Count.Should().Be(4);
    }

    [Fact]
    public void FilterLogicalAnd()
    {
        var queryFilter = new QueryGroup
        {
            Filters =
            [
                new QueryFilter{ Field = "Rank", Value = 7 },
                new QueryFilter{ Field = "Name", Value = "Blueberry" }
            ]

        };

        var builder = new LinqExpressionBuilder();
        builder.Build(queryFilter);

        builder.Expression.Should().NotBeEmpty();
        builder.Expression.Should().Be("(Rank == @0 and Name == @1)");

        builder.Parameters.Count.Should().Be(2);
        builder.Parameters[0].Should().Be(7);
        builder.Parameters[1].Should().Be("Blueberry");

        var predicate = builder.Expression;
        var parameters = builder.Parameters.ToArray();

        var query = Fruit.Data().AsQueryable();

        var results = query.Where(predicate, parameters).ToList();
        results.Should().NotBeEmpty();
        results.Count.Should().Be(1);
    }

    [Fact]
    public void FilterComplex()
    {
        var queryFilter = new QueryGroup
        {
            Filters =
            [
                new QueryFilter
                {
                    Field = "Rank",
                    Operator = QueryOperators.GreaterThan,
                    Value = 5
                },
                new QueryGroup
                {
                    Logic = QueryLogic.Or,
                    Filters =
                    [
                        new QueryFilter { Field = "Name", Value = "Strawberry" },
                        new QueryFilter { Field = "Name", Value = "Blueberry" }
                    ]
                }
            ]
        };

        var builder = new LinqExpressionBuilder();
        builder.Build(queryFilter);

        builder.Expression.Should().NotBeEmpty();
        builder.Expression.Should().Be("(Rank > @0 and (Name == @1 or Name == @2))");

        builder.Parameters.Count.Should().Be(3);
        builder.Parameters[0].Should().Be(5);
        builder.Parameters[1].Should().Be("Strawberry");
        builder.Parameters[2].Should().Be("Blueberry");


        var predicate = builder.Expression;
        var parameters = builder.Parameters.ToArray();

        var query = Fruit.Data().AsQueryable();

        var results = query.Where(predicate, parameters).ToList();
        results.Should().NotBeEmpty();
        results.Count.Should().Be(2);

    }

    [Fact]
    public void FilterContains()
    {
        var queryFilter = new QueryFilter
        {
            Field = "Name",
            Operator = QueryOperators.Contains,
            Value = "berry"
        };
        var builder = new LinqExpressionBuilder();
        builder.Build(queryFilter);

        builder.Expression.Should().NotBeEmpty();
        builder.Expression.Should().Be("Name.Contains(@0, StringComparison.OrdinalIgnoreCase)");

        builder.Parameters.Count.Should().Be(1);
        builder.Parameters[0].Should().Be("berry");

        var predicate = builder.Expression;
        var parameters = builder.Parameters.ToArray();

        var query = Fruit.Data().AsQueryable();

        var results = query.Where(predicate, parameters).ToList();
        results.Should().NotBeEmpty();
        results.Count.Should().Be(3);

    }

    [Fact]
    public void FilterIsNull()
    {
        var queryFilter = new QueryFilter
        {
            Field = "Description",
            Operator = QueryOperators.IsNull
        };
        var builder = new LinqExpressionBuilder();
        builder.Build(queryFilter);

        builder.Expression.Should().NotBeEmpty();
        builder.Expression.Should().Be("Description == NULL");

        builder.Parameters.Count.Should().Be(0);

        var predicate = builder.Expression;
        var parameters = builder.Parameters.ToArray();

        var query = Fruit.Data().AsQueryable();

        var results = query.Where(predicate, parameters).ToList();
        results.Should().NotBeEmpty();
        results.Count.Should().Be(10);

    }

    [Fact]
    public void FilterIsNotNull()
    {
        var queryFilter = new QueryFilter
        {
            Field = "Name",
            Operator = QueryOperators.IsNotNull
        };
        var builder = new LinqExpressionBuilder();
        builder.Build(queryFilter);

        builder.Expression.Should().NotBeEmpty();
        builder.Expression.Should().Be("Name != NULL");

        builder.Parameters.Count.Should().Be(0);

        var predicate = builder.Expression;
        var parameters = builder.Parameters.ToArray();

        var query = Fruit.Data().AsQueryable();

        var results = query.Where(predicate, parameters).ToList();
        results.Should().NotBeEmpty();
        results.Count.Should().Be(10);

    }

    [Fact]
    public void FilterNotContains()
    {
        var queryFilter = new QueryFilter
        {
            Field = "Name",
            Operator = QueryOperators.NotContains,
            Value = "berry"
        };
        var builder = new LinqExpressionBuilder();
        builder.Build(queryFilter);

        builder.Expression.Should().NotBeEmpty();
        builder.Expression.Should().Be("!Name.Contains(@0, StringComparison.OrdinalIgnoreCase)");

        builder.Parameters.Count.Should().Be(1);
        builder.Parameters[0].Should().Be("berry");

        var predicate = builder.Expression;
        var parameters = builder.Parameters.ToArray();

        var query = Fruit.Data().AsQueryable();

        var results = query.Where(predicate, parameters).ToList();
        results.Should().NotBeEmpty();
        results.Count.Should().Be(7);

    }

    [Fact]
    public void FilterStartsWith()
    {
        var queryFilter = new QueryFilter
        {
            Field = "Name",
            Operator = QueryOperators.StartsWith,
            Value = "P"
        };
        var builder = new LinqExpressionBuilder();
        builder.Build(queryFilter);

        builder.Expression.Should().NotBeEmpty();
        builder.Expression.Should().Be("Name.StartsWith(@0, StringComparison.OrdinalIgnoreCase)");

        builder.Parameters.Count.Should().Be(1);
        builder.Parameters[0].Should().Be("P");

        var predicate = builder.Expression;
        var parameters = builder.Parameters.ToArray();

        var query = Fruit.Data().AsQueryable();

        var results = query.Where(predicate, parameters).ToList();
        results.Should().NotBeEmpty();
        results.Count.Should().Be(3);

    }

    [Fact]
    public void FilterNotStartsWith()
    {
        var queryFilter = new QueryFilter
        {
            Field = "Name",
            Operator = QueryOperators.NotStartsWith,
            Value = "P"
        };
        var builder = new LinqExpressionBuilder();
        builder.Build(queryFilter);

        builder.Expression.Should().NotBeEmpty();
        builder.Expression.Should().Be("!Name.StartsWith(@0, StringComparison.OrdinalIgnoreCase)");

        builder.Parameters.Count.Should().Be(1);
        builder.Parameters[0].Should().Be("P");

        var predicate = builder.Expression;
        var parameters = builder.Parameters.ToArray();

        var query = Fruit.Data().AsQueryable();

        var results = query.Where(predicate, parameters).ToList();
        results.Should().NotBeEmpty();
        results.Count.Should().Be(7);
    }

    [Fact]
    public void FilterEndsWith()
    {
        var queryFilter = new QueryFilter
        {
            Field = "Name",
            Operator = QueryOperators.EndsWith,
            Value = "berry"
        };
        var builder = new LinqExpressionBuilder();
        builder.Build(queryFilter);

        builder.Expression.Should().NotBeEmpty();
        builder.Expression.Should().Be("Name.EndsWith(@0, StringComparison.OrdinalIgnoreCase)");

        builder.Parameters.Count.Should().Be(1);
        builder.Parameters[0].Should().Be("berry");

        var predicate = builder.Expression;
        var parameters = builder.Parameters.ToArray();

        var query = Fruit.Data().AsQueryable();

        var results = query.Where(predicate, parameters).ToList();
        results.Should().NotBeEmpty();
        results.Count.Should().Be(3);
    }

    [Fact]
    public void FilterNotEndsWith()
    {
        var queryFilter = new QueryFilter
        {
            Field = "Name",
            Operator = QueryOperators.NotEndsWith,
            Value = "berry"
        };
        var builder = new LinqExpressionBuilder();
        builder.Build(queryFilter);

        builder.Expression.Should().NotBeEmpty();
        builder.Expression.Should().Be("!Name.EndsWith(@0, StringComparison.OrdinalIgnoreCase)");

        builder.Parameters.Count.Should().Be(1);
        builder.Parameters[0].Should().Be("berry");

        var predicate = builder.Expression;
        var parameters = builder.Parameters.ToArray();

        var query = Fruit.Data().AsQueryable();

        var results = query.Where(predicate, parameters).ToList();
        results.Should().NotBeEmpty();
        results.Count.Should().Be(7);
    }

    [Fact]
    public void FilterNull()
    {
        var builder = new LinqExpressionBuilder();
        builder.Build(null);

        builder.Expression.Should().BeEmpty();

        builder.Parameters.Count.Should().Be(0);
    }

    [Fact]
    public void FilterEmpty()
    {
        var queryFilter = new QueryFilter();

        var builder = new LinqExpressionBuilder();
        builder.Build(queryFilter);

        builder.Expression.Should().BeEmpty();

        builder.Parameters.Count.Should().Be(0);
    }


    [Fact]
    public void IsValidNull()
    {
        QueryGroup? query = null;
        var isValid = LinqExpressionBuilder.IsValid(query);
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValidGroupEmpty()
    {
        var query = new QueryGroup();
        var isValid = LinqExpressionBuilder.IsValid(query);
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValidGroupFilterEmpty()
    {
        var query = new QueryGroup
        {
            Filters = [new QueryFilter()]
        };
        var isValid = LinqExpressionBuilder.IsValid(query);
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValidGroupNestedFilterEmpty()
    {
        var query = new QueryGroup
        {
            Filters = [new QueryGroup()]
        };
        var isValid = LinqExpressionBuilder.IsValid(query);
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValidGroupNestedFilter()
    {
        var query = new QueryGroup
        {
            Filters =
            [
                new QueryGroup
                {
                    Filters =
                    [
                        new QueryFilter { Field = "Rank", Value = 7 }
                    ]
                }
            ]
        };

        var isValid = LinqExpressionBuilder.IsValid(query);
        isValid.Should().BeTrue();
    }

    [Fact]
    public void IsValidFilterEmpty()
    {
        var query = new QueryFilter();
        var isValid = LinqExpressionBuilder.IsValid(query);
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValidFilter()
    {
        var query = new QueryFilter { Field = "Rank", Value = 7 };
        var isValid = LinqExpressionBuilder.IsValid(query);
        isValid.Should().BeTrue();
    }

    [Fact]
    public void IsValidGroupFilter()
    {
        var query = new QueryGroup
        {
            Filters = [new QueryFilter { Field = "Name", Value = "Test" }]
        };
        var isValid = LinqExpressionBuilder.IsValid(query);
        isValid.Should().BeTrue();
    }
}
