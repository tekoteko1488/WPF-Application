using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace Diplom_Application
{
    class Repository
    {
        public static Server ConnectionToServer(string nameOfServer)
        {
            ServerConnection cnn = new ServerConnection(nameOfServer);
            cnn.Connect();
            Server server = new Server(cnn);
            return server;
        }
    }
}
