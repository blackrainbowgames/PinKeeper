using System;
using System.Runtime.Serialization;
using Assets.Scripts.Common;

namespace Assets.Scripts
{
    public class PartialCardData
    {
        public ProtectedValue Slot;
        public ProtectedValue Name;
        public ProtectedValue Number;
        public ProtectedValue Color;
    }

    [Serializable]
    public class CardData : PartialCardData, ISerializable
    {
        public ProtectedValue Comments;
        public ProtectedValue Pin;

        public CardData()
        {
        }

        public CardData(SerializationInfo info, StreamingContext context)
        {
            foreach (var entry in info)
            {
                var value = Serializer.Deserialize<ProtectedValue>((string) entry.Value);

                switch (entry.Name)
                {
                    case "Slot":
                        Slot = value;
                        break;
                    case "Name":
                        Name = value;
                        break;
                    case "Number":
                        Number = value;
                        break;
                    case "Color":
                        Color = value;
                        break;
                    case "Comments":
                        Comments = value;
                        break;
                    case "Pin":
                        Pin = value;
                        break;
                }
            }
        }

        public string PinString
        {
            get { return string.Format("{0:0000}", Pin.Int); }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            AddValue(info, "Slot", Slot);
            AddValue(info, "Name", Name);
            AddValue(info, "Number", Number);
            AddValue(info, "Color", Color);
            AddValue(info, "Comments", Comments);
            AddValue(info, "Pin", Pin);
        }

        private static void AddValue(SerializationInfo info, string key, ProtectedValue value)
        {
            if (value != null)
            {
                info.AddValue(key, Serializer.Serialize(value));
            }
        }
    }
}