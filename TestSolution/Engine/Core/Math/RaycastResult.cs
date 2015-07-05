namespace Core.Math
{
    public struct RaycastResult
    {  
        private readonly string _objectName;
        private readonly float _distance;

        public RaycastResult(string objectName, float distance)
        {
            _objectName = objectName;
            _distance = distance;
        }

        public string ObjectName
        {
            get { return _objectName; }
        }

        public float Distance
        {
            get { return _distance; }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is RaycastResult && Equals((RaycastResult) obj);
        }
        public bool Equals(RaycastResult other)
        {
            return string.Equals(_objectName, other._objectName) && _distance.Equals(other._distance);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                return ((_objectName != null ? _objectName.GetHashCode() : 0) * 397) ^ _distance.GetHashCode();
            }
        }

        public static bool operator==(RaycastResult a, RaycastResult b)
        {
            return a.Equals(b);
        }
        public static bool operator !=(RaycastResult a, RaycastResult b)
        {
            return !(a == b);
        }
    }
}