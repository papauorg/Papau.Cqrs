namespace Papau.Cqrs.Domain 
{
    public abstract class ValueObject<TValue> where TValue : ValueObject<TValue>
    {
        public override bool Equals(object obj)
        {
            var compareObject = obj as TValue;

            if (ReferenceEquals(compareObject, null))
                return false;
            
            if (this.GetType() != obj.GetType())
                return false;

            return IsEqual(compareObject);
        }

        public override int GetHashCode()
        {
            return GetHashCodeMandatory();
        }

        public static bool operator ==(ValueObject<TValue> a, ValueObject<TValue> b)
        {
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
                return true;

            return a.Equals(b);
        }

        public static bool operator !=(ValueObject<TValue> a, ValueObject<TValue> b)
        {
            return !(a == b);
        }

        protected abstract int GetHashCodeMandatory();
        protected abstract bool IsEqual(TValue other);
    }
}