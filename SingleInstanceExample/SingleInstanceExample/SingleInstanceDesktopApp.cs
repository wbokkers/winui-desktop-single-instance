﻿#nullable enable

using System;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;

namespace SingleInstanceExample
{
    public class SingleInstanceLaunchEventArgs : EventArgs
    {
        public SingleInstanceLaunchEventArgs(string arguments, bool isFirstLaunch)
        {
            Arguments = arguments;
            IsFirstLaunch = isFirstLaunch;
        }
        public string Arguments { get; private set; } = "";
        public bool IsFirstLaunch { get; private set; }
    }

    public sealed class SingleInstanceDesktopApp : IDisposable
    {
        private readonly string _mutexName = "";
        private readonly string _pipeName = "";
        private readonly object _namedPiperServerThreadLock = new();

        private bool _isDisposed = false;
        private bool _isFirstInstance;

        private Mutex? _mutexApplication;
        private NamedPipeServerStream? _namedPipeServerStream;

        public event EventHandler<SingleInstanceLaunchEventArgs>? Launched;

        public SingleInstanceDesktopApp(string appId)
        {
            _mutexName = "MUTEX_" + appId;
            _pipeName = "PIPE_" + appId;
        }

        public void Launch(string arguments)
        {
            if (string.IsNullOrEmpty(arguments))
            {
                var argList = System.Environment.GetCommandLineArgs();
                if (argList.Length > 1)
                {
                    arguments = string.Join(' ', argList.Skip(1));
                }
            }

            if (IsFirstApplicationInstance())
            {
                CreateNamedPipeServer();
                Launched?.Invoke(this, new SingleInstanceLaunchEventArgs(arguments, isFirstLaunch: true));
            }
            else
            {
                SendArgumentsToRunningInstance(arguments);
                App.Current.Exit();
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            _namedPipeServerStream?.Dispose();
            _mutexApplication?.Dispose();
        }

        private bool IsFirstApplicationInstance()
        {
            // Allow for multiple runs but only try and get the mutex once
            if (_mutexApplication == null)
            {
                _mutexApplication = new Mutex(true, _mutexName, out _isFirstInstance);
            }

            return _isFirstInstance;
        }

        /// <summary>
        ///     Starts a new pipe server if one isn't already active.
        /// </summary>
        private void CreateNamedPipeServer()
        {
            // Create pipe and start the async connection wait
            _namedPipeServerStream = new NamedPipeServerStream(
                _pipeName,
                PipeDirection.In,
                1,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous,
                0,
                0);

            // Begin async wait for connections
            _namedPipeServerStream.BeginWaitForConnection(NamedPipeServerConnectionCallback, _namedPipeServerStream);
        }

        private void SendArgumentsToRunningInstance(string arguments)
        {
            try
            {
                using var namedPipeClientStream = new NamedPipeClientStream(".", _pipeName, PipeDirection.Out);
                namedPipeClientStream.Connect(3000); // Maximum wait 3 seconds
                using var sw = new StreamWriter(namedPipeClientStream);
                sw.Write(arguments);
                sw.Flush();
            }
            catch (Exception)
            {
                // Error connecting or sending
            }
        }

        private void NamedPipeServerConnectionCallback(IAsyncResult iAsyncResult)
        {
            try
            {
                if (_namedPipeServerStream == null)
                    return;

                // End waiting for the connection
                _namedPipeServerStream.EndWaitForConnection(iAsyncResult);

                // Read data and prevent access to _namedPipeXmlPayload during threaded operations
                lock (_namedPiperServerThreadLock)
                {
                    // Read data from client
                    using var sr = new StreamReader(_namedPipeServerStream);
                    var args = sr.ReadToEnd();
                    Launched?.Invoke(this, new SingleInstanceLaunchEventArgs(args, isFirstLaunch: false));
                }
            }
            catch (ObjectDisposedException)
            {
                // EndWaitForConnection will exception when someone calls closes the pipe before connection made
                // In that case we dont create any more pipes and just return
                // This will happen when app is closing and our pipe is closed/disposed
                return;
            }
            catch (Exception)
            {
                // ignored
            }
            finally
            {
                // Close the original pipe (we will create a new one each time)
                _namedPipeServerStream?.Dispose();
            }

            // Create a new pipe for next connection
            CreateNamedPipeServer();
        }
    }
}