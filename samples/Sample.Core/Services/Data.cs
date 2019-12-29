using System.Collections.Generic;
using Sample.Core.Models;

namespace Sample.Core.Services
{
    public static class Data
    {
        public static IReadOnlyCollection<Person> PersonList { get; } = new List<Person>
        {
            new Person {Id = 1, Firstname = "John", Lastname = "Smith"},
            new Person {Id = 2, Firstname = "Jane", Lastname = "Doe"},
            new Person {Id = 3, Firstname = "Tom", Lastname = "Jones"},
            new Person {Id = 4, Firstname = "Fred", Lastname = "Gouch"},
            new Person {Id = 5, Firstname = "John", Lastname = "Philips"},
            new Person {Id = 6, Firstname = "Jon", Lastname = "Thomas"}
        };

        public static IReadOnlyCollection<StateLocation> StateList { get; } = new List<StateLocation>
        {
            new StateLocation("AL - Alabama", "AL"),
            new StateLocation("AK - Alaska", "AK"),
            new StateLocation("AZ - Arizona", "AZ"),
            new StateLocation("AR - Arkansas", "AR"),
            new StateLocation("CA - California", "CA"),
            new StateLocation("CO - Colorado", "CO"),
            new StateLocation("CT - Connecticut", "CT"),
            new StateLocation("DE - Delaware", "DE"),
            new StateLocation("DC - District Of Columbia", "DC"),
            new StateLocation("FL - Florida", "FL"),
            new StateLocation("GA - Georgia", "GA"),
            new StateLocation("HI - Hawaii", "HI"),
            new StateLocation("ID - Idaho", "ID"),
            new StateLocation("IL - Illinois", "IL"),
            new StateLocation("IN - Indiana", "IN"),
            new StateLocation("IA - Iowa", "IA"),
            new StateLocation("KS - Kansas", "KS"),
            new StateLocation("KY - Kentucky", "KY"),
            new StateLocation("LA - Louisiana", "LA"),
            new StateLocation("ME - Maine", "ME"),
            new StateLocation("MD - Maryland", "MD"),
            new StateLocation("MA - Massachusetts", "MA"),
            new StateLocation("MI - Michigan", "MI"),
            new StateLocation("MN - Minnesota", "MN"),
            new StateLocation("MS - Mississippi", "MS"),
            new StateLocation("MO - Missouri", "MO"),
            new StateLocation("MT - Montana", "MT"),
            new StateLocation("NE - Nebraska", "NE"),
            new StateLocation("NV - Nevada", "NV"),
            new StateLocation("NH - New Hampshire", "NH"),
            new StateLocation("NJ - New Jersey", "NJ"),
            new StateLocation("NM - New Mexico", "NM"),
            new StateLocation("NY - New York", "NY"),
            new StateLocation("NC - North Carolina", "NC"),
            new StateLocation("ND - North Dakota", "ND"),
            new StateLocation("OH - Ohio", "OH"),
            new StateLocation("OK - Oklahoma", "OK"),
            new StateLocation("OR - Oregon", "OR"),
            new StateLocation("PA - Pennsylvania", "PA"),
            new StateLocation("RI - Rhode Island", "RI"),
            new StateLocation("SC - South Carolina", "SC"),
            new StateLocation("SD - South Dakota", "SD"),
            new StateLocation("TN - Tennessee", "TN"),
            new StateLocation("TX - Texas", "TX"),
            new StateLocation("UT - Utah", "UT"),
            new StateLocation("VT - Vermont", "VT"),
            new StateLocation("VA - Virginia", "VA"),
            new StateLocation("WA - Washington", "WA"),
            new StateLocation("WV - West Virginia", "WV"),
            new StateLocation("WI - Wisconsin", "WI"),
            new StateLocation("WY - Wyoming", "WY")
        };

    }
}