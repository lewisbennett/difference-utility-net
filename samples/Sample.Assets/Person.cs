namespace Sample.Assets
{
    public class Person
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int ID { get; set; }

        public override string ToString()
        {
            return $"{FirstName} {LastName} (ID: {ID})";
        }
    }
}
