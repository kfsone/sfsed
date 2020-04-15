namespace SFSEd
{
    public class SFSLeaf
    {
        public string Key { get; } 
        public string Value { get; }

        public SFSLeaf(string key, string value)
        {
            Key = key;
            Value = value;
        }
    };
}
