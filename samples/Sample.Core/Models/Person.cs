using System;

namespace Sample.Core.Models
{
    public class Person
    {
        public Person() { }
        public Person(string firstName, string lastName, int age, string location)
        {
            FirstName = firstName;
            LastName = lastName;
            Age = age;
            Location = location;
        }

        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => FirstName + " " + LastName;
        public int Age { get; set; }
        public string Location { get; set; }
        public DateTime? Birthday { get; set; }
    }
}
