namespace Craftsman.Domain;

using Ardalis.SmartEnum;

public abstract class DbProvider : SmartEnum<DbProvider>
{
    public static readonly DbProvider Postgres = new PostgresType();
    public static readonly DbProvider SqlServer = new SqlServerType();
    public static readonly DbProvider MySql = new MySqlType();

    protected DbProvider(string name, int value) : base(name, value)
    {
    }

    public abstract string PackageInclusionString(string version);
    public abstract string OTelSource();
    public abstract int Port();
    public abstract string DbConnectionStringCompose(string dbHostName, string dbName, string dbUser, string dbPassword);
    public abstract string DbConnectionString(string dbHostName, int? dbPort, string dbName, string dbUser, string dbPassword);

    private class PostgresType : DbProvider
    {
        public PostgresType() : base(nameof(Postgres), 1) { }
        public override string PackageInclusionString(string version)
            => @$"<PackageReference Include=""Npgsql.EntityFrameworkCore.PostgreSQL"" Version=""{version}"" />";
        public override string OTelSource()
            => @$"Npgsql";
        public override int Port()
            => 5432;
        public override string DbConnectionStringCompose(string dbHostName, string dbName, string dbUser,
            string dbPassword)
            => $"Host={dbHostName};Port={5432};Database={dbName};Username={dbUser};Password={dbPassword}";
        public override string DbConnectionString(string dbHostName, int? dbPort, string dbName, string dbUser, string dbPassword)
             => $"Host=localhost;Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword}";
    }

    private class SqlServerType : DbProvider
    {
        public SqlServerType() : base(nameof(SqlServer), 2) { }
        public override string PackageInclusionString(string version)
            => @$"<PackageReference Include=""Microsoft.EntityFrameworkCore.SqlServer"" Version=""{version}"" />";
        public override string OTelSource()
            => @$"Microsoft.EntityFrameworkCore.SqlServer";
        public override int Port()
            => 1433;
        public override string DbConnectionStringCompose(string dbHostName, string dbName, string dbUser, string dbPassword)
            => $"Data Source={dbHostName},{1433};Integrated Security=False;Database={dbName};User ID={dbUser};Password={dbPassword}";
        public override string DbConnectionString(string dbHostName, int? dbPort, string dbName, string dbUser, string dbPassword)
            => $"Data Source=localhost,{dbPort};TrustServerCertificate=True;Integrated Security=False;Database={dbName};User ID={dbUser};Password={dbPassword}";
    }

    private class MySqlType : DbProvider
    {
        private const string Response = "MySql is not supported";
        public MySqlType() : base(nameof(MySql), 3) { }
        public override string PackageInclusionString(string version)
            => throw new Exception(Response);
        public override string OTelSource()
            => throw new Exception(Response);
        public override int Port()
            => throw new Exception(Response);
        public override string DbConnectionStringCompose(string dbHostName, string dbName, string dbUser, string dbPassword)
            => throw new Exception(Response);
        public override string DbConnectionString(string dbHostName, int? dbPort, string dbName, string dbUser, string dbPassword)
             => throw new Exception(Response);
    }
}
