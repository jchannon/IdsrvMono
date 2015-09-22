using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Services.InMemory;
using IdentityServer3.Core.Models;
using System.Collections.Generic;
using System;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Extensions;
using Owin;
using Owin.Security.AesDataProtectorProvider;
using Microsoft.Owin.Security.OpenIdConnect;
using System.IO;

namespace IdSrvDemo
{
  

    public class Startup
    {
        private string baseUrl = "http://localhost:8080/";

        public void Configuration(IAppBuilder app)
        {
            var requireSSL = false;
            var idserverendpoint = "/identity";
            var serverReplyUrl = Path.Combine(baseUrl, idserverendpoint);


            // Publish the internal idserver to authenticate users
            app.Map(idserverendpoint, idServer => idServer.UseIdentityServer(
                    new IdentityServerOptions
                    {
                        SiteName = "MySite",
                        //SigningCertificate = CertificateLoader.LoadCertificate(_idServerSigningCertificateThumbprint),
                        Factory = GetFactory(),
                        RequireSsl = requireSSL,
                        PublicOrigin = baseUrl,
                        CspOptions = new CspOptions{ Enabled = false },
                        AuthenticationOptions = new AuthenticationOptions
                        {
                            CookieOptions = new CookieOptions
                            { 
                                SecureMode = requireSSL ? CookieSecureMode.Always : CookieSecureMode.SameAsRequest
                            },
                            EnableLocalLogin = true,
                            EnableSignOutPrompt = false,
                            EnablePostSignOutAutoRedirect = false,
                            IdentityProviders = ConfigureIdentityProviders
                        }                   
                    }));

            app.UseCookieAuthentication(new CookieAuthenticationOptions
                {
                    AuthenticationType = "Cookies"
                });

            // Use the internal idserver for authentication
            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
                {
                    Authority = serverReplyUrl,
                    ClientId = "myawesomeclient",
                    RedirectUri = baseUrl,
                    PostLogoutRedirectUri = baseUrl,
                    ResponseType = "id_token",
                    SignInAsAuthenticationType = "Cookies",
                    Notifications = new OpenIdConnectAuthenticationNotifications
                    {
                        SecurityTokenValidated = async n =>
                        {
//                            if (_idServerRedirectAfterLogout)
//                            {
//                                // Store extra claim in the token received so we can prove 
//                                // our identity at sign out and allow the post sign out redirect
//                                var identity = n.AuthenticationTicket.Identity;
//                                identity.AddClaim(new Claim("id_token", n.ProtocolMessage.IdToken));
//                            }
                        },
                        RedirectToIdentityProvider = async n =>
                        {
//                            if (_idServerRedirectAfterLogout)
//                            {
//                                if (n.ProtocolMessage.RequestType == OpenIdConnectRequestType.LogoutRequest)
//                                {
//                                    // Send back the id_token we captured on validation to prove we are who we said,
//                                    // so that the post logout re-direct can work....
//                                    var idTokenClaim = n.OwinContext.Authentication.User.FindFirst("id_token");
//                                    if (idTokenClaim != null)
//                                        n.ProtocolMessage.IdTokenHint = idTokenClaim.Value;
//                                }
//                            }
                        }
                    }
                });

            app.UseNancy();

            //Needed to work on Mono but will work X-Plat
            app.UseAesDataProtectorProvider();

            app.UseStageMarker(PipelineStage.MapHandler);
        }

        private void ConfigureIdentityProviders(IAppBuilder app, string signInAsType)
        {
        }

        private IdentityServerServiceFactory GetFactory()
        {
            var factory = new IdentityServerServiceFactory
            {
                //UserService = new Registration<IUserService>(typeof(SharedoUserIdentityService)),
                UserService = new Registration<IUserService>(new InMemoryUserService(GetUsers())),
                ClientStore = new Registration<IClientStore>(new InMemoryClientStore(GetClients())),
                ScopeStore = new Registration<IScopeStore>(new InMemoryScopeStore(StandardScopes.All)),
                //ViewService = new Registration<IViewService>(typeof(SharedoViewService))
            };

            // Add the user directory type in - NOT SURE WHAT THIS IS
            //factory.Register(new Registration<IUserDirectoryService>(GetDirectoryType()));

            return factory;
        }

        private System.Collections.Generic.List<InMemoryUser> GetUsers()
        {
            return new List<InMemoryUser>
            {
                new InMemoryUser
                {
                    Username = "admin",
                    Password = "password1",
                    Subject = "1"
                },
                new InMemoryUser
                {
                    Username = "user",
                    Password = "password1",
                    Subject = "2"
                }
            };
        }

        private System.Collections.Generic.IEnumerable<IdentityServer3.Core.Models.Client> GetClients()
        {
            return new[]
            {
                new Client
                {
                    Enabled = true,
                    ClientName = "My Client",
                    ClientId = "myawesomeclient",
                    Flow = Flows.Implicit,
                    RedirectUris = new List<string>{ baseUrl },
                    RequireConsent = false,
                    IdentityTokenLifetime = (int)TimeSpan.FromMinutes(5).TotalSeconds,
                    PostLogoutRedirectUris = new List<string>{ baseUrl }
                }
            };
        }
    }
}

