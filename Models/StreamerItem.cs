using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DashMaster.Models
{
    public class StreamerItem
    {
        public string Name { get; set; }
        public bool IsLive {  get; set; }
        public string StreamDuration { get; set; }

        public ICommand OpenStreamerCommand { get; set; }
    }
}
