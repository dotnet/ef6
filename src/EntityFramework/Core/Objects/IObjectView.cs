namespace System.Data.Entity.Core.Objects
{
    using System.ComponentModel;

    internal interface IObjectView
    {
        void EntityPropertyChanged(object sender, PropertyChangedEventArgs e);
        void CollectionChanged(object sender, CollectionChangeEventArgs e);
    }
}
