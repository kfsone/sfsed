namespace SFSEd
{
    public class SFSLeaf
    {
        public string Key { get; }
        public string Original { get; }
        public string Value { get; set; }
        public bool Changed { get => Value == Original; }

        public SFSLeaf(string key, string value)
        {
            Key = key;
            Original = value;
            Value = string.Copy(value);
        }
    };
}
