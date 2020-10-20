using System;
using System.IO;
using System.Windows.Forms;

namespace Game
{
    public class DiskExternalContentProvider : IExternalContentProvider
    {
        public string DisplayName => "disk";

        public bool SupportsLinks => false;

        public bool SupportsListing => false;

        public bool RequiresLogin => false;

        public bool IsLoggedIn => true;

        public string Description => "No login required; Save to disk";

        public void Dispose()
        {
        }

        public void Download(string path, CancellableProgress progress, Action<Stream> success, Action<Exception> failure)
        {
            failure(new NotSupportedException());
        }

        public void Link(string path, CancellableProgress progress, Action<string> success, Action<Exception> failure)
        {
            failure(new NotSupportedException());
        }

        public void List(string path, CancellableProgress progress, Action<ExternalContentEntry> success, Action<Exception> failure)
        {
            failure(new NotSupportedException());
        }

        public void Login(CancellableProgress progress, Action success, Action<Exception> failure)
        {
            failure(new NotSupportedException());
        }

        public void Logout()
        {
            throw new NotSupportedException();
        }

        public void Upload(string path, Stream stream, CancellableProgress progress, Action<string> success, Action<Exception> failure)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Survivalcraft World|*.scworld";
            saveFileDialog1.Title = "Save an Survivalcraft World File";
            saveFileDialog1.ShowDialog();
            try
            {
                using (Stream s = saveFileDialog1.OpenFile())
                {
                    stream.CopyTo(s);
                }
                success(null);
            }
            catch (Exception e)
            {
                failure(e);
            }
        }
    }
}
