using System;
using System.Collections.Generic;
using System.Text;

namespace FtpHelper
{
    public interface IMaintenanceTask
    {
        bool Execute();
    }
}
