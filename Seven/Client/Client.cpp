// Client.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <WinSock2.h>
#pragma comment(lib, "ws2_32.lib")

static const USHORT SERVER_PORT = 10008;

int _tmain(int argc, _TCHAR* argv[])
{
    WSADATA wsaData;
    int nRet = WSAStartup(MAKEWORD(2, 0), &wsaData);
    if (nRet != 0)
    {
        return 0;
    }

    SOCKET sClient = socket(AF_INET, SOCK_STREAM, 0);
    if (sClient == INVALID_SOCKET)
    {
        WSACleanup();
        return 0;
    }

    sockaddr_in serverAddr;
    memset(&serverAddr, 0, sizeof serverAddr);
    serverAddr.sin_family = AF_INET;
    serverAddr.sin_port = htons(SERVER_PORT);
    serverAddr.sin_addr.S_un.S_addr = inet_addr("127.0.0.1");

    nRet = connect(sClient, (struct sockaddr*)&serverAddr, sizeof serverAddr);
    if (nRet != 0)
    {
        int nError = WSAGetLastError();
        closesocket(sClient);
        WSACleanup();
        return 0;
    }

    const UINT RECV_BUFF_LEN = 20;
    char recvBuff[RECV_BUFF_LEN] = { 0 };
    int nRecvLen = recv(sClient, recvBuff, RECV_BUFF_LEN, 0);
    if (nRecvLen < 0)
    {
        closesocket(sClient);
        WSACleanup();
        return 0;
    }
    printf("recv %s\n", recvBuff);

    char sendBuff[] = "Hello server";
    int nSendLen = send(sClient, sendBuff, sizeof sendBuff, 0);
    if (nSendLen < 0)
    {
        closesocket(sClient);
        WSACleanup();
        return 0;
    }
    printf("send %dbyte\n", nSendLen);

    closesocket(sClient);
    WSACleanup();

    system("pause");
	return 0;
}

