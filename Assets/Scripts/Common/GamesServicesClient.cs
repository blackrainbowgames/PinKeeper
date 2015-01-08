using System;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using UnityEngine;

namespace Assets.Scripts.Common
{
    public class GamesServicesClient
    {
        public enum ErrorCodes
        {
            Exception,
            AuthenticationError
        }

        public static bool EnableLog = true;
        public event Action<ErrorCodes, string> Exception = (errorCode, errorMessage) => { };
        public event Action<bool> GameSaved = sucess => { };
        public event Action<bool, ISavedGameMetadata, byte[]> GameLoaded = (sucess, meta, data) => { };
        public bool Busy { get; private set; }

        private byte[] _data;
        private ISavedGameMetadata _meta;
        
        public void Load(string saveName)
        {
            if (Busy)
            {
                WriteLog("busy");
                return;
            }

            Busy = true;
            AuthorizedAction(() => OpenSavedGame(saveName));
        }

        public void Save(ISavedGameMetadata meta, byte[] data)
        {
            if (Busy)
            {
                WriteLog("busy");
                return;
            }

            Busy = true;
            AuthorizedAction(() => SaveGame(meta, data, TimeSpan.FromSeconds(0)));
        }

        public static void SignOut()
        {
            if (PlayGamesPlatform.Instance.IsAuthenticated())
            {
                PlayGamesPlatform.Instance.SignOut();
            }
        }

        private void OpenSavedGame(string filename)
        {
            WriteLog("opening: {0}...", filename);

            var savedGameClient = PlayGamesPlatform.Instance.SavedGame;
            
            savedGameClient.OpenWithAutomaticConflictResolution(filename, DataSource.ReadCacheOrNetwork, ConflictResolutionStrategy.UseOriginal, OnSavedGameOpened);
        }

        private void OnSavedGameOpened(SavedGameRequestStatus status, ISavedGameMetadata meta)
        {
            if (status == SavedGameRequestStatus.Success)
            {
                LoadGameData(_meta = meta);
            }
            else
            {
                OperationCompleted("open failed, status = " + status, () => GameLoaded(false, null, null));
            }
        }

        private void LoadGameData(ISavedGameMetadata meta)
        {
            WriteLog("loading: {0}...", meta.Filename);

            var savedGameClient = PlayGamesPlatform.Instance.SavedGame;

            savedGameClient.ReadBinaryData(meta, OnSavedGameDataRead);
        }

        private void OnSavedGameDataRead(SavedGameRequestStatus status, byte[] data)
        {
            if (status == SavedGameRequestStatus.Success)
            {
                WriteLog("status == {0}, data size in bytes: {1}", status, data.Length);
                OperationCompleted("load succeeded", () => GameLoaded(true, _meta, data));
            }
            else
            {
                OperationCompleted("load failed, status = " + status, () => GameLoaded(false, null, null));
            }
        }

        private void SaveGame(ISavedGameMetadata game, byte[] savedData, TimeSpan totalPlaytime)
        {
            WriteLog("saving...");

            var updateForMetadata = new SavedGameMetadataUpdate.Builder().WithUpdatedPlayedTime(totalPlaytime).WithUpdatedDescription("Saved game at " + DateTime.Now).Build();

            PlayGamesPlatform.Instance.SavedGame.CommitUpdate(game, updateForMetadata, savedData, OnSavedGameWritten);
        }

        private void OnSavedGameWritten(SavedGameRequestStatus status, ISavedGameMetadata game)
        {
            if (status == SavedGameRequestStatus.Success)
            {
                OperationCompleted("save succeeded", () => GameSaved(true));
            }
            else
            {
                OperationCompleted("save failed, status = " + status, () => GameSaved(false));
            }
        }

        private void OperationCompleted(string message, Action action)
        {
            WriteLog(message);
            Busy = false;
            action();
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

                var config = new PlayGamesClientConfiguration.Builder().EnableDeprecatedCloudSave().EnableSavedGames().Build();

                PlayGamesPlatform.InitializeInstance(config);
                PlayGamesPlatform.DebugLogEnabled = true;
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