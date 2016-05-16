namespace AzureBot.Azure.Management.ResourceManagement
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    public class IsolatedService
    {
        private static AppDomain armDomain;

        static IsolatedService()
        {
            // set up separate appdomain for ARM operations (currently, there's a 
            // version conflict between the ARM and Bot Framework libraries concerning 
            // the Microsoft.Rest.ClientRuntime assembly
#if USE_PRIVATE_BIN_PATH
            // PrivateBinPath 
            // This approach requires copying the Microsoft.Rest.Runtime (v2.0.0.0) assembly
            // to the bin/debug/patch or bin/release/patch folder 
            var setupInfo = new AppDomainSetup
            {
                PrivateBinPathProbe = string.Empty,
                PrivateBinPath = AppDomain.CurrentDomain.BaseDirectory + "patch;" + AppDomain.CurrentDomain.BaseDirectory
            };

            IsolatedService.armDomain = AppDomain.CreateDomain("ResourceManager", null, setupInfo);
#else
            // AssemblyResolve event
            // This approach looks for the required version of the Microsoft.Rest.ClientRuntime assembly  
            // in the packages folder of the solution
            var setupInfo = new AppDomainSetup
            {
                ApplicationBase = AppDomain.CurrentDomain.BaseDirectory,
                PrivateBinPath = AppDomain.CurrentDomain.SetupInformation.PrivateBinPath,
                ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile
            };

            IsolatedService.armDomain = AppDomain.CreateDomain("ResourceManager", null, setupInfo);
            IsolatedService.armDomain.AssemblyResolve += ArmDomain_AssemblyResolve;
#endif
        }

        private static Assembly ArmDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            // Microsoft.Rest.ClientRuntime, Version=2.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
            if (args.Name.StartsWith("Microsoft.Rest.ClientRuntime"))
            {
                var packagesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\packages");
                var prefix = "Microsoft.Rest.ClientRuntime.*";
                var packagePath = Directory.EnumerateDirectories(packagesFolder, prefix).FirstOrDefault(p =>
                {
                    var assemblyFile = Path.Combine(p, "lib\\net45\\Microsoft.Rest.ClientRuntime.dll");
                    if (File.Exists(assemblyFile))
                    {
                        var assemblyName = AssemblyName.GetAssemblyName(assemblyFile);
                        return assemblyName.FullName == args.Name;
                    }

                    return false;
                });

                var path = Path.Combine(packagePath, "lib\\net45\\Microsoft.Rest.ClientRuntime.dll");
                return Assembly.LoadFrom(path);
            }

            return null;
        }

        // marshal the call to the remote appdomain
        public static Task<T> Marshal<T>(Func<object[], Task<T>> function, params object[] args)
        {
            var tcs = new MarshalableCompletionSource<T>();
            var remoteWorker = typeof(RemoteWorker<T>);
            var service = (RemoteWorker<T>)IsolatedService.armDomain.CreateInstanceAndUnwrap(
                remoteWorker.Assembly.FullName,
                remoteWorker.FullName,
                false,
                BindingFlags.CreateInstance,
                null,
                args,
                null,
                null);
            service.Execute(function, tcs);
            return tcs.Task;
        }
        private class RemoteWorker<T> : MarshalByRefObject
        {
            private object[] args;

            public RemoteWorker(params object[] args)
            {
                this.args = args;
            }

            public void Execute(Func<object[], Task<T>> operation, MarshalableCompletionSource<T> marshaler)
            {
                operation(args).ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        // TODO: CloudException is not serializable - generating plain exceptions for now
                        marshaler.SetException(t.Exception.InnerExceptions.Select(p => new Exception(p.Message)).ToArray());
                    }
                    else if (t.IsCanceled)
                    {
                        marshaler.SetCanceled();
                    }
                    else
                    {
                        marshaler.SetResult(t.Result);
                    }
                });
            }
        }

        private class MarshalableCompletionSource<T> : MarshalByRefObject
        {
            private readonly TaskCompletionSource<T> m_tcs = new TaskCompletionSource<T>();

            public Task<T> Task
            {
                get
                {
                    return m_tcs.Task;
                }
            }

            public void SetResult(T result)
            {
                m_tcs.SetResult(result);
            }

            public void SetException(Exception[] exception)
            {
                m_tcs.SetException(exception);
            }

            public void SetCanceled()
            {
                m_tcs.SetCanceled();
            }
        }
    }
}
