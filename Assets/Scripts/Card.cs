using System;
using Assets.Scripts.Common;
using Assets.Scripts.Views;
using UnityEngine;

namespace Assets.Scripts
{
    public class Card : MonoBehaviour
    {
        public UILabel Name;
        public UILabel Number;
        public GameButton Button;
        public UISprite Image;

        public void Initialize(PartialCardData card)
        {
            Initialize(card.Name.String, card.Number.Int, card.Color.CardColor);
            Button.Up += () => FindObjectOfType<PatternLock>().Open(TweenDirection.Right, new Task { Type = TaskType.OpenCard, Slot = card.Slot });
        }

        public void Initialize(string cardName, int number, CardColor cardColor)
        {
            Name.SetText(cardName);
            Number.SetText(string.Format("[646464]XXXX-XXXX-XXXX-[-]{0:0000}", number));

            Color color;

            switch (cardColor)
            {
                case CardColor.Red:
                    color = ColorHelper.GetColor(255, 50, 0);
                    break;
                case CardColor.Green:
                    color = ColorHelper.GetColor(0, 180, 0);
                    break;
                case CardColor.Blue:
                    color = ColorHelper.GetColor(0, 160, 255);
                    break;
                case CardColor.Platinum:
                    color = ColorHelper.GetColor(180, 180, 180);
                    break;
                case CardColor.Gold:
                    color = ColorHelper.GetColor(255, 150, 0);
                    break;
                default:
                    throw new Exception();
            }

            Image.color = color;
            color.a = 180 / 255f;
            Button.Color = Button.GetComponent<UIBasicSprite>().color = color;
        }
    }
}