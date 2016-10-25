#pragma once
#include <WinSock2.h>

class CServiceController
{
public:
    CServiceController();
    ~CServiceController();
    HRESULT Init(void);
    HRESULT Uninit(void);
    HRESULT Run(void);

private:
    SOCKET m_socketListen;

private:
    static const USHORT LISTEN_PORT = 10008;
    static const USHORT MAX_LISTEN_COUNT = 10;
};

