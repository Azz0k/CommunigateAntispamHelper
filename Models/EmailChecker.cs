using System;
using System.Collections.Generic;
using System.Text;

namespace CommunigateAntispamHelper.Models
{
    internal class EmailChecker
    {
        private readonly AppSettings appSettings;
        private bool _isUpdateAllowed = true;
        public bool isUpdateAllowed { get { return _isUpdateAllowed; } }
        private FileDataStore fileDataStore = new FileDataStore();
        public EmailChecker(AppSettings appSettings)
        {
            this.appSettings = appSettings;
        }
        public void DisableUpdates()
        {
            _isUpdateAllowed = false;
        }
        public void EnableUpdates()
        {
            _isUpdateAllowed = true;
        }
        public void UpdateStore(FileTypes fileType, List<string> data)
        {
            fileDataStore.UpdateStore(fileType, data);
        }
    }
}
