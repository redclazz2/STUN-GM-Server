using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STUN
{
	enum InterfaceTCPApplicationStatus
	{
		ConnectionUnkown = 0,
		ConnectionAccepted = 1,
		ConnectionRejected = 2,
	}

	enum InterfaceTCPMessageType
	{
		InitialStationDataReport = 0,

	}
}
