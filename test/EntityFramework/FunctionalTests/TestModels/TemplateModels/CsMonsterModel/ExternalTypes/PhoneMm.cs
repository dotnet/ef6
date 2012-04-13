namespace Another.Place
{
    public partial class PhoneMm
    {
        public PhoneMm()
        {
            Extension = "None";
        }

        public string PhoneNumber { get; set; }
        public string Extension { get; set; }
        public PhoneTypeMm PhoneType { get; set; }
    }
}