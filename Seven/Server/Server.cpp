// Server.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "ServiceController.h"

int _tmain(int argc, _TCHAR* argv[])
{
    CServiceController serviceController;
    if (SUCCEEDED(serviceController.Init()))
    {
        serviceController.Run();
    }
    serviceController.Uninit();
	return 0;
}

