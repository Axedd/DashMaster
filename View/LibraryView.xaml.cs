using DashMaster.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DashMaster.View
{
    public partial class LibraryView : UserControl
    {
        public LibraryView()
        {
            InitializeComponent();
        }

        public LibraryView(LibraryViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}
