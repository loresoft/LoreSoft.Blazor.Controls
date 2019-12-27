using System;

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

        public int Id { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string FullName => Firstname + " " + Lastname;
        public int Age { get; set; }
        public string Location { get; set; }
        public DateTime? Birthday { get; set; }
    }
}
