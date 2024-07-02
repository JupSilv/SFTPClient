namespace FTPClient;

public class SftpConfiguration
{
    public string Host { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }

    public SftpConfiguration GetSftpConfig(SftpConfigOption option)
    {
        switch (option)
        {
            case SftpConfigOption.Prod:
                return new SftpConfiguration() { Host = "eft-prd.mgti.mmc.com", Username = "interfile", Password = "JBFd8RfrMBYjDBI" };
            case SftpConfigOption.Holog:
                return new SftpConfiguration() { Host = "205.156.79.5", Username = "interfiletest", Password = "WXTx7PJq57IL2a4" };
            default:
                return null;
        }
    }
}

public enum SftpConfigOption
{
    Prod, Holog
}