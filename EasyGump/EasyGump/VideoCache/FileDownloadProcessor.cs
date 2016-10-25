using System;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Threading;
using Windows.Storage;
using Windows.Storage.Streams;

namespace EasyGump
{
    public static class DownloadErrorCode
    {
        public const int SUCCESS = 0;
        public const int NETWORK_FAIL = 1;
        public const int TIMEOUT = 2;
        public const int SERVER_ERROR = 3;
        public const int CLIENT_ERROR = 4;
        public const int CANCEL = 5;
    }

    public class FileDownloadProcessor
    {
        #region memeber
        private HttpWebRequest mHttpWebRequest;
        private string mUrl;
        private string mFileName;
        private string mCacheDirName;
        private bool mStop;
        private CancellationTokenSource mReadStreamTimeoutTokenSource = new CancellationTokenSource();
        private int mTimeout = -1;
        public long mRequestAliveTick = -1;
        private Timer mCheckTimeoutTimer;
        private IRandomAccessStream mRandomAccessStream;
        #endregion

        #region method
        public void Init(string url, string cacheDirName, string fileName)
        {
            mStop = false;
            mTimeout = 20;
            mUrl = url;
            mFileName = fileName;
            mCacheDirName = cacheDirName;
            mHttpWebRequest = WebRequest.Create(url) as HttpWebRequest;
            mHttpWebRequest.Method = "GET";
            mHttpWebRequest.AllowReadStreamBuffering = false;
            mCheckTimeoutTimer = new Timer(OnCheckRequestTimeoutTimerCallback, null, 0, 3000);
        }
        public async Task<int> Start()
        {
            try
            {
                UpdateRequestAliveTick();

                HttpWebResponse response = await mHttpWebRequest.GetResponseAsync() as HttpWebResponse;

                UpdateRequestAliveTick();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    mRandomAccessStream = await FetchDownloadFileRandomAccessStream();
                    Stream stream =  response.GetResponseStream();
                    ulong totalLen = 0;
                    int read0LenghCount = 0;

                    while (stream.CanRead && !mStop)
                    {
                        const int BUFSIZE = 16 * 1024;
                        byte[] buffer = new byte[BUFSIZE];

                        UpdateRequestAliveTick();

                        int len = 0;
                        try
                        {
                            if (!mReadStreamTimeoutTokenSource.IsCancellationRequested)
                            {
                                len = await stream.ReadAsync(buffer, 0,
                                    BUFSIZE, mReadStreamTimeoutTokenSource.Token);
                            }
                            else
                            {
                                return DownloadErrorCode.NETWORK_FAIL;
                            }

                        }
                        catch (TaskCanceledException e)
                        {
                            Debug.WriteLine(e.Message);
                            return DownloadErrorCode.NETWORK_FAIL;
                        }
                        

                        if (len > 0)
                        {
                            
                            Stream fileStream = mRandomAccessStream.GetOutputStreamAt(totalLen).AsStreamForWrite();
                            await fileStream.WriteAsync(buffer, 0, len);
                            if (len != BUFSIZE)
                            {
                                await fileStream.FlushAsync();
                                await mRandomAccessStream.FlushAsync();
                                mRandomAccessStream.Dispose();
                            }
                            
                            totalLen += (ulong)len;
                        }
                        else
                        {
                            read0LenghCount++;
                            if(read0LenghCount > 100)
                            {
                                return DownloadErrorCode.NETWORK_FAIL;
                            }
                        }
                    }
                    if (mStop)
                    {
                        return DownloadErrorCode.CANCEL;
                    }
                }
                else
                {
                    string strLog = "Download url " + mUrl + " http code " + response.StatusCode;
                    Debug.WriteLine(strLog);
                    return DownloadErrorCode.NETWORK_FAIL;
                }
            }
            catch(WebException e)
            {
                Debug.WriteLine(e.Message);
            }
            catch(IOException e)
            {
                Debug.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            return DownloadErrorCode.SUCCESS;
        }
        public void Stop()
        {
            mStop = true;
        }
        private void UpdateRequestAliveTick()
        {
            mRequestAliveTick = DateTime.Now.Ticks;
        }
        private void OnCheckRequestTimeoutTimerCallback(object state)
        {
            if (mRequestAliveTick > 0)
            {
                long diffSeconds = (DateTime.Now.Ticks - mRequestAliveTick)/10000000;
                if (diffSeconds > mTimeout && mHttpWebRequest != null)
                {
                    try
                    {
                        if (!mReadStreamTimeoutTokenSource.IsCancellationRequested)
                        {
                            mHttpWebRequest.Abort();
                            mReadStreamTimeoutTokenSource.Cancel();
                        }
                        else
                        {
                           
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
            }
        }

        private async Task<IRandomAccessStream> FetchDownloadFileRandomAccessStream()
        {
            //StorageFolder storageFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(mCacheDirName, CreationCollisionOption.OpenIfExists);
            //StorageFile storageFile = await storageFolder.CreateFileAsync(mFileName, CreationCollisionOption.ReplaceExisting);

            StorageFile storageFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(mFileName, CreationCollisionOption.OpenIfExists);

            IRandomAccessStream randomAccessStream = await storageFile.OpenAsync(FileAccessMode.ReadWrite, StorageOpenOptions.None);
         
            return randomAccessStream;
        }
        private void GetFileName(string filePath, out string fileName, out string filePapa)
        {
            fileName = "";
            filePapa = "";

            int nPos = filePath.LastIndexOf('\\');
            if(nPos > 0)
            {
                filePapa = filePath.Substring(0, nPos);
                fileName = filePath.Substring(nPos + 1); 
            }
        }
        #endregion
    }
}
