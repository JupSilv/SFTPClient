using Renci.SshNet;
using Renci.SshNet.Sftp;
using Services.Interfaces;
using System.IO;
using System.Text;

namespace Services;

public class SftService : ISftService
{
    private string host = "host-link";
    private int port = 22; // Porta padrão do SFTP é 22
    private string username = "user";
    private string password = "password";

    public IEnumerable<ISftpFile> ListAllFilesAsync(string remoteDirectory = ".")
    {
        // Assumindo que 'SftpClientConnect' retorna uma instância de 'SftpClient'
        // e implementa IDisposable corretamente.
        using var client = SftpClientConnect(host, port, username, password);

        try
        {
            client.Connect();
            var files = client.ListDirectory(remoteDirectory);
            return files;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao listar arquivos: {ex}");
            throw; // Relançar a exceção pode ser adequado, dependendo do seu manejo de erro.
        }
    }

    private SftpClient SftpClientConnect(string host, int port, string userName, string password)
    {
        SftpClient client = new SftpClient(host, port, userName, password);
        return client;
    }

    public void UploadFile(string localFilePath, string remoteFilePath)
    {
        try
        {
           

            UploadFile(localFilePath);

            bool hasPermission = CheckUploadPermission(remoteFilePath);

            Console.WriteLine($"Permissão para fazer upload: {hasPermission}");

            var lista = (ListAllDirectoriesAsync(remoteFilePath).Result).ToList();

            remoteFilePath = lista[2];

            using var client = SftpClientConnect(host, port, username, password);
            client.Connect();

            if (!client.Exists(remoteFilePath))
            {
                client.CreateDirectory(remoteFilePath);
            }

            using FileStream s = File.OpenRead(localFilePath);
            //client.UploadFile(s, remoteFilePath);
            client.UploadFile(s, "/home/user/" + remoteFilePath, null);
        }
        catch (Renci.SshNet.Common.SftpPathNotFoundException ex)
        {
            Console.WriteLine($"O caminho especificado não existe: {ex.Message}");
        }
    }

    public void DeleteFile(string localFilePath)
    {
        throw new NotImplementedException();
    }

    private void ListDirectories(SftpClient client, string directory, List<string> directories)
    {
        directories.Add(directory); // Add the current directory to the list

        var files = client.ListDirectory(directory);
        foreach (var file in files)
        {
            if (file.IsDirectory && !file.Name.StartsWith("."))
            {
                // Recursively list subdirectories
                ListDirectories(client, file.FullName, directories);
            }
        }
    }

    public async Task<IEnumerable<string>> ListAllDirectoriesAsync(string remoteDirectory = ".")
    {
        var directories = new List<string>();

        using var client = SftpClientConnect(host, port, username, password);

        try
        {
            client.Connect();

            await Task.Run(() => ListDirectories(client, remoteDirectory, directories));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
        finally
        {
            client.Disconnect();
        }

        return directories;
    }

    public void UploadFile(string localFilePath)
    {
        // Caminho absoluto no servidor SFTP onde o arquivo será salvo
        //string remoteDirectory = "/from_interfile/mrshborges/interfiletest";
        string remoteDirectory = "/from_interfile/mrshborges";
        string remoteFileName = Path.GetFileName(localFilePath);
        string remoteFullPath = Path.Combine(remoteDirectory, remoteFileName).Replace('\\', '/');

        using var sftp = SftpClientConnect(host, port, username, password);

        try
        {
            sftp.Connect();
            Console.WriteLine("Conectado ao servidor SFTP.");

            // Verifica se o diretório remoto existe; se não, cria-o
            if (!sftp.Exists(remoteDirectory))
            {
                sftp.CreateDirectory(remoteDirectory);
                Console.WriteLine($"Diretório criado: {remoteDirectory}");
            }

            // Fazendo o upload do arquivo
            using (var fileStream = new FileStream(localFilePath, FileMode.Open))
            {
                //sftp.UploadFile(fileStream, remoteFullPath);
                sftp.UploadFile(fileStream,  remoteFullPath, true, null);
                Console.WriteLine($"Arquivo enviado com sucesso para: {remoteFullPath}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao enviar arquivo: {ex.Message}");
        }
        finally
        {
            if (sftp.IsConnected)
            {
                sftp.Disconnect();
                Console.WriteLine("Desconectado do servidor SFTP.");
            }
        }
    }

    public bool CheckUploadPermission(string remoteDirectory)
    {
        using var sftp = SftpClientConnect(host, port, username, password);

        try
        {
            sftp.Connect();
            Console.WriteLine("Conectado ao servidor SFTP.");

            // Tenta criar um diretório temporário para teste de permissão
            string testDirectory = Path.Combine(remoteDirectory, "test_permission").Replace('\\', '/');
            if (!sftp.Exists(testDirectory))
            {
                sftp.CreateDirectory(testDirectory);
            }

            // Tenta fazer o upload de um arquivo pequeno para o diretório de teste
            string testFilePath = Path.Combine(testDirectory, "test.txt").Replace('\\', '/');
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes("test")))
            {
                sftp.UploadFile(stream, testFilePath);
            }

            // Se chegou até aqui, o upload foi bem-sucedido
            // Apaga o arquivo e diretório de teste
            sftp.DeleteFile(testFilePath);
            sftp.DeleteDirectory(testDirectory);

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Não é possível fazer upload no diretório especificado: {ex.Message}");
            return false;
        }
        finally
        {
            if (sftp.IsConnected)
            {
                sftp.Disconnect();
                Console.WriteLine("Desconectado do servidor SFTP.");
            }
        }
    }

    public void DownloadFile(string remoteFilePath, string localFilePath)
    {
        using (var sftp = new SftpClient(host, port, username, password))
        {
            try
            {
                string remoteFilePathDownload = "/from_interfile/mrshborges/interfiletest/55301125_RCG_FRONT - UPLOAD.pdf";
                CheckDownloadPermission(remoteFilePathDownload);

                sftp.Connect();
                Console.WriteLine("Conectado ao servidor SFTP.");

                if (sftp.Exists(remoteFilePath))
                {
                    using (var fileStream = File.Create(localFilePath))
                    {
                        sftp.DownloadFile(remoteFilePath, fileStream);
                        Console.WriteLine($"Arquivo {remoteFilePath} baixado com sucesso para {localFilePath}.");
                    }
                }
                else
                {
                    Console.WriteLine($"O arquivo {remoteFilePath} não existe no servidor SFTP.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao baixar o arquivo: {ex.Message}");
            }
            finally
            {
                if (sftp.IsConnected)
                {
                    sftp.Disconnect();
                    Console.WriteLine("Desconectado do servidor SFTP.");
                }
            }
        }
    }

    public bool CheckDownloadPermission(string remoteFilePath)
    {
        using (var sftp = new SftpClient(host, port, username, password))
        {
            try
            {
                sftp.Connect();
                Console.WriteLine("Conectado ao servidor SFTP.");

                // Tenta abrir o arquivo remoto para leitura
                using (var stream = sftp.OpenRead(remoteFilePath))
                {
                    Console.WriteLine($"Permissão de leitura (download) confirmada para o arquivo: {remoteFilePath}");
                    return true; // Conseguiu abrir o arquivo, tem permissão de leitura
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao verificar permissão de download: {ex.Message}");
                return false; // Não conseguiu abrir o arquivo, pode não ter permissão de leitura
            }
            finally
            {
                if (sftp.IsConnected)
                {
                    sftp.Disconnect();
                    Console.WriteLine("Desconectado do servidor SFTP.");
                }
            }
        }
    }
}