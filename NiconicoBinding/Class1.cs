using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NicoNico.Net;

namespace NicoNico.Binding
{
    public class Class1
    {
        public async void Test()
        {
            var authManager = new Net.Managers.AuthenticationManager();
            var userLoginSession = await authManager.LoginUserThroughV1ApiAsync("email address", "password");
            var cookieContainer = authManager.CreateLoginCookieContainer(userLoginSession);
            authManager = new Net.Managers.AuthenticationManager(cookieContainer);
            var session = await authManager.StartUserSessionAsync();

        }
    }
}
