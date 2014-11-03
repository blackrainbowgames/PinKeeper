using System;
using Assets.Scripts.Common;

namespace Assets.Scripts.Views
{
    public class CardView : ViewBase
    {
        public Card Card;
        public UILabel[] Pin;
        public UILabel Comments;
        public UILabel Timer;

        private CardData _card;
        private long _timer;
        private const int TickTaskId = 800;

        public override void Open(TweenDirection direction, object meta)
        {
            if (meta == null) throw new Exception();

            var args = (ProtectedValue[]) meta;

            _card = Profile.Instance.GetCard(args[0], args[1]);

            var pin = _card.PinString;

            for (var i = 0; i < pin.Length; i++)
            {
                Pin[i].text = Convert.ToString(pin[i]);
            }

            Card.Initialize(_card.Name.String, _card.Number.Int, _card.Color.CardColor);
            Comments.SetText(_card.Comments.String);
            TaskScheduler.CreateTask(() => base.Open(direction), 0.1f);
            _timer = 10;
            Tick();
        }

        private void Tick()
        {
            if (_timer >= 0)
            {
                Timer.SetLocalizedText("%ClosingIn%\n{0}", _timer);
                TaskScheduler.CreateTask(Tick, TickTaskId, 1);
            }
            else
            {
                GetComponent<Engine>().Back();
            }

            _timer--;
        }

        protected override void Cleanup()
        {
            TaskScheduler.Kill(TickTaskId);

            foreach (var digit in Pin)
            {
                digit.text = null;
            }

            Comments.text = null;
            _card = null;
        }

        public void EditCard()
        {
            GetComponent<CardEdit>().Open(TweenDirection.Right, _card);
        }

        public void DeleteCard()
        {
            GetComponent<PatternLock>().Open(TweenDirection.Right, new Task { Type = TaskType.DeleteCard, Slot = _card.Slot });
        }
    }
}