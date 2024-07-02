// See https://aka.ms/new-console-template for more information
using FTPClient;
using Services;

ConectarFtp();

void ConectarFtp()
{
    SftService sftService = new SftService();

    string remoteSftp = "/from_interfile/mrshborges";
    //string remoteSftp = "/from_interfile/mrshborges";
    string localPath = "C:\\Users\\crist\\Downloads\\FTP\\55301125_RCG_FRONT - UPLOAD - V2.pdf";
    //var files = sftService.ListAllFilesAsync(remoteSftp);
    sftService.UploadFile(localPath, remoteSftp);

    //string remoteFilePath = "/from_interfile/mrshborges/interfiletest/"; // Caminho do arquivo no servidor SFTP
    //string localFilePath = "C:\\Users\\crist\\Downloads\\FTP\\DOWNLOAD\\55301125_RCG_FRONT - UPLOAD.pdf"; // Caminho local para salvar o arquivo

    //sftService.DownloadFile(remoteFilePath, localFilePath);

    //string sourcePath = @"C:\Users\crist\Downloads\INDICE_20240308_0944.dat";
    //string destinationPath = @"C:\Extract\INDICE_20240308_0944.dat";

    //FtpUtil ftp = new FtpUtil(SftpConfigOption.Prod);

    //ftp.Connect();

    //ftp.UploadFile(sourcePath, destinationPath);

    ////foreach (FileInfo arq in pastaRetorno.GetFiles())
    ////{
    ////    ftp.UploadFile(arq.FullName, "/from_interfile/mrshborges");

    ////    arq.MoveTo($"{dirProcessado.FullName}\\{arq.Name}");
    ////    Console.WriteLine($"{DateTime.Now} - FTP: [ENVIADO] {arq.Name}");
    ////}
    //ftp.Disconnect();
}