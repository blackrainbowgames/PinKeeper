using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Common;

namespace Assets.Scripts.Views
{
    public class PinKeyboard : ViewBase
    {
        public UILabel Title;
        public UILabel Stars;
        public GameButton[] Keys;
        public GameButton ClearButton;
        public GameButton ConfirmButton;

        private string _hash;
        private readonly List<ProtectedValue> _pin = new List<ProtectedValue>();
        private CardData _card;

        public override void Open(TweenDirection direction, object meta)
        {
            _card = (CardData) meta;

            Reset();

            if (_card.Pin != null)
            {
                foreach (var c in _card.PinString)
                {
                    PressButton(Convert.ToString(c));
                }
            }

            base.Open(direction);
        }

        public void Update()
        {
            ClearButton.Enabled = _pin.Count > 0;
            ConfirmButton.Enabled = _pin.Count == 4;
        }

        public void PressButton(string key)
        {
            if (_pin.Count < 4)
            {
                _pin.Add(new ProtectedValue(key));
                Stars.SetText(Stars.text + "*");
            }
        }

        public void Clear()
        {
            if (_pin.Count > 0)
            {
                _pin.RemoveAt(_pin.Count - 1);
                Stars.SetText(Stars.text.Substring(0, Stars.text.Length - 1));
            }
        }

        public void Confirm()
        {
            if (_hash == null)
            {
                if (_card.Pin != null && _card.PinString == Pin)
                {
                    Complete();
                }
                else
                {
                    _hash = Md5.Encode(Pin);
                    _pin.Clear();
                    Stars.text = null;
                    Title.SetLocalizedText("%ConfirmPIN%");
                }
            }
            else
            {
                if (_hash == Md5.Encode(Pin))
                {
                    Complete();
                }
                else
                {
                    Reset();
                    Title.SetLocalizedText("%WrongPIN%");
                }
            }
        }

        private void Complete()
        {
            _card.Pin = new ProtectedValue(Pin);

            var card = Profile.Instance.ProtectCard(_card);

            GetComponent<PatternLock>().Open(TweenDirection.Right, new Task
            {
                Type = Profile.Instance.CardExists(_card.Slot) ? TaskType.EditCard : TaskType.CreateCard,
                Slot = _card.Slot,
                Card = card
            });

            _card = null;
        }

        private void Reset()
        {
            _pin.Clear();
            _hash = Stars.text = null;
        }

        private string Pin
        {
            get
            {
                return string.Join(string.Empty, _pin.Select(i => i.String).ToArray());
            }
        }
    }
}