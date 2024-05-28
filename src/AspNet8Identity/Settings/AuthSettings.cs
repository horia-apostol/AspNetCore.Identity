namespace AspNet8Identity.Settings
{
    internal sealed class AuthSettings
    {
        public ProviderSettings? Microsoft { get; init; }
        public ProviderSettings? Google { get; init; }
        public ProviderSettings? Facebook { get; init; }
        public ProviderSettings? Twitter { get; init; }
    }
}
