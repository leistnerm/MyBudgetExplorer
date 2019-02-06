/* 
 * Copyright 2019 Mark D. Leistner
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *   http://www.apache.org/licenses/LICENSE-2.0
 *   
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using Amazon;
using Amazon.S3;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyBudgetExplorer.Models.YNAB;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MyBudgetExplorer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.Cookie.Expiration = new TimeSpan(2, 0, 0);
                options.ExpireTimeSpan = new TimeSpan(2, 0, 0);
            })
            .AddOAuth("ynab", options =>
            {
                options.ClientId = Configuration["ynab:ClientId"];
                options.ClientSecret = Configuration["ynab:ClientSecret"];
                options.CallbackPath = new PathString("/signin-ynab");
                options.AuthorizationEndpoint = $"https://{Configuration["ynab:Domain"]}/oauth/authorize";
                options.TokenEndpoint = $"https://{Configuration["ynab:Domain"]}/oauth/token";
                options.UserInformationEndpoint = $"https://{Configuration["ynab:userinfo"]}/v1/user";
                options.SaveTokens = true;
                options.Scope.Clear();
                options.Events = new OAuthEvents
                {
                    OnCreatingTicket = async context =>
                    {
                        var api = new YnabApi(context.AccessToken);
                        var user = api.GetUser();

                        context.Identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Data.User.UserId, ClaimValueTypes.String, context.Options.ClaimsIssuer));
                    },
                    OnTicketReceived = context =>
                    {
                        context.Properties.IsPersistent = true;
                        context.Properties.ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(90);
                        return Task.FromResult(0);
                    }
                };
            });

            services.AddMvc().AddRazorPagesOptions(options =>
            {
                options.Conventions.AddAreaPageRoute("Identity", "/Login", "");
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddDefaultAWSOptions(Configuration.GetAWSOptions());
            services.AddAWSService<IAmazonS3>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment() && false)
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(options =>
                {
                    options.Run(async context =>
                    {

                        try
                        {
                            var ex = context.Features.Get<IExceptionHandlerFeature>();
                            if (ex != null)
                            {
                                var err = "<html><body>";
                                err += $"<h1>Error: {ex.Error.Message}</h1>{ex.Error.Source}<hr />{context.Request.Path}<br />";
                                err += $"QueryString: {context.Request.QueryString}<hr />";

                                if (false && context.Request.Form != null)
                                {
                                    err += "<table border=\"1\"><tr><td colspan=\"2\">Form collection:</td></tr>";
                                    foreach (var form in context.Request.Form)
                                    {
                                        err += $"<tr><td>{form.Key}</td><td>{form.Value}</td></tr>";
                                    }
                                    err += "</table><hr />";
                                }

                                var tempEx = ex.Error;
                                do
                                {
                                    err += "<table border=\"1\">";
                                    err += $"<tr><th colspan=\"2\">{tempEx.GetType().Name}</th></tr>";
                                    err += $"<tr><td>HResult</td><td>{tempEx.HResult}</td></tr>";
                                    err += $"<tr><td>Message</td><td>{tempEx.Message}</td></tr>";
                                    err += $"<tr><td>Source</td><td>{tempEx.Source}</td></tr>";
                                    err += $"<tr><td>Target Site</td><td>{tempEx.TargetSite}</td></tr>";
                                    err += $"<tr><td>Stack Trace</td><td><pre>{tempEx.StackTrace}</pre></td></tr>";
                                    if (tempEx.Data.Count > 0)
                                    {
                                        foreach (var key in tempEx.Data.Keys)
                                            err += $"<tr><td>Data: {key}</td><td><pre>{tempEx.Data[key]}</pre></td></tr>";
                                    }
                                    err += "</table><hr />";

                                    tempEx = tempEx.InnerException;
                                } while (tempEx != null);


                                err += "</body></html>";

                                using (var client = new AmazonSimpleEmailServiceClient(Configuration["AWS:AccessKey"], Configuration["AWS:SecretKey"], RegionEndpoint.USEast1))
                                {
                                    var sendRequest = new SendEmailRequest
                                    {
                                        Source = "mark@theleistners.com",
                                        Destination = new Destination
                                        {
                                            ToAddresses = new List<string> { "mark@theleistners.com" }
                                        },
                                        Message = new Message
                                        {
                                            Subject = new Content("Error: My Budget Explorer for YNAB"),
                                            Body = new Body
                                            {
                                                Html = new Content
                                                {
                                                    Charset = "UTF-8",
                                                    Data = err
                                                }
                                            }
                                        }
                                    };
                                    try
                                    {
                                        Console.WriteLine("Sending email using Amazon SES...");
                                        var response = await client.SendEmailAsync(sendRequest);
                                        Console.WriteLine("The email was sent successfully.");
                                    }
                                    catch (Exception exception)
                                    {
                                        Console.WriteLine("The email was not sent.");
                                        Console.WriteLine("Error message: " + exception.Message);

                                    }
                                }
                            }
                        }
                        catch { }
                        context.Response.Redirect("/Error?r=" +
                            System.Net.WebUtility.UrlEncode(context.Request.Path + "?" +
                                                            context.Request.QueryString));
                    });
                });
                app.UseHsts();
            }

            app.UseXRay("MyBudgetExplorer X-Ray");
            //app.Use(async (context, next) =>
            //{
            //    AWSXRayRecorder.Instance.BeginSubSegment("MyBudgetExplorer");
            //    try
            //    {
            //        await next.Invoke();
            //    }
            //    finally
            //    {
            //        AWSXRayRecorder.Instance.EndSubSegment();
            //    }
            //});
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
