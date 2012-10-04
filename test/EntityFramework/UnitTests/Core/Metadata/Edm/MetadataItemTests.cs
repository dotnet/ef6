namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Linq;
    using Xunit;

    public class MetadataItemTests
    {
        [Fact]
        public void Can_get_and_set_annotations()
        {
            var entityType = new EntityType();
            var dataModelAnnotation = new DataModelAnnotation();

            Assert.Empty(entityType.Annotations);

            entityType.Annotations.Add(dataModelAnnotation);

            Assert.Same(dataModelAnnotation, entityType.Annotations.Single());
        } 
    }
}