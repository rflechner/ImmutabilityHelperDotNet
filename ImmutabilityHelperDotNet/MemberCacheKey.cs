using System;

namespace ImmutabilityHelperDotNet
{
    class MemberCacheKey
    {
        public MemberCacheKey(Type modelType, string memberName)
        {
            ModelType = modelType;
            MemberName = memberName;
        }

        Type ModelType { get; }
        string MemberName { get; }

        protected bool Equals(MemberCacheKey other)
        {
            return Object.Equals(ModelType, other.ModelType) && string.Equals((string) MemberName, (string) other.MemberName);
        }

        public override bool Equals(object obj)
        {
            if (Object.ReferenceEquals(null, obj)) return false;
            if (Object.ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MemberCacheKey) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((ModelType != null ? ModelType.GetHashCode() : 0) * 397) ^ (MemberName != null ? MemberName.GetHashCode() : 0);
            }
        }
    }
}