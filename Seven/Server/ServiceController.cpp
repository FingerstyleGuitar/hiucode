#include "stdafx.h"
#include "ServiceController.h"
#pragma comment(lib, "ws2_32.lib")

CServiceController::CServiceController() :
m_socketListen(INVALID_SOCKET)
{
}


CServiceController::~CServiceController()
{
}

HRESULT CServiceController::Init(void)
{   
    WSADATA wsaData;
    int nRet = WSAStartup(MAKEWORD(2, 0), &wsaData);
    if (nRet != 0)
    {
        return E_FAIL;
    }

    m_socketListen = socket(AF_INET, SOCK_STREAM, 0);
    if (m_socketListen == INVALID_SOCKET)
    {
        return E_FAIL;
    }
    return S_OK;
}

HRESULT CServiceController::Uninit(void)
{
    if (m_socketListen != INVALID_SOCKET)
    {
        closesocket(m_socketListen);
    }

    int nRet = WSACleanup();
    if (nRet != 0)
    {
        return E_FAIL;
    }

    return S_OK;
}

HRESULT CServiceController::Run(void)
{
    sockaddr_in host;
    sockaddr_in client;
    memset((void*)&host, 0, sizeof sockaddr_in);
    memset((void*)&client, 0, sizeof sockaddr_in);

    host.sin_family = AF_INET;
    host.sin_addr.S_un.S_addr = htonl(INADDR_ANY);
    host.sin_port = htons(LISTEN_PORT);

    int nRet = bind(m_socketListen, (struct sockaddr*)&host, sizeof host);
    if (nRet < 0)
    {
        return E_FAIL;
    }
    
    nRet = listen(m_socketListen, MAX_LISTEN_COUNT);
    if (nRet < 0)
    {
        return E_FAIL;
    }

    while (true)
    {
        int sockAddrLen = sizeof sockaddr;
        SOCKET sClient = accept(m_socketListen, (struct sockaddr*)&client, 
            &sockAddrLen);
        if (sClient == INVALID_SOCKET)
        {
            Sleep(1000);
            int nError = WSAGetLastError();
            printf("accept error[%d]\n", nError);
            continue;
        }

        char sendContent[] = { "Hello Client" };
        int nSendLen = send(sClient, sendContent, sizeof sendContent, 0);

        if (nSendLen > 0)
        {
            printf("send finished\n");

            const UINT RECEIVE_BUFF_LEN = 20;
            char receiveBuff[RECEIVE_BUFF_LEN] = {0};
            int nRecvLen = recv(sClient, receiveBuff, RECEIVE_BUFF_LEN, 0);
            if (nRecvLen <= 0)
            {
                printf("recv failed\n");
            }
            else
            {
                printf("recv %s\n", receiveBuff);
            }
        }
        else
        {
            printf("send failed\n");
        }
        closesocket(sClient);
    }
    return S_OK;
}
