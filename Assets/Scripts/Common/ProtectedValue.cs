using System;
using System.Runtime.Serialization;

namespace Assets.Scripts.Common
{
    [Serializable]
    public class ProtectedValue : ISerializable
    {
        private readonly string _protected;

        private ProtectedValue(string value)
        {
            _protected = value;
        }

        public ProtectedValue(object value)
        {
            _protected = B64R.Encode(Convert.ToString(value));
        }

        #region Types

        public int Int
        {
            get { return int.Parse(B64R.Decode(_protected)); }
        }

        public long Long
        {
            get { return long.Parse(B64R.Decode(_protected)); }
        }

        public string String
        {
            get { return B64R.Decode(_protected); }
        }

        public CardColor CardColor
        {
            get { return B64R.Decode(_protected).ToEnum<CardColor>(); }
        }

        public DateTime DateTime
        {
            get { return DateTime.Parse(B64R.Decode(_protected)); }
        }

        #endregion

        #region Serialization

        public ProtectedValue(SerializationInfo info, StreamingContext context)
        {
            _protected = info.GetValue("_protected", typeof(string)).ToString();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("_protected", _protected);
        }

        #endregion

        #region Common

        public override int GetHashCode()
        {
            return _protected != null ? _protected.GetHashCode() : 0;
        }

        public static bool operator !=(ProtectedValue a, ProtectedValue b)
        {
            return !(a == b);
        }

        public static bool operator ==(ProtectedValue a, ProtectedValue b)
        {
            if ((ReferenceEquals(null, a) && !ReferenceEquals(null, b)) || (!ReferenceEquals(null, a) && ReferenceEquals(null, b)))
            {
                return false;
            }

            if (ReferenceEquals(null, a))
            {
                return true;
            }

            return a._protected == b._protected;
        }

        public bool Equals(ProtectedValue other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;

            return obj as ProtectedValue == this;
        }

        public ProtectedValue Copy()
        {
            return new ProtectedValue(_protected);
        }

        #endregion
    }
}