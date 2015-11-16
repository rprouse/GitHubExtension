using Alteridem.GitHub.Extension.Interfaces;
using Alteridem.GitHub.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Alteridem.GitHub.Extension.ViewModel
{
    public class AuthViewModel : BaseGitHubViewModel
    {
        private ILoginView _view;
        private string _accessToken;
        private string _message;


        public AuthViewModel(ILoginView view)
        {
            _view = view;
        }
        public Uri AuthorizeUrl => GitHubApi.AuthorizeUrl;

        public string AccessToken
        {
            get { return _accessToken; }
            set
            {
                if (value == _accessToken) return;
                _accessToken = value;
                OnPropertyChanged();
            }
        }

        public string Message
        {
            get { return _message; }
            set
            {
                if (value == _message) return;
                _message = value;
                OnPropertyChanged();
            }
        }

        public async Task Logon(Uri oAuthReturnUri)
        {
            Message = "Logging in...";

            if (oAuthReturnUri == null)
            {
                AccessToken = string.Empty;
                _view.Close();
            }

            if (oAuthReturnUri.AbsoluteUri.Contains("code="))
            {
                AccessToken = Regex.Split(oAuthReturnUri.AbsoluteUri, "code=")[1];

                try
                {
                    // This will throw if authentication fails
                    await GitHubApi.Login(AccessToken);
                    
                    _view.Close();
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Failed to log in: ", e.Message);
                }
            }
        }
    }
}
