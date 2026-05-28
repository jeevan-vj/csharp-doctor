using System.Security.Cryptography;

class SqlCommand
{
    public string CommandText { get; set; } = string.Empty;
}

class Bad
{
    void M(string input)
    {
        var cmd = new SqlCommand();
        cmd.CommandText = "SELECT * FROM t WHERE id = " + input;
        var h = MD5.Create();
    }
}
