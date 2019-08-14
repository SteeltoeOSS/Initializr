namespace Steeltoe.Initializr.Services.Mustache
{
    public class TemplateKey
    {
        public string Name { get;}

        public TemplateVersion Version { get;}

        public TemplateKey(string name, TemplateVersion version)
        {
            Name = name;
            Version = version;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ Version.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is TemplateKey key && (Name.Equals(key.Name) && Version.Equals(key.Version));
        }
    }
}