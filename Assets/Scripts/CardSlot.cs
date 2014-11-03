using System;
using System.Runtime.Serialization;
using Assets.Scripts.Common;

namespace Assets.Scripts
{
    [Serializable]
    public class CardSlot : ISerializable
    {
        public ProtectedValue Slot;
        public ProtectedValue Data;
        public ProtectedValue Timestamp;
        public ProtectedValue LostPToken;

        public CardSlot()
        {
        }

        public CardSlot(SerializationInfo info, StreamingContext context)
        {
            foreach (var entry in info)
            {
                var value = Serializer.Deserialize<ProtectedValue>((string) entry.Value);

                switch (entry.Name)
                {
                    case "Slot":
                        Slot = value; break;
                    case "Data":
                        Data = value; break;
                    case "Timestamp":
                        Timestamp = value; break;
                }
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            AddValue(info, "Slot", Slot);
            AddValue(info, "Data", Data);
            AddValue(info, "Timestamp", Timestamp);
        }

        private static void AddValue(SerializationInfo info, string key, ProtectedValue value)
        {
            if (value != null)
            {
                info.AddValue(key, Serializer.Serialize(value));
            }
        }

        public CardSlot Copy()
        {
            return new CardSlot
            {
                Slot = Slot == null ? null : Slot.Copy(),
                Data = Data == null ? null : Data.Copy(),
                Timestamp = Timestamp == null ? null : Timestamp.Copy()
            };
        }
    }
}