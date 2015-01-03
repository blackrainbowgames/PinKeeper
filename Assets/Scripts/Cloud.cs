using System;
using System.Text;
using Assets.Scripts.Common;
using Assets.Scripts.Views;
using UnityEngine;

namespace Assets.Scripts
{
    public class Cloud : Script
    {
        public UILabel SyncInfo;

        private readonly PlayGamesPlatformClient _cloud = new PlayGamesPlatformClient();
        private bool _storageChanged;
        private string _errorMessage;
        private const byte NullByte = 255;

        public void Start()
        {
            _cloud.Exception += Exception;
            _cloud.StateLoaded += StateLoaded;
            _cloud.StateSaved += StateSaved;
            _cloud.ConflictResolver = StateConflict;
        }

        public void Sync()
        {
            if (!Profile.Instance.Premium || _cloud.Busy) return;

            WriteSyncMessage("%Connecting%");
            Initialize();
            _cloud.Load();
        }

        public void Reset()
        {
            if (!Profile.Instance.Premium || _cloud.Busy) return;

            WriteSyncMessage("%Connecting%");
            Initialize();
            _cloud.Save(new[] { NullByte });
        }

        private void Initialize()
        {
            _storageChanged = false;
            _errorMessage = null;
        }

        private void Exception(PlayGamesPlatformClient.ErrorCodes errorCode, string errorMessage)
        {
            switch (errorCode)
            {
                case PlayGamesPlatformClient.ErrorCodes.AuthenticationError:
                    WriteSyncMessage("%AuthenticationError%");
                    break;
                case PlayGamesPlatformClient.ErrorCodes.Exception:
                    _errorMessage = errorMessage;
                    break;
            }
        }

        private void StateSaved(bool success, int slot)
        {
            if (!success)
            {
                WriteSyncMessage("%SaveFailed%");
            }
            else if (_errorMessage != null)
            {
                WriteSyncMessage(_errorMessage);
            }
            else
            {
                Profile.Instance.SaveSyncTime();

                if (_storageChanged)
                {
                    GetComponent<PatternLock>().Open(TweenDirection.Right);
                }

                WriteSyncMessage("%Synced%");
            }
        }

        private void StateLoaded(bool success, int slot, byte[] data)
        {
            if (!success && Profile.Instance.SyncTime != null)
            {
                WriteSyncMessage("%LoadFailed%");
            }
            else if (_errorMessage != null)
            {
                WriteSyncMessage(_errorMessage);
            }
            else if (IsEmpty(data))
            {
                if (Profile.Instance.CountCards() == 0)
                {
                    WriteSyncMessage("%NothingToSync%");
                }
                else
                {
                    _cloud.Save(Profile.Instance.Encrypt());
                }
            }
            else
            {
                try
                {
                    WriteSyncMessage("%ResolvingData%");
                    _storageChanged = Profile.Instance.Merge(Encoding.UTF8.GetString(data));

                    if (Profile.Instance.ReadyForSave)
                    {
                        _cloud.Save(Profile.Instance.Encrypt());
                    }
                    else
                    {
                        GetComponent<PatternLock>().Open(TweenDirection.Right);
                        WriteSyncMessage("%Synced%");
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                    WriteSyncMessage(e.Message);
                }
            }
        }

        private byte[] StateConflict(int slot, byte[] localData, byte[] serverData)
        {
            WriteSyncMessage("%ResolvingData%");

            if (localData != null && localData[0] == NullByte)
            {
                return localData;
            }

            if (IsEmpty(localData))
            {
                return serverData;
            }
            
            if (IsEmpty(serverData))
            {
                return localData;
            }

            return Profile.Sync.Merge(Encoding.UTF8.GetString(localData), Encoding.UTF8.GetString(serverData));
        }

        private static bool IsEmpty(byte[] data)
        {
            return data == null || (data.Length == 1 && data[0] == NullByte) || string.IsNullOrEmpty(Encoding.UTF8.GetString(data));
        }

        private void WriteSyncMessage(string message)
        {
            SyncInfo.SetLocalizedText(message);
        }
    }
}