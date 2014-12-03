using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alteridem.GitHub.Extension.View;
using Alteridem.GitHub.Interfaces;

namespace Alteridem.GitHub.Extension.Test.Mocks
{
    public class ErrorReporterMock : IErrorReporter
    {
        #region Implementation of IErrorReporter

        /// <summary>
        /// Shows the specified message to the user.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Show( string message )
        {
            Console.WriteLine( message );
        }

        /// <summary>
        /// Shows the specified message to the user along with the exception information.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="ex">The exception.</param>
        public void Show( string message, Exception ex )
        {
            string formatted = string.Format( "{0}\r\n\r\n{1}:\r\n{2}", message, ex.GetType().Name, ex.Message );
            Console.WriteLine( formatted );
        }

        #endregion
    }
}
