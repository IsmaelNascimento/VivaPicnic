using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Permissions;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using UnityEngine;

public class BlobStorageManager : MonoBehaviour
{
    #region Variables

    private static BlobStorageManager instance;
    public static BlobStorageManager Instance
    {
        get
        {
            if(instance == null)
            {
                instance = FindObjectOfType(typeof(BlobStorageManager)) as BlobStorageManager;
            }

            return instance;
        }
    }
    [Header("Connection Account")]
    private string ConnectionString = "DefaultEndpointsProtocol=http;AccountName=vivapicnic;AccountKey=llmWwmm/gb1dSnQkXQnkv092+jtxHpN6IKuMC1Q8hiwxlghXiSoYR7C2hZAfxtATdR7vS4jGXC8WfldtwRPVVg==;EndpointSuffix=core.windows.net";
    private CloudStorageAccount storageAccount;
    private CloudBlobClient blobClient;
    FileIOPermission permissions;

    #endregion

    #region Methods MonoBehaviour

    private void Start()
    {
		storageAccount = CloudStorageAccount.Parse(ConnectionString);
        blobClient = storageAccount.CreateCloudBlobClient();
        permissions = new FileIOPermission(FileIOPermissionAccess.Read, Application.streamingAssetsPath);
        permissions.AddPathList(FileIOPermissionAccess.Write | FileIOPermissionAccess.Read, Application.streamingAssetsPath);
        permissions.Demand();
    }

    #endregion

    #region CRUD with BlockBlobs

    #region Methods Create

    public async Task CreateContainerAsync(string nameContainer, Action actionBefore = null, Action actionAfter = null)
    {
        try
        {
            actionBefore?.Invoke();
            await GetContainerReference(nameContainer).CreateIfNotExistsAsync();
            actionAfter?.Invoke();
        }
        catch (StorageException)
        {
            Debug.LogError("Error Create a container");
            throw;
        }
    }

    public async Task UploadOrUpdateBlockBlobAsync(string nameContainer, string pathFileDevice, string nameFile, Action actionBefore = null, Action actionAfter = null, Action<Exception> actionError = null)
    {
        try
        {
            CloudBlockBlob blockBlob = GetBlockBlobReference(nameContainer, nameFile);
            actionBefore?.Invoke();
            await blockBlob.UploadFromFileAsync(pathFileDevice + nameFile);
            actionAfter?.Invoke();
        }
        catch (Exception e)
        {
            actionError?.Invoke(e);
        }
    }

    public async Task UploadOrUpdateBlockBlobInDirectoryAsync(string nameContainer, string pathDirectory, string pathFileDevice, string nameFile, Action actionBefore = null, Action actionAfter = null, Action<Exception> actionError = null)
    {
        try
        {
            CloudBlockBlob blockBlob = GetDiretoryReference(nameContainer, pathDirectory).GetBlockBlobReference(nameFile);
            actionBefore?.Invoke();
            await blockBlob.UploadFromFileAsync(pathFileDevice + ("/" + nameFile));
            actionAfter?.Invoke();
        }
        catch(Exception e)
        {
            actionError?.Invoke(e);
        }
    }

    #endregion

    #region Methods Read

    public CloudBlobContainer GetContainerReference(string nameContainer)
    {
        return blobClient.GetContainerReference(nameContainer);
    }

    public CloudBlockBlob GetBlockBlobReference(string nameContainer, string nameBlockBlob)
    {
        return GetContainerReference(nameContainer).GetBlockBlobReference(nameBlockBlob);
    }

    public CloudBlobDirectory GetDiretoryReference(string nameContainer, string pathDirectory)
    {
        return GetContainerReference(nameContainer).GetDirectoryReference(pathDirectory);
    }

    public async Task<IEnumerable<IListBlobItem>> GetListBlobsInContainerAsync(string nameContainer, Action actionBefore = null, Action actionAfter = null)
    {
        BlobContinuationToken token = null;
        BlobResultSegment list = await GetContainerReference(nameContainer).ListBlobsSegmentedAsync(token);
        actionBefore?.Invoke();

        //foreach (IListBlobItem blob in list.Results)
        //{
        //    var nameBlob = blob.Uri.ToString().Split('/')[blob.Uri.ToString().Split('/').Length - 1];
        //    Debug.Log(string.Format("Name: {0} - Link: {0} - Type: {1}", nameBlob, blob.Uri, blob.GetType()));
        //}

        actionAfter?.Invoke();
        return list.Results;
    }

