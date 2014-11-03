using System;
using Assets.Scripts.Common;
using UnityEngine;

namespace Assets.Scripts.Views
{
    public enum TaskType
    {
        CreatePattern,
        ConfirmPattern,
        ChangePattern,
        CreatePatternChange,
        ConfirmPatternChange,
        LoadCards,
        CreateCard,
        OpenCard,
        DeleteCard,
        EditCard,
        ResetProfile,
        ResetCloud,
        CreateToken
    }

    public class Task
    {
        public TaskType Type;
        public ProtectedValue Slot;
        public ProtectedValue Card;
        public ProtectedValue Hash;
        public ProtectedValue Pattern;
        public ProtectedValue Token;
    }

    public abstract class LockBase : ViewBase
    {
        public UILabel Message;
        public UILabel Timer;
        public GameObject BackButton;

        protected int TaskId = 500;
        protected int LockSeconds = 20;
        protected int Attempt;
        protected Task Task;

        protected static class Colors
        {
            public static readonly Color Blue = ColorHelper.GetColor(0, 160, 255);
            public static readonly Color Green = ColorHelper.GetColor(0, 255, 0);
            public static readonly Color Red = ColorHelper.GetColor(255, 50, 0);
            public static readonly Color White = ColorHelper.GetColor(255, 255, 255);
        }

        public override void Open(TweenDirection direction, object meta)
        {
            Time.fixedDeltaTime *= 2;

            if (meta == null)
            {
                Task = new Task { Type = Profile.Instance.PatternExists() ? TaskType.LoadCards : TaskType.CreatePattern };
            }
            else
            {
                Task = (Task) meta;
            }

            Refresh();
            base.Open(direction);
        }

        public override void Open(TweenDirection direction)
        {
            Open(direction, null);
        }

        protected override void Cleanup()
        {
            Time.fixedDeltaTime /= 2;
            Task = null;
        }

        public virtual void Start()
        {
            Tick();
        }

        public TaskType TaskType
        {
            get { return Task.Type; }
        }

        private void Tick()
        {
            if (Profile.Instance.LockTime != null)
            {
                var left = (int) (Profile.Instance.LockTime.DateTime - DateTime.UtcNow).TotalSeconds;

                if (left >= 0)
                {
                    Timer.SetLocalizedText("%Locked%\n{0}", left);
                }
                else
                {
                    Timer.text = null;
                    Profile.Instance.Unlock();
                }

                Refresh();
            }

            TaskScheduler.CreateTask(Tick, 1);
        }

        protected virtual bool ProcessKey(ProtectedValue pattern)
        {
            var success = CheckSuccess(Task.Type, pattern);

            if (success)
            {
                Message.SetLocalizedText(GetActionMessage(Task.Type));
                TaskScheduler.CreateTask(() => Success(pattern), 1);
            }
            else
            {
                switch (Task.Type)
                {
                    case TaskType.CreatePattern:
                    case TaskType.CreatePatternChange:
                        Message.SetLocalizedText("%DrawMoreComplexPattern%");
                        break;
                    case TaskType.ConfirmPattern:
                        Task.Type = TaskType.CreatePattern;
                        Message.SetLocalizedText("%WrongPatternDrawNew%");
                        break;
                    case TaskType.ConfirmPatternChange:
                        Task.Type = TaskType.CreatePatternChange;
                        Message.SetLocalizedText("%WrongPatternDrawNew%");
                        break;
                    default:
                    {
                        if (Attempt < 2)
                        {
                            Attempt++;
                            Message.SetLocalizedText("%WrongPatternTryAgain%");
                        }
                        else
                        {
                            Attempt = 0;
                            Profile.Instance.Lock(DateTime.UtcNow.AddSeconds(LockSeconds));
                        }

                        break;
                    }
                }

                TaskScheduler.CreateTask(() => { enabled = true; }, TaskId, 1);
            }

            enabled = false;

            return success;
        }

