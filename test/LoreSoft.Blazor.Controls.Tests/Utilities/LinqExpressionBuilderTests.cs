using System.Linq.Dynamic.Core;

using LoreSoft.Blazor.Controls.Extensions;
using LoreSoft.Blazor.Controls.Tests.Models;
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

        Assert.NotEmpty(builder.Expression);
        Assert.Equal("Rank == @0", builder.Expression);

        Assert.Single(builder.Parameters);
        Assert.Equal(7, builder.Parameters[0]);

        var predicate = builder.Expression;
        var parameters = builder.Parameters.ToArray();

        var query = Fruit.Data().AsQueryable();

        var results = query.Where(predicate, parameters).ToList();
        Assert.NotEmpty(results);
        Assert.Equal(3, results.Count);
    }

    [Fact]
    public void FilterGuid()
    {
        var pearId = new Guid("3a1ec4ee-239c-41e5-b934-fbe4ce8113df");
        var queryFilter = new QueryFilter { Field = "Id", Value = pearId };

        var builder = new LinqExpressionBuilder();
        builder.Build(queryFilter);

        Assert.NotEmpty(builder.Expression);
        Assert.Equal("Id == @0", builder.Expression);

        Assert.Single(builder.Parameters);
        Assert.Equal(pearId, builder.Parameters[0]);

        var predicate = builder.Expression;
        var parameters = builder.Parameters.ToArray();

        var query = Fruit.Data().AsQueryable();

        var results = query.Where(predicate, parameters).ToList();
        Assert.NotEmpty(results);
        Assert.Single(results);
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

        Assert.NotEmpty(builder.Expression);
        Assert.Equal("(Rank == @0 or Name == @1)", builder.Expression);

        Assert.Equal(2, builder.Parameters.Count);
        Assert.Equal(7, builder.Parameters[0]);
        Assert.Equal("Apple", builder.Parameters[1]);

        var predicate = builder.Expression;
        var parameters = builder.Parameters.ToArray();

        var query = Fruit.Data().AsQueryable();

        var results = query.Where(predicate, parameters).ToList();
        Assert.NotEmpty(results);
        Assert.Equal(4, results.Count);
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

        Assert.NotEmpty(builder.Expression);
        Assert.Equal("(Rank == @0 and Name == @1)", builder.Expression);

        Assert.Equal(2, builder.Parameters.Count);
        Assert.Equal(7, builder.Parameters[0]);
        Assert.Equal("Blueberry", builder.Parameters[1]);

        var predicate = builder.Expression;
        var parameters = builder.Parameters.ToArray();

        var query = Fruit.Data().AsQueryable();

        var results = query.Where(predicate, parameters).ToList();
        Assert.NotEmpty(results);
        Assert.Single(results);
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

        Assert.NotEmpty(builder.Expression);
        Assert.Equal("(Rank > @0 and (Name == @1 or Name == @2))", builder.Expression);

        Assert.Equal(3, builder.Parameters.Count);
        Assert.Equal(5, builder.Parameters[0]);
        Assert.Equal("Strawberry", builder.Parameters[1]);
        Assert.Equal("Blueberry", builder.Parameters[2]);


        var predicate = builder.Expression;
        var parameters = builder.Parameters.ToArray();

        var query = Fruit.Data().AsQueryable();

        var results = query.Where(predicate, parameters).ToList();
        Assert.NotEmpty(results);
        Assert.Equal(2, results.Count);

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

        Assert.NotEmpty(builder.Expression);
        Assert.Equal("Name != NULL && Name.Contains(@0, StringComparison.OrdinalIgnoreCase)", builder.Expression);

        Assert.Single(builder.Parameters);
        Assert.Equal("berry", builder.Parameters[0]);

        var predicate = builder.Expression;
        var parameters = builder.Parameters.ToArray();

        var query = Fruit.Data().AsQueryable();

        var results = query.Where(predicate, parameters).ToList();
        Assert.NotEmpty(results);
        Assert.Equal(3, results.Count);

    }

    [Fact]
    public void FilterContainsNull()
    {
        var queryFilter = new QueryFilter
        {
            Field = "Description",
            Operator = QueryOperators.Contains,
            Value = "berry"
        };
        var builder = new LinqExpressionBuilder();
        builder.Build(queryFilter);

        Assert.NotEmpty(builder.Expression);
        Assert.Equal("Description != NULL && Description.Contains(@0, StringComparison.OrdinalIgnoreCase)", builder.Expression);

        Assert.Single(builder.Parameters);
        Assert.Equal("berry", builder.Parameters[0]);

        var predicate = builder.Expression;
        var parameters = builder.Parameters.ToArray();

        var query = Fruit.Data().AsQueryable();

        var results = query.Where(predicate, parameters).ToList();
        Assert.Empty(results);
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

        Assert.NotEmpty(builder.Expression);
        Assert.Equal("Description == NULL", builder.Expression);

        Assert.Empty(builder.Parameters);

        var predicate = builder.Expression;
        var parameters = builder.Parameters.ToArray();

        var query = Fruit.Data().AsQueryable();

        var results = query.Where(predicate, parameters).ToList();
        Assert.NotEmpty(results);
        Assert.Equal(10, results.Count);

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

        Assert.NotEmpty(builder.Expression);
        Assert.Equal("Name != NULL", builder.Expression);

        Assert.Empty(builder.Parameters);

        var predicate = builder.Expression;
        var parameters = builder.Parameters.ToArray();

        var query = Fruit.Data().AsQueryable();

        var results = query.Where(predicate, parameters).ToList();
        Assert.NotEmpty(results);
        Assert.Equal(10, results.Count);

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

        Assert.NotEmpty(builder.Expression);
        Assert.Equal("Name != NULL && !Name.Contains(@0, StringComparison.OrdinalIgnoreCase)", builder.Expression);

        Assert.Single(builder.Parameters);
        Assert.Equal("berry", builder.Parameters[0]);

        var predicate = builder.Expression;
        var parameters = builder.Parameters.ToArray();

        var query = Fruit.Data().AsQueryable();

        var results = query.Where(predicate, parameters).ToList();
        Assert.NotEmpty(results);
        Assert.Equal(7, results.Count);

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

        Assert.NotEmpty(builder.Expression);
        Assert.Equal("Name != NULL && Name.StartsWith(@0, StringComparison.OrdinalIgnoreCase)", builder.Expression);

        Assert.Single(builder.Parameters);
        Assert.Equal("P", builder.Parameters[0]);

        var predicate = builder.Expression;
        var parameters = builder.Parameters.ToArray();

        var query = Fruit.Data().AsQueryable();

        var results = query.Where(predicate, parameters).ToList();
        Assert.NotEmpty(results);
        Assert.Equal(3, results.Count);

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

        Assert.NotEmpty(builder.Expression);
        Assert.Equal("Name != NULL && !Name.StartsWith(@0, StringComparison.OrdinalIgnoreCase)", builder.Expression);

        Assert.Single(builder.Parameters);
        Assert.Equal("P", builder.Parameters[0]);

        var predicate = builder.Expression;
        var parameters = builder.Parameters.ToArray();

        var query = Fruit.Data().AsQueryable();

        var results = query.Where(predicate, parameters).ToList();
        Assert.NotEmpty(results);
        Assert.Equal(7, results.Count);
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

        Assert.NotEmpty(builder.Expression);
        Assert.Equal("Name != NULL && Name.EndsWith(@0, StringComparison.OrdinalIgnoreCase)", builder.Expression);

        Assert.Single(builder.Parameters);
        Assert.Equal("berry", builder.Parameters[0]);

        var predicate = builder.Expression;
        var parameters = builder.Parameters.ToArray();

        var query = Fruit.Data().AsQueryable();

        var results = query.Where(predicate, parameters).ToList();
        Assert.NotEmpty(results);
        Assert.Equal(3, results.Count);
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

        Assert.NotEmpty(builder.Expression);
        Assert.Equal("Name != NULL && !Name.EndsWith(@0, StringComparison.OrdinalIgnoreCase)", builder.Expression);

        Assert.Single(builder.Parameters);
        Assert.Equal("berry", builder.Parameters[0]);

        var predicate = builder.Expression;
        var parameters = builder.Parameters.ToArray();

        var query = Fruit.Data().AsQueryable();

        var results = query.Where(predicate, parameters).ToList();
        Assert.NotEmpty(results);
        Assert.Equal(7, results.Count);
    }

    [Theory]
    [InlineData(QueryOperators.Contains, "Name != NULL && Name.Contains(@0)")]
    [InlineData(QueryOperators.NotContains, "Name != NULL && !Name.Contains(@0)")]
    [InlineData(QueryOperators.StartsWith, "Name != NULL && Name.StartsWith(@0)")]
    [InlineData(QueryOperators.NotStartsWith, "Name != NULL && !Name.StartsWith(@0)")]
    [InlineData(QueryOperators.EndsWith, "Name != NULL && Name.EndsWith(@0)")]
    [InlineData(QueryOperators.NotEndsWith, "Name != NULL && !Name.EndsWith(@0)")]
    public void FilterStringWithoutStringComparison(string queryOperator, string expectedExpression)
    {
        var queryFilter = new QueryFilter
        {
            Field = "Name",
            Operator = queryOperator,
            Value = "berry"
        };
        var options = new LinqExpressionOptions { StringComparison = null };

        var builder = new LinqExpressionBuilder();
        builder.Build(queryFilter, options);

        Assert.NotEmpty(builder.Expression);
        Assert.Equal(expectedExpression, builder.Expression);

        Assert.Single(builder.Parameters);
        Assert.Equal("berry", builder.Parameters[0]);
    }

    [Fact]
    public void LinqExpressionOptionsDefaultUsesOrdinalIgnoreCase()
    {
        var options = LinqExpressionOptions.Default;

        Assert.Equal(StringComparison.OrdinalIgnoreCase, options.StringComparison);
    }

    [Fact]
    public void LinqExpressionOptionsEmptyHasNoStringComparison()
    {
        var options = LinqExpressionOptions.Empty;

        Assert.Null(options.StringComparison);
    }

    [Fact]
    public void QueryExtensionsFilterUsesStringComparisonOptions()
    {
        var queryFilter = new QueryFilter
        {
            Field = "Name",
            Operator = QueryOperators.Contains,
            Value = "BERRY"
        };
        var options = new LinqExpressionOptions { StringComparison = null };
        var query = Fruit.Data().AsQueryable();

        var defaultResults = query.Filter(queryFilter).ToList();
        var providerCompatibleResults = query.Filter(queryFilter, options).ToList();

        Assert.Equal(3, defaultResults.Count);
        Assert.Empty(providerCompatibleResults);
    }

    [Fact]
    public void QueryExtensionsDataQueryUsesStringComparisonOptions()
    {
        var request = new DataRequest
        {
            Query = new QueryFilter
            {
                Field = "Name",
                Operator = QueryOperators.Contains,
                Value = "BERRY"
            }.AsGroup()
        };
        var options = new LinqExpressionOptions { StringComparison = null };
        var query = Fruit.Data().AsQueryable();

        var defaultResult = query.DataQuery(request);
        var providerCompatibleResult = query.DataQuery(request, options);

        Assert.Equal(3, defaultResult.Total);
        Assert.Equal(0, providerCompatibleResult.Total);
    }

    [Fact]
    public void FilterNull()
    {
        var builder = new LinqExpressionBuilder();
        builder.Build(null);

        Assert.Empty(builder.Expression);

        Assert.Empty(builder.Parameters);
    }

    [Fact]
    public void FilterEmpty()
    {
        var queryFilter = new QueryFilter();

        var builder = new LinqExpressionBuilder();
        builder.Build(queryFilter);

        Assert.Empty(builder.Expression);

        Assert.Empty(builder.Parameters);
    }


    [Fact]
    public void IsValidNull()
    {
        QueryGroup? query = null;
        var isValid = LinqExpressionBuilder.IsValid(query);
        Assert.False(isValid);
    }

    [Fact]
    public void IsValidGroupEmpty()
    {
        var query = new QueryGroup();
        var isValid = LinqExpressionBuilder.IsValid(query);
        Assert.False(isValid);
    }

    [Fact]
    public void IsValidGroupFilterEmpty()
    {
        var query = new QueryGroup
        {
            Filters = [new QueryFilter()]
        };
        var isValid = LinqExpressionBuilder.IsValid(query);
        Assert.False(isValid);
    }

    [Fact]
    public void IsValidGroupNestedFilterEmpty()
    {
        var query = new QueryGroup
        {
            Filters = [new QueryGroup()]
        };
        var isValid = LinqExpressionBuilder.IsValid(query);
        Assert.False(isValid);
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
        Assert.True(isValid);
    }

    [Fact]
    public void IsValidFilterEmpty()
    {
        var query = new QueryFilter();
        var isValid = LinqExpressionBuilder.IsValid(query);
        Assert.False(isValid);
    }

    [Fact]
    public void IsValidFilter()
    {
        var query = new QueryFilter { Field = "Rank", Value = 7 };
        var isValid = LinqExpressionBuilder.IsValid(query);
        Assert.True(isValid);
    }

    [Fact]
    public void IsValidGroupFilter()
    {
        var query = new QueryGroup
        {
            Filters = [new QueryFilter { Field = "Name", Value = "Test" }]
        };
        var isValid = LinqExpressionBuilder.IsValid(query);
        Assert.True(isValid);
    }
}
