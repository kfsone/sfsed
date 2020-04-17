namespace SFSEd
{
    /// <summary>
    /// OrderingEntry is used to track which order the domains/properties in a given
    /// Domain were originally loaded (because they are separated into domain vs
    /// property during loading). Could just as easily be replaced with a bitmap of
    /// 0=Domain, 1=Property but *now* I think of that.
    /// </summary>
    public class OrderingEntry
    {
        public OrderingEntry(Domain subDomain) => this.subDomain = subDomain;
        public OrderingEntry(Property property) => this.property = property;

        #region Members
        public Domain subDomain { get; private set; } = null;
        public Property property { get; private set; } = null;
        #endregion
    };

}
