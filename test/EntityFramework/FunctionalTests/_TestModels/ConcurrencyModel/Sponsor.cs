namespace ConcurrencyModel
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;

    public class Sponsor
    {
        private readonly ObservableCollection<Team> _teams = new ObservableCollection<Team>();

        [Timestamp]
        public byte[] Version { get; set; }

        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<Team> Teams { get { return _teams; } }
    }
}