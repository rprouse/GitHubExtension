using Alteridem.GitHub.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alteridem.GitHub.Extension.Test.Mocks
{
    public class OptionsProviderMock: IOptionsProvider
    {
        public OptionsProviderMock()
        {
            Options = new GitHub.Model.OptionsPage();
        }

        public GitHub.Model.OptionsPage Options { get; private set; }
    }
}
