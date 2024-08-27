using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DashMaster.Models
{
    public class DialogDataModel : INotifyPropertyChanged
    {
        #region Data Fields and Properties

        private string resultTitle = "";
        private string resultBody = "";
        private string dialogDescription = "";
        private string clickOperationDescription = "";

        public string ResultTitle
        {
            get => resultTitle;
            set
            {
                if (resultTitle != value)
                {
                    resultTitle = value;
                    OnPropertyChanged(nameof(ResultTitle));
                }
            }
        }

        public string ResultBody
        {
            get => resultBody;
            set
            {
                if (resultBody != value)
                {
                    resultBody = value;
                    OnPropertyChanged(nameof(ResultBody));
                }
            }
        }

        public string DialogDescription
        {
            get => dialogDescription;
            set
            {
                if (dialogDescription != value)
                {
                    dialogDescription = value;
                    OnPropertyChanged(nameof(DialogDescription));
                }
            }
        }

        public string ClickOperationDescription
        {
            get => clickOperationDescription;
            set
            {
                if (clickOperationDescription != value)
                {
                    clickOperationDescription = value;
                    OnPropertyChanged(nameof(ClickOperationDescription));
                }
            }
        }

        #endregion

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
