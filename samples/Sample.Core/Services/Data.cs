using System.Collections.Generic;

using Bogus;

using Sample.Core.Models;

using Person = Sample.Core.Models.Person;

namespace Sample.Core.Services;

public static class Data
{
    public static IReadOnlyCollection<Person> PersonList { get; } = new List<Person>
    {
        new() {Id = 1, FirstName = "John", LastName = "Smith"},
        new() {Id = 2, FirstName = "Jane", LastName = "Doe"},
        new() {Id = 3, FirstName = "Tom", LastName = "Jones"},
        new() {Id = 4, FirstName = "Fred", LastName = "Gouch"},
        new() {Id = 5, FirstName = "John", LastName = "Philips"},
        new() {Id = 6, FirstName = "Jon", LastName = "Thomas"}
    };

    public static IReadOnlyCollection<StateLocation> StateList { get; } = new List<StateLocation>
    {
        new("AL - Alabama", "AL"),
        new("AK - Alaska", "AK"),
        new("AZ - Arizona", "AZ"),
        new("AR - Arkansas", "AR"),
        new("CA - California", "CA"),
        new("CO - Colorado", "CO"),
        new("CT - Connecticut", "CT"),
        new("DE - Delaware", "DE"),
        new("DC - District Of Columbia", "DC"),
        new("FL - Florida", "FL"),
        new("GA - Georgia", "GA"),
        new("HI - Hawaii", "HI"),
        new("ID - Idaho", "ID"),
        new("IL - Illinois", "IL"),
        new("IN - Indiana", "IN"),
        new("IA - Iowa", "IA"),
        new("KS - Kansas", "KS"),
        new("KY - Kentucky", "KY"),
        new("LA - Louisiana", "LA"),
        new("ME - Maine", "ME"),
        new("MD - Maryland", "MD"),
        new("MA - Massachusetts", "MA"),
        new("MI - Michigan", "MI"),
        new("MN - Minnesota", "MN"),
        new("MS - Mississippi", "MS"),
        new("MO - Missouri", "MO"),
        new("MT - Montana", "MT"),
        new("NE - Nebraska", "NE"),
        new("NV - Nevada", "NV"),
        new("NH - New Hampshire", "NH"),
        new("NJ - New Jersey", "NJ"),
        new("NM - New Mexico", "NM"),
        new("NY - New York", "NY"),
        new("NC - North Carolina", "NC"),
        new("ND - North Dakota", "ND"),
        new("OH - Ohio", "OH"),
        new("OK - Oklahoma", "OK"),
        new("OR - Oregon", "OR"),
        new("PA - Pennsylvania", "PA"),
        new("RI - Rhode Island", "RI"),
        new("SC - South Carolina", "SC"),
        new("SD - South Dakota", "SD"),
        new("TN - Tennessee", "TN"),
        new("TX - Texas", "TX"),
        new("UT - Utah", "UT"),
        new("VT - Vermont", "VT"),
        new("VA - Virginia", "VA"),
        new("WA - Washington", "WA"),
        new("WV - West Virginia", "WV"),
        new("WI - Wisconsin", "WI"),
        new("WY - Wyoming", "WY")
    };

    private static readonly List<string> TopBankNames = new()
    {
        "JPMorgan Chase",
        "Bank of America",
        "Wells Fargo",
        "Citibank",
        "U.S. Bank",
        "PNC Bank",
        "Truist",
        "Goldman Sachs",
        "TD Bank",
        "Capital One",
        "HSBC",
        "Fifth Third Bank",
        "KeyBank",
        "Regions Bank",
        "BMO Harris Bank",
        "M&T Bank",
        "Citizens Bank",
        "Morgan Stanley",
        "Santander Bank",
        "Ally Bank"
    };

    public static IReadOnlyCollection<Person> GeneratePeople(int count = 100)
    {
        var generator = new Faker<Person>()
            .RuleFor(p => p.Id, f => f.IndexGlobal)
            .RuleFor(p => p.FirstName, f => f.Name.FirstName())
            .RuleFor(p => p.LastName, f => f.Name.LastName())
            .RuleFor(p => p.Score, f => f.Random.Int(1, 100))
            .RuleFor(p => p.Location, f => f.Address.City())
            .RuleFor(p => p.Birthday, f => f.Date.Past(60));

        return generator.Generate(count);
    }

    public static IReadOnlyCollection<Bank> GenerateBanks(int count = 1000)
    {
        var generator = new Faker<Bank>()
            .RuleFor(b => b.Id, f => f.IndexGlobal)
            .RuleFor(b => b.Identifier, f => f.Random.Guid())
            .RuleFor(b => b.BankName, f => f.Random.CollectionItem(TopBankNames))
            .RuleFor(b => b.RoutingNumber, f => f.Finance.RoutingNumber())
            .RuleFor(b => b.AccountNumber, f => f.Finance.Account())
            .RuleFor(b => b.IBAN, f => f.Finance.Iban())
            .RuleFor(b => b.SwiftBIC, f => f.Finance.Bic())
            .RuleFor(b => b.IsActive, f => f.Random.Bool(0.8f))
            .RuleFor(b => b.Created, f => f.Date.Past(10));

        return generator.Generate(count);
    }
}
