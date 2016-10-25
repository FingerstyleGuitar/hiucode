using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Streams;
using System.IO;
using Windows.Storage.FileProperties;
using System.Diagnostics;

namespace EasyGump
{
    class VideoCache
    {
        public class VideoItem
        {
            public string Url;
            public string Md5;
            public bool Cached;
            public VideoItem()
            {
                Url = "";
                Md5 = "";
                Cached = false;
            }
        }

        #region private member
        private const string CACHE_DIR_NAME = "VideoCache";
        private const string VIDEO_DEFAULT_EXT = "pdf";

        private List<VideoItem> mVideoItems;
        #endregion

        #region public method
        public VideoCache()
        {
            mVideoItems = new List<VideoItem>();
        }
        public async Task<StorageFile> GetCacheStorage(string url, string md5)
        {
            for (int i = 0; i < mVideoItems.Count(); i++)
            {
                VideoItem item = mVideoItems[i];
                if(item.Url == url && item.Md5 == md5 && item.Cached)
                {
                    try
                    {
                        string fileName = GetVideoCacheName(item);
                        StorageFolder storageFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(CACHE_DIR_NAME, CreationCollisionOption.OpenIfExists);
                        StorageFile storageFile = await storageFolder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
                        return storageFile;
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
            }
            return null;
        }
        public async void Init(List<VideoItem> items)
        {
            bool cacheDirExist = false;
            try
            {
                await ApplicationData.Current.LocalFolder.GetFolderAsync(CACHE_DIR_NAME);
                cacheDirExist = true;
            }
            catch(Exception e)
            {
                Debug.WriteLine(e.Message);
            }

            if(!cacheDirExist)
            {
                try
                {
                    StorageFolder storageFolder =  await ApplicationData.Current.LocalFolder.CreateFolderAsync(CACHE_DIR_NAME);
                    
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                    return;
                }
            }

            mVideoItems.AddRange(items.Where((item) => {
                return !string.IsNullOrEmpty(item.Md5) && !string.IsNullOrEmpty(item.Md5);
            }));
            StartVideoCache();
        }
        private async void StartVideoCache()
        {
            for (int i = 0; i < mVideoItems.Count(); i++)
            {
                VideoItem item = mVideoItems[i];
                bool videoCached = await CheckVideoItemCached(item);
                if (!videoCached)
                {
                    FileDownloadProcessor downloadProcessor = new FileDownloadProcessor();
                    downloadProcessor.Init(item.Url, CACHE_DIR_NAME, GetVideoCacheName(item));
                    int downloadRet = await downloadProcessor.Start();
                    item.Cached = downloadRet == DownloadErrorCode.SUCCESS;
                }
                else
                {
                    item.Cached = true;
                }
            }
        }
        private async Task<bool> CheckVideoItemCached(VideoItem item)
        {
            if(item.Cached)
            {
                return true;
            }

            string fileName = GetVideoCacheName(item);
            if(string.IsNullOrEmpty(fileName))
            {
                return false;
            }

            string videoMd5 = await CalculateFileMd5(fileName);
            if(string.IsNullOrEmpty(videoMd5))
            {
                return false;
            }

            return videoMd5 == item.Md5;
        }
        private string GetVideoCacheName(VideoItem item)
        {
            string fileExt = VIDEO_DEFAULT_EXT;
            string url = item.Url;
            if(string.IsNullOrEmpty(url))
            {
                return "";
            }
            int pos = url.LastIndexOf('.');
            if(pos > 0)
            {
                fileExt = url.Substring(pos + 1);
            }
            return item.Md5 + "." + VIDEO_DEFAULT_EXT;
        }
        private async Task<string> CalculateFileMd5(string fileName)
        {
            const int bufferSize = 256 * 1024;
            byte[] buffer = new byte[bufferSize];

            var hashProvider = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
            CryptographicHash md5Total = hashProvider.CreateHash();

            StorageFile storageFile = null;
            try
            {
                StorageFolder storageFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(CACHE_DIR_NAME, CreationCollisionOption.OpenIfExists);
                storageFile = await storageFolder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
            }
            catch(Exception)
            {
                return "";
            }

            IRandomAccessStream randomAccessStream = await storageFile.OpenAsync(FileAccessMode.ReadWrite, StorageOpenOptions.None);
            BasicProperties baseProperties = await storageFile.GetBasicPropertiesAsync();
            long fileSize = (long)baseProperties.Size;

            long readBytes = 0;
            do
            {
                int needRead = (int)Math.Min(fileSize - readBytes, buffer.Length);
                
                Stream inputStream = randomAccessStream.GetInputStreamAt((ulong)readBytes).AsStreamForRead();
                await inputStream.ReadAsync(buffer, 0, needRead);
                readBytes += needRead;
                md5Total.Append(CryptographicBuffer.CreateFromByteArray(buffer.Take(needRead).ToArray()));
            }
            while (readBytes < fileSize);

            return CryptographicBuffer.EncodeToHexString(md5Total.GetValueAndReset()).ToLower();
        }
        #endregion
    }
}