    public IEnumerable<IListBlobItem> GetListBlobsInDirectory(string nameContainer, string pathDirectory, Action actionBefore = null, Action actionAfter = null)
    {
        actionBefore?.Invoke();
        var listBlobs = GetDiretoryReference(nameContainer, pathDirectory).ListBlobs();

        //foreach (IListBlobItem blob in listBlobs)
        //{
        //    var nameBlob = blob.Uri.ToString().Split('/')[blob.Uri.ToString().Split('/').Length - 1];
        //    Debug.Log(string.Format("Name: {0} - Link: {0} - Type: {1}", nameBlob, blob.Uri, blob.GetType()));
        //}

        actionAfter?.Invoke();
        return listBlobs;
    }

    public async Task DownloadBlobAsync(string pathBlobInContainer, string nameBlockBlob, string pathSaveBlob, Action actionBefore = null, Action actionAfter = null)
    {
        CloudBlobContainer container = GetContainerReference(pathBlobInContainer);
        CloudBlockBlob blockBlob = container.GetBlockBlobReference(nameBlockBlob);
        actionBefore?.Invoke();
        await blockBlob.DownloadToFileAsync(Path.Combine(pathSaveBlob, nameBlockBlob), FileMode.Create);
        actionAfter?.Invoke();
    }

    #endregion

    #region Methods Update

    // Call method UploadFileOrUpdateFile for update any type of file

    public async Task UpdateFileTxtBlockBlobAsync(string nameContainer, string pathFileDevice, string pathFileInContainer, string nameFile, string content, Action actionBefore = null, Action actionAfter = null, Action<Exception> actionError = null)
    {
        try
        {
            actionBefore?.Invoke();

            if (!Directory.Exists(pathFileDevice))
            {
                DirectoryInfo di = Directory.CreateDirectory(pathFileDevice);
            }

            var pathFileInDevice = pathFileDevice + nameFile;
            //print(pathFileInDevice);
            File.WriteAllText(pathFileInDevice, content);
            var container = GetContainerReference(nameContainer);
            var blockBlob = container.GetBlockBlobReference(pathFileInContainer + nameFile);
            await blockBlob.UploadFromFileAsync(pathFileInDevice);
            actionAfter?.Invoke();
        }
        catch(Exception e)
        {
            actionError?.Invoke(e);
        }
    }

    #endregion

    #region Methods Delete    

    public async Task DeleteBlockBlobAsync(string nameContainer, string nameBlockBlob, Action actionBefore = null, Action actionAfter = null)
    {
        actionBefore?.Invoke();
        await GetContainerReference(nameContainer).GetBlobReference(nameBlockBlob).DeleteAsync();
        actionAfter?.Invoke();
    }

    public async Task DeleteBlockBlobInDirectoryAsync(string nameContainer, string pathDirectory, string nameBlockBlob, Action actionBefore = null, Action actionAfter = null)
    {
        actionBefore?.Invoke();
        await GetContainerReference(nameContainer).GetDirectoryReference(pathDirectory).GetBlockBlobReference(nameBlockBlob).DeleteIfExistsAsync();
        actionAfter?.Invoke();
    }

    public async Task DeleteFolderBlobAsync(string nameContainer, string pathDirectory, Action actionBefore = null, Action actionAfter = null)
    {
        actionBefore?.Invoke();
        var listBlobs = GetListBlobsInDirectory(nameContainer, pathDirectory);

        foreach (var blob in listBlobs)
            await ((CloudBlockBlob)blob).DeleteIfExistsAsync();

        actionAfter?.Invoke();
    }

    public async Task DeleteContainerAsync(string nameContainer, Action actionBefore = null, Action actionAfter = null)
    {
        actionBefore?.Invoke();
        await GetContainerReference(nameContainer).DeleteAsync();
        actionAfter?.Invoke();
    }

    #endregion

    #endregion
}