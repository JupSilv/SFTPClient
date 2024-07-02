using Renci.SshNet.Sftp;

namespace Services.Interfaces;

public interface ISftService
{
    IEnumerable<ISftpFile> ListAllFilesAsync(string remoteDirectory = ".");

    void UploadFile(string localFilePath, string remoteFilePath);
    
    void DownloadFile(string localFilePath, string remoteFilePath);
    
    void DeleteFile(string localFilePath);

    Task<IEnumerable<string>> ListAllDirectoriesAsync(string remoteDirectory = ".");
}