        protected virtual void Success(ProtectedValue pattern)
        {
            switch (Task.Type)
            {
                case TaskType.CreatePattern:
                    Task.Type = TaskType.ConfirmPattern;
                    Task.Hash = new ProtectedValue(Md5.Encode(pattern.String));
                    enabled = true;
                    return;
                case TaskType.CreatePatternChange:
                    Task.Type = TaskType.ConfirmPatternChange;
                    Task.Hash = new ProtectedValue(Md5.Encode(pattern.String));
                    enabled = true;
                    return;
                case TaskType.ChangePattern:
                    Task.Type = TaskType.CreatePatternChange;
                    Task.Pattern = pattern;
                    enabled = true;
                    return;
                case TaskType.ConfirmPatternChange:
                    Profile.Instance.ChangePattern(Task.Pattern, pattern);
                    GetComponent<Storage>().Open(TweenDirection.Right, pattern);
                    break;
                case TaskType.ConfirmPattern:
                    Profile.Instance.CreatePattern(pattern);
                    GetComponent<Storage>().Open(TweenDirection.Right, pattern);
                    break;
                case TaskType.LoadCards:
                    GetComponent<Storage>().Open(TweenDirection.Right, pattern);
                    break;
                case TaskType.CreateCard:
                    Profile.Instance.CreateCard(Task.Slot, Task.Card, pattern);
                    GetComponent<Storage>().Open(TweenDirection.Right, pattern);
                    break;
                case TaskType.OpenCard:
                    GetComponent<CardView>().Open(TweenDirection.Right, new [] { Task.Slot, pattern });
                    break;
                case TaskType.DeleteCard:
                    Profile.Instance.DeleteCard(Task.Slot);
                    GetComponent<Storage>().Open(TweenDirection.Right, pattern);
                    break;
                case TaskType.EditCard:
                    Profile.Instance.UpdateCard(Task.Slot, Task.Card, pattern);
                    GetComponent<Storage>().Open(TweenDirection.Right, pattern);
                    break;
                case TaskType.ResetProfile:
                    Profile.Instance.Reset();
                    Open(TweenDirection.Right, new Task { Type = TaskType.CreatePattern } );
                    return;
                case TaskType.ResetCloud:
                    GetComponent<Cloud>().Reset();
                    GetComponent<Storage>().Open(TweenDirection.Right);
                    break;
                case TaskType.CreateToken:
                    Profile.Instance.CreateToken(Task.Token, pattern);
                    GetComponent<Storage>().Open(TweenDirection.Right, pattern);
                    break;
            }
        }

        protected virtual void Refresh()
        {
            Attempt = 0;

            if (Profile.Instance.LockTime == null)
            {
                switch (Task.Type)
                {
                    case TaskType.CreatePattern:
                        Message.SetLocalizedText("%CreatePattern%");
                        break;
                    case TaskType.ChangePattern:
                    case TaskType.DeleteCard:
                    case TaskType.ResetProfile:
                    case TaskType.ResetCloud:
                    case TaskType.CreateToken: 
                        Message.SetLocalizedText("%ConfirmAction%");
                        break;
                    default:
                        Message.SetLocalizedText("%DrawPattern%");
                        break;
                }
            }

            #if UNITY_IPHONE

            BackButton.SetActive(Task.Type != TaskType.CreatePattern && Task.Type != TaskType.ConfirmPattern && Task.Type != TaskType.LoadCards);

            #endif
        }

        private bool CheckSuccess(TaskType taskType, ProtectedValue pattern)
        {
            switch (taskType)
            {
                case TaskType.CreatePattern:
                case TaskType.CreatePatternChange:
                    return pattern.String.Length >= 4 && pattern.String != "0123" && pattern.String != "048C" && pattern.String != "05AF";
                case TaskType.ConfirmPattern:
                case TaskType.ConfirmPatternChange:
                    return Md5.Encode(pattern.String).Equals(Task.Hash.String, StringComparison.InvariantCultureIgnoreCase);
                case TaskType.LoadCards:
                case TaskType.CreateCard:
                case TaskType.OpenCard:
                case TaskType.DeleteCard:
                case TaskType.EditCard:
                case TaskType.ResetProfile:
                case TaskType.ResetCloud:
                case TaskType.ChangePattern:
                case TaskType.CreateToken:
                    return Profile.Instance.CheckPattern(pattern);
                default:
                    throw new Exception();
            }
        }

        private static string GetActionMessage(TaskType taskType)
        {
            switch (taskType)
            {
                case TaskType.CreatePattern:
                case TaskType.CreatePatternChange: return "%RepeatPattern%";
                case TaskType.ConfirmPattern: return "%SavingPattern%";
                case TaskType.LoadCards: return "%LoadingCards%";
                case TaskType.CreateCard: return "%SavingCard%";
                case TaskType.OpenCard: return "%OpeningCard%";
                case TaskType.DeleteCard: return "%DeletingCard%";
                case TaskType.EditCard: return "%UpdatingCard%";
                case TaskType.ChangePattern: return "%DrawNewPattern%";
                case TaskType.ConfirmPatternChange: return "%UpdatingStorage%";
                case TaskType.ResetProfile: return "%ResetingProfile%";
                case TaskType.ResetCloud: return "%ResetingCloud%";
                case TaskType.CreateToken: return "%UpdatingProfile%";
                default:
                    throw new Exception();
            }
        }
    }
}