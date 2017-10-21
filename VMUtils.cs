using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace autopsy
{
    class VMUtils
    {
        public static bool IsVirtualMachine()
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem"))
            {
                using (var items = searcher.Get())
                {
                    foreach (var item in items)
                    {
                        var keywords = new string[] { "innotek", "microsoft corporation", "virtual", "vmware", "virtualbox" };

                        var manufacturer = item["Manufacturer"].ToString().ToLowerInvariant();
                        var model = item["Model"].ToString().ToLower();

                        if (keywords.Any(s=>manufacturer.Contains(s)) || keywords.Any(s=>model.Contains(s)))
                        {
                            return true;
                        }
                        
                        var hypervisorPresentProperty = item.Properties.OfType<PropertyData>().FirstOrDefault(p => p.Name == "HypervisorPresent");

                        if ((bool?)hypervisorPresentProperty?.Value == true)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
