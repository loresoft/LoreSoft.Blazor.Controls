using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sample.Shared
{
    public class Person
    {
        public Person() { }
        public Person(string firstname, string lastname, int age, string location)
        {
            Firstname = firstname;
            Lastname = lastname;
            Age = age;
            Location = location;
        }

        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string FullName => Firstname + " " + Lastname;
        public int Age { get; set; }
        public string Location { get; set; }
    }

    public static class Data
    {
        public static IReadOnlyCollection<Person> PersonList { get; } = new List<Person>
        {
            new Person {Firstname = "John", Lastname = "Smith"},
            new Person {Firstname = "Jane", Lastname = "Doe"},
            new Person {Firstname = "Tom", Lastname = "Jones"},
            new Person {Firstname = "Fred", Lastname = "Gouch"},
            new Person {Firstname = "John", Lastname = "Philips"},
            new Person {Firstname = "Jon", Lastname = "Thomas"}
        };
    }

    public class Starship
    {
        [Required]
        [StringLength(16, ErrorMessage = "Identifier too long (16 character limit).")]
        public string Identifier { get; set; }

        public string Description { get; set; }

        [Required]
        public string Classification { get; set; }

        [Range(1, 100000, ErrorMessage = "Accommodation invalid (1-100000).")]
        public int MaximumAccommodation { get; set; }

        [Required]
        [Range(typeof(bool), "true", "true",
            ErrorMessage = "This form disallows unapproved ships.")]
        public bool IsValidatedDesign { get; set; }

        [Required]
        public DateTime ProductionDate { get; set; }

        [Required]
        public Person Captain { get; set; }
    }
}
