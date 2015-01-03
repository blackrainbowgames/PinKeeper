using System;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine;

namespace Assets.Scripts.Common
{
    class PlayGamesPlatformClient : OnStateLoadedListener
    {
        public enum ErrorCodes
        {
            Exception,
            AuthenticationError
        }

        public static bool EnableLog = true;
        public event Action<ErrorCodes, string> Exception = (errorCode, errorMessage) => { };
        public event Action<bool, int> StateSaved = (sucess, slot) => { };
        public event Action<bool, int, byte[]> StateLoaded = (sucess, slot, data) => { };
        public Func<int, byte[], byte[], byte[]> ConflictResolver = (slot, localData, serverData) => localData;
        public bool Busy { get; private set; }

        private static PlayGamesPlatform PlayGamesPlatform
        {
            get { return (PlayGamesPlatform) Social.Active; }
        }

        public void Save(byte[] data, int slot = 0)
        {
            if (Busy)
            {
                WriteLog("busy");

                return;
            }

            Busy = true;

            AuthorizedAction(() =>
            {
                WriteLog("saving data: {0} bytes", data.Length);
                PlayGamesPlatform.UpdateState(0, data, this);
            });
        }

        public void Load(int slot = 0)
        {
            if (Busy)
            {
                WriteLog("busy");

                return;
            }

            Busy = true;

            AuthorizedAction(() =>
            {
                WriteLog("loading data...");
                PlayGamesPlatform.LoadState(0, this);
            });
        }

        public void OnStateSaved(bool success, int slot)
        {
            Busy = false;
            WriteLog("state saved");
            StateSaved(success, slot);
        }

        public void OnStateLoaded(bool success, int slot, byte[] data)
        {
            Busy = false;

            WriteLog("state loaded: success = " + success);

            if (data == null)
            {
                WriteLog("state loaded: slot is empty");
            }
            else
            {
                WriteLog("state loaded: {0} bytes", data.Length);
            }

            StateLoaded(success, slot, data);
        }

        public byte[] OnStateConflict(int slot, byte[] localData, byte[] serverData)
        {
            WriteLog("resolving conflicts...");

            try
            {
                return ConflictResolver(slot, localData, serverData);
            }
            catch (Exception e)
            {
                WriteLog("resolving conflicts failed: {0}", e);
                Exception(ErrorCodes.Exception, e.Message);

                return localData;
            }
        }

        private void AuthorizedAction(Action action)
        {
            if (Social.localUser.authenticated)
            {
                action();
            }
            else
            {
                WriteLog("authentication...");
                PlayGamesPlatform.Activate();
                Social.localUser.Authenticate(authenticated =>
                {
                    if (authenticated)
                    {
                        action();
                    }
                    else
                    {
                        Exception(ErrorCodes.AuthenticationError, null);
                        WriteLog("authentication failed");
                    }
                });
            }
        }

        private void WriteLog(string message, params object[] args)
        {
            if (EnableLog)
            {
                Debug.Log(GetType().Name + ": " + string.Format(message, args));
            }
        }
    }
}