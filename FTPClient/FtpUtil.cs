using Renci.SshNet;

namespace FTPClient;

public class FtpUtil
{
    private string _host { get; set; }
    private string _username { get; set; }
    private string _password { get; set; }

    private readonly SftpClient _sftpClient;

    public FtpUtil(SftpConfigOption option)
    {
        var config = new SftpConfiguration().GetSftpConfig(option);

        this._host = config.Host;
        this._username = config.Username;
        this._password = config.Password;

        _sftpClient = new SftpClient(_host, _username, _password);
    }

    public void Connect()
    {
        if (!_sftpClient.IsConnected)
            _sftpClient.Connect();
    }

    public void Disconnect()
    {
        if (_sftpClient.IsConnected)
            _sftpClient.Disconnect();
    }

    public List<string> ListAllDirectories()
    {
        List<string> lstReturn = new List<string>();

        var dir = _sftpClient.ListDirectory("");
        foreach (var item in dir)
        {
            if (item.IsDirectory)
            {
                //lstReturn.Add(item.Name);
                var deepPath = _sftpClient.ListDirectory(item.FullName);
                foreach (var deep in deepPath)
                {
                    lstReturn.Add(deep.FullName);
                }
            }
        }
        return lstReturn;
    }

    public void DownloadFileFromDirectory(string sourcePath, string destPath)
    {
        if (!_sftpClient.IsConnected)
            throw new Exception("Cliente FTP não conectado");

        DownloadFile(sourcePath, destPath);
    }

    public void DownloadAllFileFromDirectory(string sourcePath, string destPath, DateTime? date = null)
    {
        if (!_sftpClient.IsConnected)
            throw new Exception("Cliente FTP não conectado");

        var files = ListItemsInDirectory(sourcePath, date);

        if (files.Count() == 0)
            throw new Exception("Nenhum arquivo encontrado para esta data");

        foreach (var file in files)
            DownloadFile(file, destPath);
    }

    public IEnumerable<string> ListItemsInDirectory(string dirName, DateTime? date = null)
    {
        if (date != null)
            return _sftpClient.ListDirectory(dirName).Where(w => w.LastWriteTime.Date == date.Value.Date).Select(s => s.FullName).ToList();

        return _sftpClient.ListDirectory(dirName).Select(s => s.FullName).ToList();
    }

    private void DownloadFile(string sourcePath, string destPath)
    {
        string arqOut = destPath + Path.GetFileName(sourcePath);
        using (Stream outputStream = File.Create(arqOut))
        {
            _sftpClient.DownloadFile(sourcePath, outputStream);

            FileInfo arq = new FileInfo(arqOut);

            string tipo = arq.Extension == ".dat" ? "INDICE" : "DOCUMENTO";
            //using (UnitOfWork ctx = new UnitOfWork(new DBContextMarshRisk()))
            //{
            //    ctx.LogFtp.RegistraLog(arq, sourcePath, tipo);
            //}
        }
    }

    //public void UploadFile(string sourcePath, string destPath)
    //{
    //    try
    //    {
    //        if (!_sftpClient.IsConnected)
    //            throw new Exception("Cliente FTP não conectado");

    //        if (_sftpClient.WorkingDirectory != destPath)
    //            _sftpClient.ChangeDirectory(destPath);

    //        using (Stream inputStream = File.OpenRead(sourcePath))
    //        {
    //            _sftpClient.UploadFile(inputStream, Path.GetFileName(sourcePath));
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        throw ex;
    //    }
    //}

    public void UploadFile(string sourcePath, string destPath)
    {
        if (string.IsNullOrEmpty(sourcePath))
            throw new ArgumentException("Caminho de origem não pode ser nulo ou vazio.", nameof(sourcePath));

        if (string.IsNullOrEmpty(destPath))
            throw new ArgumentException("Caminho de destino não pode ser nulo ou vazio.", nameof(destPath));

        if (!File.Exists(sourcePath))
            throw new FileNotFoundException("O arquivo de origem não foi encontrado.", sourcePath);

        if (!_sftpClient.IsConnected)
            throw new InvalidOperationException("Cliente FTP não conectado.");

        string destDirectory = Path.GetDirectoryName(destPath);
        if (!_sftpClient.Exists(destDirectory))
            throw new DirectoryNotFoundException($"O diretório de destino '{destDirectory}' não existe no servidor SFTP.");

        // Verifica se o diretório de trabalho atual é diferente do diretório de destino e muda se necessário.
        if (_sftpClient.WorkingDirectory != destDirectory)
            _sftpClient.ChangeDirectory(destDirectory);

        using (Stream inputStream = File.OpenRead(sourcePath))
        {
            _sftpClient.UploadFile(inputStream, Path.GetFileName(destPath), true);
        }
    }


    public void UploadFiles(string[] sourcePaths, string destPath)
    {
        try
        {
            if (!_sftpClient.IsConnected)
                throw new Exception("Cliente FTP não conectado");

            _sftpClient.ChangeDirectory(destPath);

            foreach (var path in sourcePaths)
            {
                using (Stream inputStream = File.OpenRead(path))
                {
                    _sftpClient.UploadFile(inputStream, Path.GetFileName(path));
                }
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    //public void DownloadAllFileFromDirectory(string v, object p, DateTime date)
    //{
    //    throw new NotImplementedException();
    //}
}