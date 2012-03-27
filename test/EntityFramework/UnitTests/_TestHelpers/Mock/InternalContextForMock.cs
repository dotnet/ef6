namespace System.Data.Entity
{

    /// <summary>
    /// A derived InternalContext implementation that exposes a parameterless constructor
    /// that creates a mocked underlying DbContext such that the internal context can
    /// also be mocked.
    /// </summary>
    internal abstract class InternalContextForMock : InternalContextForMock<DbContext>
    {
    }
}
