namespace Sample.Core.Models
{
    public class StateLocation
    {
        public StateLocation(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; set; }  
        public string Value { get; set; }  
    }
}