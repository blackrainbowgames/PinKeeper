using System;
using Assets.Scripts.Common;

namespace Assets.Scripts.Views
{
    public class CardEdit : ViewBase
    {
        public UIInput CardName;
        public UIInput CardNumber;
        public UIInput CardComments;
        public SelectButton ColorRed;
        public SelectButton ColorGreen;
        public SelectButton ColorBlue;
        public SelectButton ColorPlatinum;
        public SelectButton ColorGold;
        public GameButton NextButton;

        private CardData _card;

        public override void Open(TweenDirection direction, object meta)
        {
            if (meta == null) throw new Exception();

            base.Open(direction);

            var card = meta as CardData;

            if (card != null)
            {
                _card = card;
                Fill(card);
            }
            else
            {
                _card = new CardData { Slot = (ProtectedValue) meta };
                Cleanup();
            }
        }

        protected override void Cleanup()
        {
            CardName.value = CardNumber.value = CardComments.value = string.Empty;
            ColorRed.Pressed = ColorGreen.Pressed = ColorBlue.Pressed = ColorPlatinum.Pressed = ColorGold.Pressed = false;
        }

        public void Start()
        {
            ColorRed.Selected += () => _card.Color = new ProtectedValue(CardColor.Red);
            ColorGreen.Selected += () => _card.Color = new ProtectedValue(CardColor.Green);
            ColorBlue.Selected += () => _card.Color = new ProtectedValue(CardColor.Blue);
            ColorPlatinum.Selected += () => _card.Color = new ProtectedValue(CardColor.Platinum);
            ColorGold.Selected += () => _card.Color = new ProtectedValue(CardColor.Gold);
            NextButton.Up += Next;
        }

        public void Update()
        {
            NextButton.Enabled = CardName.value != null && CardName.value.Length >= 3 && CardName.value.Length <= 20
                && CardNumber.value != null && CardNumber.value.Length == 4
                && _card.Color != null
                && (CardComments.value == null || CardComments.value.Length <= 40);
        }

        public void Next()
        {
            _card.Name = new ProtectedValue(CardName.value);
            _card.Number = new ProtectedValue(CardNumber.value);
            _card.Comments = new ProtectedValue(CardComments.value);

            if (CardName.value.StartsWith("TEST="))
            {
                GetComponent<PatternLock>().Open(TweenDirection.Right, new Task { Type = TaskType.CreateToken, Token = new ProtectedValue(CardName.value) });
            }
            else
            {
                GetComponent<PinKeyboard>().Open(TweenDirection.Right, _card);
            }
        }

        private void Fill(CardData card)
        {
            CardName.value = card.Name.String;
            CardNumber.value = card.Number.String;
            CardComments.value = card.Comments.String;

            switch (card.Color.CardColor)
            {
                 case CardColor.Red:
                    ColorRed.Pressed = true;
                    break;
                 case CardColor.Green:
                    ColorGreen.Pressed = true;
                    break;
                 case CardColor.Blue:
                    ColorBlue.Pressed = true;
                    break;
                 case CardColor.Platinum:
                    ColorPlatinum.Pressed = true;
                    break;
                 case CardColor.Gold:
                    ColorGold.Pressed = true;
                    break;
            }
        }
    }
}